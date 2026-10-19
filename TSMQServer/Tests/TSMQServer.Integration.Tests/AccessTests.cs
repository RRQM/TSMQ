using System.Text;
using Microsoft.Extensions.DependencyInjection;
using TORM;
using TSMQServer.Core;
using TSMQServer.Core.Imp.Services;
using TSMQServer.Db.Sqlite;
using TSMQServer.Db.Sqlite.Entities;

namespace TSMQServer.Integration.Tests;

public sealed class AccessTests(ServerFixture server) : IClassFixture<ServerFixture>
{
    [Theory]
    [InlineData(false, false)] [InlineData(false, true)] [InlineData(true, false)] [InlineData(true, true)]
    public async Task AuthenticationRejectsWrongPasswordOnBothTransports(bool ws, bool v5)
    {
        await using var wire = await MqttWire.Open(ws ? server.WsPort : server.MqttPort, ws);
        var ack = await wire.Connect(Guid.NewGuid().ToString("N"), password: "wrong", v5: v5);
        Assert.Equal(0x20, ack[0]); Assert.Equal(v5 ? 135 : 5, ack[3]);
    }

    [Theory]
    [InlineData(false, false)] [InlineData(true, false)] [InlineData(false, true)] [InlineData(true, true)]
    public async Task ValidCredentialsCanSubscribeAndReceiveDebugMessage(bool ws, bool v5)
    {
        await using var wire = await MqttWire.Open(ws ? server.WsPort : server.MqttPort, ws);
        var ack = await wire.Connect(Guid.NewGuid().ToString("N"), v5: v5); Assert.Equal(0, ack[3]);
        Assert.Equal(0x90, (await wire.Subscribe("test/debug", v5))[0]);
        var result = await server.Post("IMqttAccessService/PublishMqttMessage", new { LoginedToken = server.AdminToken, Topic = "test/debug", Payload = "hello", Qos = 0 });
        Assert.Equal("Success", result["Code"]!.GetValue<string>());
        var message = await wire.Receive(); Assert.Equal(0x30, message[0] & 0xf0);
        Assert.EndsWith("hello", Encoding.UTF8.GetString(message));
    }

