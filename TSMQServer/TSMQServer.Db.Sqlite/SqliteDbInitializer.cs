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

using TORM;
using TSMQServer.Db.Sqlite.Entities;

namespace TSMQServer.Db.Sqlite;

public class SqliteDbInitializer
{
    private const string AdminAccount = "admin";
    private const string AdminPassword = "admin123";
    private const string AdminRoleCode = "admin";
    private readonly string m_dbPath;

    public SqliteDbInitializer(string dbPath = "TSMQ.db")
    {
        this.m_dbPath = dbPath;
    }

    public OrmClient CreateOrmClient()
    {
        return new OrmClient(new ConnectionConfig
        {
            DatabaseType = DatabaseType.Sqlite,
            ConnectionString = $"Data Source={m_dbPath}"
        });
    }

    public void Initialize()
    {
        this.InitializeAsync().GetAwaiter().GetResult();
    }

    public async Task InitializeAsync()
    {
        using (var client = CreateOrmClient())
        {
            await client.CodeFirst!.InitTableAsync<UserEntity>().ConfigureAwait(false);
            await client.CodeFirst!.InitTableAsync<LoginSessionEntity>().ConfigureAwait(false);
            await client.CodeFirst!.InitTableAsync<RoleEntity>().ConfigureAwait(false);
            await client.CodeFirst!.InitTableAsync<RpcMethodRoleEntity>().ConfigureAwait(false);
            await client.CodeFirst!.InitTableAsync<MqttServerEntity>().ConfigureAwait(false);
            await client.CodeFirst!.InitTableAsync<MqttAuthUserEntity>().ConfigureAwait(false);
            await client.CodeFirst!.InitTableAsync<MqttSslCertEntity>().ConfigureAwait(false);
            await client.CodeFirst!.InitTableAsync<MqttStatsSnapshotEntity>().ConfigureAwait(false);
            await client.CodeFirst!.InitTableAsync<BrokerSettingsEntity>().ConfigureAwait(false);
            await client.CodeFirst!.InitTableAsync<MqttAccessPolicyEntity>().ConfigureAwait(false);
            await SeedDefaultDataAsync(client).ConfigureAwait(false);
        }
    }

    private static IEnumerable<(string TargetKey, string RoleCodes, string Description)> GetDefaultRpcMethodRoles(string adminRoleCode)
    {
        yield return ("UserManagerService.GetUserList", adminRoleCode, "获取用户列表");
        yield return ("UserManagerService.AssignUserRoles", adminRoleCode, "分配用户角色");
        yield return ("RoleManagerService.CreateRole", adminRoleCode, "创建角色");
        yield return ("RoleManagerService.UpdateRole", adminRoleCode, "更新角色");
        yield return ("RoleManagerService.DeleteRole", adminRoleCode, "删除角色");
        yield return ("RoleManagerService.GetRoleList", adminRoleCode, "获取角色列表");
        yield return ("RpcAuthorizationService.SetRpcMethodRoles", adminRoleCode, "设置RPC方法角色");
        yield return ("RpcAuthorizationService.GetRpcMethodRoles", adminRoleCode, "获取RPC方法角色");
        yield return ("LogManagerService", adminRoleCode, "日志管理（全部方法）");
    }

    private static async Task SeedDefaultDataAsync(OrmClient client)
    {
        var adminRole = await client.Queryable<RoleEntity>()
            .Where(r => r.RoleCode == AdminRoleCode)
            .FirstAsync()
            .ConfigureAwait(false);

        if (adminRole == null)
        {
            var roleId = await client.Insertable(new RoleEntity
            {
                RoleName = "管理员",
                RoleCode = AdminRoleCode,
                Description = "系统内置超级管理员角色",
                IsEnabled = 1,
                CreateTime = DateTime.Now
            }).ExecuteReturnIdentityAsync().ConfigureAwait(false);

            adminRole = new RoleEntity { Id = roleId };
        }

        var adminUser = await client.Queryable<UserEntity>()
            .Where(u => u.Account == AdminAccount)
            .FirstAsync()
            .ConfigureAwait(false);

        if (adminUser == null)
        {
            await client.Insertable(new UserEntity
            {
                Account = AdminAccount,
                Password = AdminPassword,
                NickName = "管理员",
                RoleIds = adminRole.Id.ToString(),
                Status = 1,
                CreateTime = DateTime.Now
            }).ExecuteAsync().ConfigureAwait(false);
        }

        foreach (var (targetKey, roleCodes, description) in GetDefaultRpcMethodRoles(AdminRoleCode))
        {
            var exists = await client.Queryable<RpcMethodRoleEntity>()
                .Where(r => r.TargetKey == targetKey)
                .AnyAsync()
                .ConfigureAwait(false);

            if (!exists)
            {
                await client.Insertable(new RpcMethodRoleEntity
                {
                    TargetKey = targetKey,
                    RoleCodes = roleCodes,
                    Description = description,
                    UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                }).ExecuteAsync().ConfigureAwait(false);
            }
        }
    }
}
