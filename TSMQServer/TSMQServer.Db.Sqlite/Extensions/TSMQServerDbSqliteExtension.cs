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

using Microsoft.Extensions.DependencyInjection;
using TORM;
using TSMQServer.Db;
using TSMQServer.Db.Sqlite.Repositories;

namespace TSMQServer.Db.Sqlite.Extensions;

public static class TSMQServerDbSqliteExtension
{
    public static void UseSqliteDb(this IServiceCollection services)
    {
        var dbInitializer = new SqliteDbInitializer();
        dbInitializer.Initialize();

        services.AddTransient<OrmClient>(p => dbInitializer.CreateOrmClient());

        services.AddTransient<UserRepository>();
        services.AddTransient<LoginSessionRepository>();
        services.AddTransient<RoleRepository>();
        services.AddTransient<RpcMethodRoleRepository>();
        services.AddTransient<MqttServerRepository>();
        services.AddTransient<MqttAuthUserRepository>();
        services.AddTransient<MqttSslCertRepository>();
        services.AddTransient<MqttStatsSnapshotRepository>();
        services.AddTransient<BrokerSettingsRepository>();

        services.AddTransient<IUserManagerFromDB, UserManagerFromDB>();
        services.AddTransient<IRoleManagerFromDB, RoleManagerFromDB>();
        services.AddTransient<IRpcAuthorizationFromDB, RpcAuthorizationFromDB>();
        services.AddTransient<IMqttServerFromDB, MqttServerFromDB>();
        services.AddTransient<IMqttConnectAuthFromDB, MqttConnectAuthFromDB>();
        services.AddTransient<IMqttSslCertFromDB, MqttSslCertFromDB>();
        services.AddTransient<IMqttStatsSnapshotFromDB, MqttStatsSnapshotFromDB>();
        services.AddTransient<IBrokerSettingsFromDB, BrokerSettingsFromDB>();
    }
}