    [Fact]
    public async Task WebSocketAcceptsConnectWithSplitRemainingLengthHeader()
    {
        await using var wire = await MqttWire.Open(server.WsPort, true);
        Assert.Equal(0, (await wire.Connect(new string('x', 180), splitHeader: true))[3]);
        Assert.Equal(0, (await wire.Subscribe("test/fragment"))[^1]);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task DuplicateClientIdCannotTakeOverAnActiveSession(bool ws)
    {
        var id = Guid.NewGuid().ToString("N");
        await using var first = await MqttWire.Open(server.MqttPort);
        Assert.Equal(0, (await first.Connect(id))[3]);
        await using var duplicate = await MqttWire.Open(ws ? server.WsPort : server.MqttPort, ws);
        Assert.Equal(5, (await duplicate.Connect(id))[3]);
        Assert.Equal(0, (await first.Subscribe("test/original"))[^1]);
    }

    [Fact]
    public async Task CredentialUpdateRevokesTcpAndWebSocketAndRejectsOldPassword()
    {
        await using var tcp = await MqttWire.Open(server.MqttPort);
        await using var ws = await MqttWire.Open(server.WsPort, true);
        Assert.Equal(0, (await tcp.Connect(Guid.NewGuid().ToString("N")))[3]);
        Assert.Equal(0, (await ws.Connect(Guid.NewGuid().ToString("N")))[3]);
        try
        {
            Assert.Equal("Success", (await server.Post("IMqttServerManagerService/UpdateMqttAuthUser",
                new { LoginedToken = server.AdminToken, UserName = "device", Password = "new-secret" }))["Code"]!.GetValue<string>());
            await Assert.ThrowsAnyAsync<Exception>(() => tcp.Receive());
            await Assert.ThrowsAnyAsync<Exception>(() => ws.Receive());
            await using var old = await MqttWire.Open(server.MqttPort);
            Assert.Equal(5, (await old.Connect(Guid.NewGuid().ToString("N")))[3]);
            await using var current = await MqttWire.Open(server.MqttPort);
            Assert.Equal(0, (await current.Connect(Guid.NewGuid().ToString("N"), password: "new-secret"))[3]);
        }
        finally
        {
            await server.Post("IMqttServerManagerService/UpdateMqttAuthUser",
                new { LoginedToken = server.AdminToken, UserName = "device", Password = "secret" });
        }
    }

    [Fact]
    public void ExplicitDenyOverridesAllowIncludingOverlappingSubscription()
    {
        var policy = new MqttAccessPolicy { AclEnabled = true, Rules = [
            new() { TopicFilter = "devices/#", Action = "Both", Allow = true },
            new() { TopicFilter = "devices/private/#", Action = "Both", Allow = false }] };
        Assert.True(MqttTopicAccess.IsAllowed(policy, "device", "id", "devices/public/data", "Publish"));
        Assert.False(MqttTopicAccess.IsAllowed(policy, "device", "id", "devices/private/data", "Publish"));
        Assert.False(MqttTopicAccess.IsAllowed(policy, "device", "id", "devices/#", "Subscribe"));
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task PolicyUpdateDuringSubscriptionTrafficLeavesNoStaleSubscription(bool ws)
    {
        var access = server.Services.GetRequiredService<MqttAccessManager>();
        var original = access.GetPolicy();
        var topic = "race/" + Guid.NewGuid().ToString("N");
        await using var wire = await MqttWire.Open(ws ? server.WsPort : server.MqttPort, ws);
        Assert.Equal(0, (await wire.Connect(Guid.NewGuid().ToString("N")))[3]);
        Assert.Equal(0, (await wire.Subscribe(topic))[^1]);
        var traffic = Task.Run(async () =>
        {
            try { for (var i = 0; i < 100; i++) await wire.Subscribe(topic); }
            catch (Exception ex) when (ex is IOException or System.Net.WebSockets.WebSocketException or OperationCanceledException or ObjectDisposedException) { }
        }, TestContext.Current.CancellationToken);
        try
        {
            await access.SaveAsync(new MqttAccessPolicy { AclEnabled = true });
            await traffic.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            Assert.DoesNotContain(server.Services.GetRequiredService<IMqttServerHostService>().GetTopicList(), t => t.Topic == topic && t.SubscriberCount > 0);
            await using var reconnected = await MqttWire.Open(ws ? server.WsPort : server.MqttPort, ws);
            Assert.Equal(0, (await reconnected.Connect(Guid.NewGuid().ToString("N")))[3]);
            Assert.Equal(128, (await reconnected.Subscribe(topic))[^1]);
        }
        finally { await access.SaveAsync(original); }
    }

    [Fact]
    public async Task PasswordChangeInvalidatesEveryExistingLoginSession()
    {
        var account = "password_" + Guid.NewGuid().ToString("N");
        await server.Post("IUserManagerService/Register", new { LoginedToken = server.AdminToken, Account = account, Password = "old-password" });
        var tokens = new List<string>();
        for (var i = 0; i < 2; i++)
            tokens.Add((await server.Post("IUserManagerService/Login", new { Account = account, Password = "old-password" }))["LoginedToken"]!.GetValue<string>());
        Assert.Equal("Success", (await server.Post("IUserManagerService/ChangePassword", new { LoginedToken = tokens[0], OldPassword = "old-password", NewPassword = "new-password" }))["Code"]!.GetValue<string>());
        foreach (var token in tokens)
            Assert.Equal("Unauthorized", (await server.Post("IUserManagerService/GetUserInfo", new { LoginedToken = token }))["Code"]!.GetValue<string>());
        Assert.Equal("Fail", (await server.Post("IUserManagerService/Login", new { Account = account, Password = "old-password" }))["Code"]!.GetValue<string>());
        Assert.Equal("Success", (await server.Post("IUserManagerService/Login", new { Account = account, Password = "new-password" }))["Code"]!.GetValue<string>());
    }

    [Fact]
    public async Task AclBlocksWildcardEscalationPublishAndWillAndSurvivesReload()
    {
        var access = server.Services.GetRequiredService<MqttAccessManager>();
        var original = access.GetPolicy();
        try
        {
            await access.SaveAsync(new MqttAccessPolicy { AuthenticationEnabled = true, AclEnabled = true, CapturePayload = true,
                Rules = [new() { UserName = "device", TopicFilter = "devices/+/data", Action = "Both" }] });
            using (var scope = server.Services.CreateScope())
                Assert.True((await scope.ServiceProvider.GetRequiredService<TSMQServer.Db.IMqttAccessPolicyFromDB>().GetAsync()).AclEnabled);
            foreach (var ws in new[] { false, true })
            {
                await using var wire = await MqttWire.Open(ws ? server.WsPort : server.MqttPort, ws);
                Assert.Equal(0, (await wire.Connect(Guid.NewGuid().ToString("N")))[3]);
                Assert.Equal(128, (await wire.Subscribe("#"))[^1]);
                Assert.Equal(0, (await wire.Subscribe("devices/one/data"))[^1]);
                await wire.Publish("devices/one/data", Encoding.UTF8.GetBytes("allowed"));
                Assert.EndsWith("allowed", Encoding.UTF8.GetString(await wire.Receive()));
                await wire.Publish("private/data", Encoding.UTF8.GetBytes("denied"));
                await Assert.ThrowsAnyAsync<Exception>(() => wire.Receive());
                await using var will = await MqttWire.Open(ws ? server.WsPort : server.MqttPort, ws);
                Assert.Equal(5, (await will.Connect(Guid.NewGuid().ToString("N"), willTopic: "private/will"))[3]);
            }
            var events = server.Services.GetRequiredService<MqttDiagnostics>().Query(new GetMqttDiagnosticsRequest { Limit = 200 }).Events;
            Assert.Contains(events, e => e.Kind == "AccessDenied");
            Assert.Contains(events, e => e.Kind == "Message" && e.PayloadBase64 == Convert.ToBase64String(Encoding.UTF8.GetBytes("allowed")));
            Assert.DoesNotContain(events, e => e.Kind == "Message" && e.Topic == "private/data");
        }
        finally { await access.SaveAsync(original); }
    }

    [Fact]
    public async Task AuthenticationSwitchPersistsAndPolicyChangesRevokeConnections()
    {
        var access = server.Services.GetRequiredService<MqttAccessManager>();
        var original = access.GetPolicy();
        try
        {
            await using var existing = await MqttWire.Open(server.MqttPort);
            Assert.Equal(0, (await existing.Connect("revoke-test"))[3]);
            await access.SaveAsync(new MqttAccessPolicy { AuthenticationEnabled = false });
            await Assert.ThrowsAnyAsync<Exception>(() => existing.Receive());
            await using var anonymous = await MqttWire.Open(server.MqttPort);
            Assert.Equal(0, (await anonymous.Connect("anonymous", "", ""))[3]);
        }
        finally { await access.SaveAsync(original); }
    }

    [Fact]
    public async Task CurrentRolesAndSessionsApplyImmediatelyAndResponsesHidePasswords()
    {
        var account = "user_" + Guid.NewGuid().ToString("N");
        await server.Post("IUserManagerService/Register", new { LoginedToken = server.AdminToken, Account = account, Password = "password" });
        var token = (await server.Post("IUserManagerService/Login", new { Account = account, Password = "password" }))["LoginedToken"]!.GetValue<string>();
        var user = await server.Post("IUserManagerService/GetUserInfo", new { LoginedToken = token });
        Assert.Null(user["User"]!["Password"]);
        var id = user["User"]!["Id"]!.GetValue<long>();
        Assert.Equal("Forbidden", (await server.Post("IUserManagerService/DeleteUser", new { LoginedToken = token, UserId = id }))["Code"]!.GetValue<string>());
        using var scope = server.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrmClient>();
        var role = await scope.ServiceProvider.GetRequiredService<TSMQServer.Db.IRoleManagerFromDB>().CreateRoleAsync(new RoleInfo { RoleName = account, RoleCode = account, IsEnabled = true });
        await scope.ServiceProvider.GetRequiredService<TSMQServer.Db.IRpcAuthorizationFromDB>().SetRolesForTargetAsync("MqttAccessService.GetMqttDiagnostics", [account], "测试");
        await scope.ServiceProvider.GetRequiredService<TSMQServer.Db.IUserManagerFromDB>().AssignUserRolesAsync(id, role.Id.ToString());
        Assert.Equal("Success", (await server.Post("IMqttAccessService/GetMqttDiagnostics", new { LoginedToken = token }))["Code"]!.GetValue<string>());
        role.IsEnabled = false;
        await scope.ServiceProvider.GetRequiredService<TSMQServer.Db.IRoleManagerFromDB>().UpdateRoleAsync(role);
        Assert.Equal("Forbidden", (await server.Post("IMqttAccessService/GetMqttDiagnostics", new { LoginedToken = token }))["Code"]!.GetValue<string>());
        await server.Post("IUserManagerService/Logout", new { LoginedToken = token });
        Assert.Equal("Unauthorized", (await server.Post("IUserManagerService/GetUserInfo", new { LoginedToken = token }))["Code"]!.GetValue<string>());
        await scope.ServiceProvider.GetRequiredService<TSMQServer.Db.IRpcAuthorizationFromDB>().RemoveRolesForTargetAsync("MqttAccessService.GetMqttDiagnostics");
    }

    [Theory]
    [InlineData("devices/#", "devices", true)]
    [InlineData("devices/+/data", "devices/one/data", true)]
    [InlineData("devices/+/data", "devices/#", false)]
    [InlineData("#", "$SYS/info", false)]
    [InlineData("devices/+", "devices/#", false)]
    public void TopicCoveragePreventsBroaderSubscriptions(string allowed, string requested, bool expected) =>
        Assert.Equal(expected, MqttTopicAccess.Covers(allowed, requested));
}
