using System.Text;
using Microsoft.Extensions.DependencyInjection;
using TSMQServer.Core;
using TSMQServer.Core.Imp.Services;
using TSMQServer.Db;

namespace TSMQServer.Integration.Tests;

public sealed class PersistenceTests(ServerFixture server) : IClassFixture<ServerFixture>
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ConfiguredTopicsPersistFullBinaryPayloadOnBothTransports(bool websocket)
    {
        var manager = server.Services.GetRequiredService<MqttPersistenceManager>();
        var original = manager.GetPolicy();
        var prefix = Guid.NewGuid().ToString("N");
        var payload = Enumerable.Range(0, 8192).Select(i => (byte)i).ToArray();
        try
        {
            var saved = await server.Post("IMqttPersistenceService/SavePolicy", new
            {
                LoginedToken = server.AdminToken,
                Policy = new { Enabled = true, TopicFilters = new[] { prefix + "/+/data", prefix + "/#" }, RetentionDays = 7 }
            });
            Assert.Equal("Success", saved["Code"]!.GetValue<string>());
            using (var scope = server.Services.CreateScope())
                Assert.Equal(7, (await scope.ServiceProvider.GetRequiredService<IMqttMessageStore>().GetPolicyAsync()).RetentionDays);
            await using var wire = await MqttWire.Open(websocket ? server.WsPort : server.MqttPort, websocket);
            var clientId = Guid.NewGuid().ToString("N");
            Assert.Equal(0, (await wire.Connect(clientId))[3]);
            Assert.Equal(0, (await wire.Subscribe(prefix + "/#"))[^1]);
            await wire.Publish(prefix + "/one/data", payload);
            await wire.Receive();
            QueryMqttMessagesResponse response = new();
            for (var i = 0; i < 100; i++)
            {
                response = await manager.QueryAsync(new() { TopicFilter = prefix + "/#" });
                if (response.Messages.Count > 0) break;
                await Task.Delay(20, TestContext.Current.CancellationToken);
            }
            var message = Assert.Single(response.Messages);
            Assert.Equal(payload, Convert.FromBase64String(message.PayloadBase64));
            Assert.Equal(payload.Length, message.PayloadLength);
            Assert.Equal(clientId, message.ClientId);
            Assert.Equal("device", message.UserName);
            Assert.Equal(DateTimeKind.Utc, message.Timestamp.Kind);
            await wire.Publish("unmatched/" + prefix, Encoding.UTF8.GetBytes("ignored"));
            Assert.Empty((await manager.QueryAsync(new() { TopicFilter = "unmatched/" + prefix })).Messages);
            var queried = await server.Post("IMqttPersistenceService/QueryMessages", new { LoginedToken = server.AdminToken, TopicFilter = prefix + "/+/data" });
            Assert.Equal("Success", queried["Code"]!.GetValue<string>());
        }
        finally { await manager.SaveAsync(original); }
    }

    [Fact]
    public async Task QueryPaginationFiltersAndCleanupWorkWithPersistedRows()
    {
        var manager = server.Services.GetRequiredService<MqttPersistenceManager>();
        var original = manager.GetPolicy();
        var topic = "history/" + Guid.NewGuid().ToString("N");
        using var scope = server.Services.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IMqttMessageStore>();
        try
        {
            await manager.SaveAsync(new() { RetentionDays = 1 });
            foreach (var time in new[] { DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddHours(-2), DateTime.UtcNow.AddHours(-1) })
                await store.AppendAsync(new() { Topic = topic, Timestamp = time, ClientId = "history-client", PayloadBase64 = "AP8=" });
            var first = await manager.QueryAsync(new() { TopicFilter = topic, ClientId = "history-client", Limit = 1 });
            Assert.Single(first.Messages);
            Assert.True(first.HasMore);
            var next = await manager.QueryAsync(new() { TopicFilter = topic, BeforeId = first.Messages[0].Id, Limit = 1 });
            Assert.Single(next.Messages);
            Assert.True(next.HasMore);
            Assert.True(next.Messages[0].Id < first.Messages[0].Id);
            Assert.Empty((await manager.QueryAsync(new() { TopicFilter = topic, ClientId = "missing" })).Messages);
            Assert.Single((await manager.QueryAsync(new() { TopicFilter = topic, StartTime = DateTime.UtcNow.AddMinutes(-90) })).Messages);
            await manager.CleanupAsync();
            Assert.Equal(2, (await manager.QueryAsync(new() { TopicFilter = topic })).Messages.Count);
            await manager.RecordAsync(topic, new byte[] { 1 }, 0, false, "disabled", "");
            Assert.Equal(2, (await manager.QueryAsync(new() { TopicFilter = topic })).Messages.Count);
            var invalid = await server.Post("IMqttPersistenceService/SavePolicy", new
            {
                LoginedToken = server.AdminToken, Policy = new { Enabled = true, TopicFilters = new[] { "bad/#/topic" }, RetentionDays = 0 }
            });
            Assert.Equal("Fail", invalid["Code"]!.GetValue<string>());
            Assert.Equal("Unauthorized", (await server.Post("IMqttPersistenceService/QueryMessages", new { })) ["Code"]!.GetValue<string>());
        }
        finally { await manager.SaveAsync(original); }
    }

    [Fact]
    public async Task PolicyAndPayloadSurviveReopeningDatabase()
    {
        var path = Path.Combine(Path.GetTempPath(), "tsmq-persistence-" + Guid.NewGuid().ToString("N") + ".db");
        var connectionString = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder { DataSource = path, Pooling = false }.ToString();
        try
        {
            var store = new TSMQServer.Db.Sqlite.SqliteMqttMessageStore(connectionString);
            store.Initialize();
            await store.SavePolicyAsync(new() { Enabled = true, TopicFilters = ["devices/#"], RetentionDays = 42 });
            await store.AppendAsync(new() { Topic = "devices/a", Timestamp = DateTime.UtcNow, PayloadBase64 = "AP8=", Retain = true, Qos = 2 });
            var reopened = new TSMQServer.Db.Sqlite.SqliteMqttMessageStore(connectionString);
            reopened.Initialize();
            var policy = await reopened.GetPolicyAsync();
            Assert.True(policy.Enabled);
            Assert.Equal(42, policy.RetentionDays);
            Assert.Equal("devices/#", Assert.Single(policy.TopicFilters));
            var message = Assert.Single((await reopened.QueryAsync(new() { TopicFilter = "devices/#" })).Messages);
            Assert.Equal("AP8=", message.PayloadBase64);
            Assert.True(message.Retain);
            Assert.Equal(2, message.Qos);
            Assert.Equal(2, message.PayloadLength);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task ConsolePublishesPersistOnceWithoutSubscribers()
    {
        var manager = server.Services.GetRequiredService<MqttPersistenceManager>();
        var original = manager.GetPolicy();
        var topic = "console/" + Guid.NewGuid().ToString("N");
        try
        {
            await manager.SaveAsync(new() { Enabled = true, TopicFilters = [topic] });
            var result = await server.Post("IMqttAccessService/PublishMqttMessage", new
            {
                LoginedToken = server.AdminToken, Topic = topic, Payload = "AP8=", IsBase64 = true, Qos = 0
            });
            Assert.Equal("Success", result["Code"]!.GetValue<string>());
            var message = Assert.Single((await manager.QueryAsync(new() { TopicFilter = topic })).Messages);
            Assert.Equal("AP8=", message.PayloadBase64);
            Assert.Equal("[console]", message.ClientId);
        }
        finally { await manager.SaveAsync(original); }
    }
}
