// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using TSMQServer.Core;
using TSMQServer.Core.Clients;

namespace TSMQClient.Remote.Module.Tests;

/// <summary>
/// <see cref="IRemoteService"/> 单元测试基类，包含所有共享测试逻辑。
/// 每次测试运行前通过 admin 预创建独立的测试角色和测试用户，确保各测试互不干扰。
/// </summary>
public abstract class RemoteServiceTestsBase : IAsyncLifetime
{
    private RoleInfo m_testRole = null!;
    private long m_testUserId;
    private const string TestRpcRoleDescription = "单元测试授权";

    /// <summary>
    /// 获取被测试的远程服务实例。
    /// </summary>
    protected abstract IRemoteService Service { get; }

    /// <summary>
    /// 获取当前测试运行专用的角色信息（由 admin 在初始化阶段创建）。
    /// </summary>
    protected RoleInfo TestRole => this.m_testRole;

    /// <summary>
    /// 获取当前测试运行专用的用户账号（由 admin 在初始化阶段注册）。
    /// </summary>
    protected string TestUserAccount { get; private set; } = null!;

    [Fact]
    public async Task CreateRoleAsync_成功时返回RoleInfo()
    {
        await this.LoginAsAdminAsync();

        var roleName = $"role_{Guid.NewGuid():N}";
        var roleCode = $"code_{Guid.NewGuid():N}";
        var result = await this.Service.CreateRoleAsync(roleName, roleCode, "测试角色", TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(roleName, result.Value.RoleName);

        await this.Service.DeleteRoleAsync(result.Value.Id, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DeleteRoleAsync_成功时返回Success()
    {
        await this.LoginAsAdminAsync();

        var createResult = await this.Service.CreateRoleAsync($"role_{Guid.NewGuid():N}", $"code_{Guid.NewGuid():N}", "待删除", TestContext.Current.CancellationToken);
        Assert.True(createResult.IsSuccess);

        var result = await this.Service.DeleteRoleAsync(createResult.Value.Id, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DeleteUserAsync_成功时返回Success()
    {
        await this.LoginAsAdminAsync();

        var account = $"test_{Guid.NewGuid():N}";
        var registerResult = await this.Service.RegisterAsync(account, "123456", TestContext.Current.CancellationToken);
        Assert.True(registerResult.IsSuccess);

        var userListResult = await this.Service.GetUserListAsync(TestContext.Current.CancellationToken);
        Assert.True(userListResult.IsSuccess);

        var targetUser = userListResult.Value.First(u => u.Account == account);

        var result = await this.Service.DeleteUserAsync(targetUser.Id, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await this.TearDownTestEnvironmentAsync();
        await this.DisposeServiceAsync();
    }

    [Fact]
    public async Task GetAllRpcMethodsAsync_成功时返回方法列表()
    {
        await this.LoginAsAdminAsync();

        var result = await this.Service.GetAllRpcMethodsAsync(TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value);
    }

    [Fact]
    public async Task GetAllRpcRolesAsync_成功时返回配置列表()
    {
        await this.LoginAsAdminAsync();

        var result = await this.Service.GetAllRpcRolesAsync(TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    [Fact]
    public async Task GetLogListAsync_成功时返回日志列表()
    {
        await this.LoginAsAdminAsync();

        var result = await this.Service.GetLogListAsync(new GetLogListRequest
        {
            Take = 10
        }, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    [Fact]
    public async Task GetRoleListAsync_成功时返回列表()
    {
        await this.LoginAsAdminAsync();

        var result = await this.Service.GetRoleListAsync(TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    [Fact]
    public async Task GetRpcRolesAsync_设置后能获取到角色()
    {
        await this.LoginAsAdminAsync();

        var context = await this.PrepareRpcAuthorizationTestContextAsync(TestContext.Current.CancellationToken);
        try
        {
            var setResult = await this.Service.SetRpcRolesAsync(context.TargetKey, [this.TestRole.RoleCode!], cancellationToken: TestContext.Current.CancellationToken);
            Assert.True(setResult.IsSuccess);

            var result = await this.Service.GetRpcRolesAsync(context.TargetKey, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Contains(this.TestRole.RoleCode, result.Value);
        }
        finally
        {
            await this.RestoreRpcAuthorizationAsync(context, TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task GetUserInfoAsync_成功时返回用户信息()
    {
        await this.LoginAsAdminAsync();

        var result = await this.Service.GetUserInfoAsync(TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("admin", result.Value.Account);
    }

    [Fact]
    public async Task GetUserListAsync_成功时返回用户列表()
    {
        await this.LoginAsAdminAsync();

        var result = await this.Service.GetUserListAsync(TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    /// <inheritdoc/>
    public async ValueTask InitializeAsync()
    {
        await this.InitializeServiceAsync();
        await this.SetupTestEnvironmentAsync();
    }

    [Fact]
    public async Task LoginAsync_成功时返回Success并保存Token()
    {
        var result = await this.Service.LoginAsync(this.TestUserAccount, "Test@123456", TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(this.Service.LoginedToken);
    }

    [Fact]
    public async Task LoginAsync_密码错误时返回Fail()
    {
        var result = await this.Service.LoginAsync("admin", "wrong_password", TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task RegisterAsync_成功时返回Success()
    {
        await this.LoginAsAdminAsync();
        var account = $"test_{Guid.NewGuid():N}";
        var result = await this.Service.RegisterAsync(account, "123456", TestContext.Current.CancellationToken);
        Assert.True(result.IsSuccess);

        var userListResult = await this.Service.GetUserListAsync(TestContext.Current.CancellationToken);
        if (userListResult.IsSuccess)
        {
            var user = userListResult.Value.FirstOrDefault(u => u.Account == account);
            if (user != null)
                await this.Service.DeleteUserAsync(user.Id, TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task RemoveRpcRolesAsync_成功时返回Success()
    {
        await this.LoginAsAdminAsync();

        var context = await this.PrepareRpcAuthorizationTestContextAsync(TestContext.Current.CancellationToken);
        try
        {
            var setResult = await this.Service.SetRpcRolesAsync(context.TargetKey, [this.TestRole.RoleCode!], cancellationToken: TestContext.Current.CancellationToken);
            Assert.True(setResult.IsSuccess);

            var result = await this.Service.RemoveRpcRolesAsync(context.TargetKey, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
        }
        finally
        {
            await this.RestoreRpcAuthorizationAsync(context, TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task SetRpcRolesAsync_成功时返回Success()
    {
        await this.LoginAsAdminAsync();

        var context = await this.PrepareRpcAuthorizationTestContextAsync(TestContext.Current.CancellationToken);
        try
        {
            var result = await this.Service.SetRpcRolesAsync(context.TargetKey, [this.TestRole.RoleCode!], TestRpcRoleDescription, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
        }
        finally
        {
            await this.RestoreRpcAuthorizationAsync(context, TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task UpdateRoleAsync_成功时返回Success()
    {
        await this.LoginAsAdminAsync();

        var createResult = await this.Service.CreateRoleAsync($"role_{Guid.NewGuid():N}", $"code_{Guid.NewGuid():N}", "描述", TestContext.Current.CancellationToken);
        Assert.True(createResult.IsSuccess);

        var result = await this.Service.UpdateRoleAsync(createResult.Value.Id, "新名称", "新描述", true, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);

        await this.Service.DeleteRoleAsync(createResult.Value.Id, TestContext.Current.CancellationToken);
    }

    // =====================================================================
    // MqttClientManager
    // =====================================================================

    [Fact]
    public async Task GetMqttClientListAsync_成功时返回客户端列表()
    {
        await this.LoginAsAdminAsync();

        var result = await this.Service.GetMqttClientListAsync(TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    [Fact]
    public async Task KickMqttClientsAsync_传入空列表时返回Success()
    {
        await this.LoginAsAdminAsync();

        var result = await this.Service.KickMqttClientsAsync([], TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
    }

    // =====================================================================
    // MqttServerManager
    // =====================================================================

    [Fact]
    public async Task GetMqttServerListAsync_成功时返回服务器列表()
    {
        await this.LoginAsAdminAsync();

        var result = await this.Service.GetMqttServerListAsync(TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    [Fact]
    public async Task AddAndRemoveMqttServerAsync_成功时返回服务器信息()
    {
        await this.LoginAsAdminAsync();

        var addResult = await this.Service.AddMqttServerAsync(MqttServerType.Tcp, "0.0.0.0", 18883, "/mqtt", null, TestContext.Current.CancellationToken);
        Assert.True(addResult.IsSuccess);
        Assert.NotNull(addResult.Value);

        var serverId = addResult.Value.Id;
        var removeResult = await this.Service.RemoveMqttServerAsync(serverId!, TestContext.Current.CancellationToken);
        Assert.True(removeResult.IsSuccess);
    }

    [Fact]
    public async Task GetMqttAuthUsersAsync_成功时返回用户列表()
    {
        await this.LoginAsAdminAsync();

        var result = await this.Service.GetMqttAuthUsersAsync(TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    [Fact]
    public async Task AddUpdateDeleteMqttAuthUserAsync_完整生命周期成功()
    {
        await this.LoginAsAdminAsync();

        var userName = $"mqttuser_{Guid.NewGuid():N}";

        var addResult = await this.Service.AddMqttAuthUserAsync(userName, "pass123", "测试用户", TestContext.Current.CancellationToken);
        Assert.True(addResult.IsSuccess);

        var updateResult = await this.Service.UpdateMqttAuthUserAsync(userName, "newpass456", "更新备注", TestContext.Current.CancellationToken);
        Assert.True(updateResult.IsSuccess);

        var deleteResult = await this.Service.DeleteMqttAuthUserAsync(userName, TestContext.Current.CancellationToken);
        Assert.True(deleteResult.IsSuccess);
    }

    [Fact]
    public async Task GetMqttBrokerSettingsAsync_成功时返回配置()
    {
        await this.LoginAsAdminAsync();

        var result = await this.Service.GetMqttBrokerSettingsAsync(TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    [Fact]
    public async Task SetMqttBrokerSettingsAsync_成功时返回Success()
    {
        await this.LoginAsAdminAsync();

        var getResult = await this.Service.GetMqttBrokerSettingsAsync(TestContext.Current.CancellationToken);
        Assert.True(getResult.IsSuccess);

        var result = await this.Service.SetMqttBrokerSettingsAsync(
            getResult.Value.MessageCapacity,
            getResult.Value.MessageExpirySeconds,
            getResult.Value.SessionExpirySeconds,
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetMqttDiscardedStatsAsync_成功时返回统计信息()
    {
        await this.LoginAsAdminAsync();

        var result = await this.Service.GetMqttDiscardedStatsAsync(TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    [Fact]
    public async Task ResetMqttDiscardedStatsAsync_成功时返回Success()
    {
        await this.LoginAsAdminAsync();

        var result = await this.Service.ResetMqttDiscardedStatsAsync(TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetMqttDashboardStatsAsync_成功时返回仪表盘统计()
    {
        await this.LoginAsAdminAsync();

        var result = await this.Service.GetMqttDashboardStatsAsync(TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    [Fact]
    public async Task GetDashboardStatsHistoryAsync_成功时返回历史快照列表()
    {
        await this.LoginAsAdminAsync();

        var result = await this.Service.GetDashboardStatsHistoryAsync("minute", TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    [Fact]
    public async Task GetMqttSslCertsAsync_成功时返回证书列表()
    {
        await this.LoginAsAdminAsync();

        var result = await this.Service.GetMqttSslCertsAsync(TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    // =====================================================================
    // MqttTopicManager
    // =====================================================================

    [Fact]
    public async Task GetMqttTopicListAsync_成功时返回主题列表()
    {
        await this.LoginAsAdminAsync();

        var result = await this.Service.GetMqttTopicListAsync(TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    [Fact]
    public async Task GetMqttTopicStatsAsync_传入空列表时返回所有主题统计()
    {
        await this.LoginAsAdminAsync();

        var result = await this.Service.GetMqttTopicStatsAsync([], TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    [Fact]
    public async Task ResetMqttTopicStatsAsync_传入空列表时成功重置所有主题()
    {
        await this.LoginAsAdminAsync();

        var result = await this.Service.ResetMqttTopicStatsAsync([], TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
    }

    /// <summary>
    /// 释放服务连接资源。
    /// </summary>
    protected abstract ValueTask DisposeServiceAsync();

    /// <summary>
    /// 初始化服务连接。
    /// </summary>
    protected abstract ValueTask InitializeServiceAsync();

    private async Task LoginAsAdminAsync()
    {
        var loginResult = await this.Service.LoginAsync("admin", "admin123", TestContext.Current.CancellationToken);
        Assert.True(loginResult.IsSuccess);
    }

    private async Task<RpcAuthorizationTestContext> PrepareRpcAuthorizationTestContextAsync(CancellationToken cancellationToken)
    {
        var methodsResult = await this.Service.GetAllRpcMethodsAsync(cancellationToken);
        Assert.True(methodsResult.IsSuccess);
        Assert.NotEmpty(methodsResult.Value);

        var rolesResult = await this.Service.GetAllRpcRolesAsync(cancellationToken);
        Assert.True(rolesResult.IsSuccess);

        var configuredTargets = rolesResult.Value.ToDictionary(item => item.TargetKey, item => item);
        var targetKey = methodsResult.Value
            .Select(item => item.MethodTargetKey)
            .FirstOrDefault(key => !configuredTargets.ContainsKey(key))
            ?? methodsResult.Value[0].MethodTargetKey;

        configuredTargets.TryGetValue(targetKey, out var originalConfig);
        return new RpcAuthorizationTestContext(targetKey, originalConfig);
    }

    private async Task RestoreRpcAuthorizationAsync(RpcAuthorizationTestContext context, CancellationToken cancellationToken)
    {
        await this.LoginAsAdminAsync();

        if (context.OriginalConfig == null)
        {
            var currentRolesResult = await this.Service.GetRpcRolesAsync(context.TargetKey, cancellationToken);
            Assert.True(currentRolesResult.IsSuccess, $"获取RPC授权配置失败，TargetKey={context.TargetKey}");
            if (currentRolesResult.Value.Length == 0)
                return;

            var removeResult = await this.Service.RemoveRpcRolesAsync(context.TargetKey, cancellationToken);
            Assert.True(removeResult.IsSuccess, $"恢复RPC授权配置失败，TargetKey={context.TargetKey}");
            return;
        }

        var setResult = await this.Service.SetRpcRolesAsync(context.TargetKey, context.OriginalConfig.RoleCodes, context.OriginalConfig.Description, cancellationToken);
        Assert.True(setResult.IsSuccess, $"恢复RPC授权配置失败，TargetKey={context.TargetKey}");
    }

    private async Task SetupTestEnvironmentAsync()
    {
        var loginResult = await this.Service.LoginAsync("admin", "admin123", default);
        Assert.True(loginResult.IsSuccess, "初始化时登录 admin 账号失败");

        var roleName = $"test_role_{Guid.NewGuid():N}";
        var roleCode = $"test_code_{Guid.NewGuid():N}";
        var roleResult = await this.Service.CreateRoleAsync(roleName, roleCode, "单元测试专用角色", default);
        Assert.True(roleResult.IsSuccess, "创建测试专用角色失败");
        this.m_testRole = roleResult.Value;

        this.TestUserAccount = $"test_{Guid.NewGuid():N}";
        var userResult = await this.Service.RegisterAsync(this.TestUserAccount, "Test@123456", default);
        Assert.True(userResult.IsSuccess, "创建测试专用用户失败");

        var userListResult = await this.Service.GetUserListAsync(default);
        Assert.True(userListResult.IsSuccess, "获取用户列表失败");
        this.m_testUserId = userListResult.Value.First(u => u.Account == this.TestUserAccount).Id;
    }

    private async Task TearDownTestEnvironmentAsync()
    {
        var loginResult = await this.Service.LoginAsync("admin", "admin123", default);
        if (!loginResult.IsSuccess)
            return;

        if (this.m_testUserId > 0)
            await this.Service.DeleteUserAsync(this.m_testUserId, default);

        if (this.m_testRole != null)
            await this.Service.DeleteRoleAsync(this.m_testRole.Id, default);
    }

    private sealed record RpcAuthorizationTestContext(string TargetKey, RpcMethodRoleInfo? OriginalConfig);
}
