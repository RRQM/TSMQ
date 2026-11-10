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

using TouchSocket.Core;

namespace TSMQServer.Core.Clients;

public partial interface IRemoteService
{
    /// <summary>获取所有 MQTT 服务器列表</summary>
    Task<Result<List<MqttServerInfo>>> GetMqttServerListAsync(CancellationToken cancellationToken = default);

    /// <summary>添加 MQTT 服务器监听</summary>
    Task<Result<MqttServerInfo>> AddMqttServerAsync(MqttServerType type, string host, int port, string path, string? sslCertId, CancellationToken cancellationToken = default);

    /// <summary>启动指定 MQTT 服务器</summary>
    Task<Result> StartMqttServerAsync(string serverId, CancellationToken cancellationToken = default);

    /// <summary>停止指定 MQTT 服务器</summary>
    Task<Result> StopMqttServerAsync(string serverId, CancellationToken cancellationToken = default);

    /// <summary>移除指定 MQTT 服务器配置</summary>
    Task<Result> RemoveMqttServerAsync(string serverId, CancellationToken cancellationToken = default);

    /// <summary>获取 MQTT 连接授权用户列表</summary>
    Task<Result<List<MqttAuthUserInfo>>> GetMqttAuthUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>添加 MQTT 连接授权用户</summary>
    Task<Result> AddMqttAuthUserAsync(string userName, string password, string? remark, CancellationToken cancellationToken = default);

    /// <summary>更新 MQTT 连接授权用户</summary>
    Task<Result> UpdateMqttAuthUserAsync(string userName, string password, string? remark, CancellationToken cancellationToken = default);

    /// <summary>删除 MQTT 连接授权用户</summary>
    Task<Result> DeleteMqttAuthUserAsync(string userName, CancellationToken cancellationToken = default);

    /// <summary>获取 MQTT Broker 当前设置</summary>
    Task<Result<MqttBrokerSettingsResponse>> GetMqttBrokerSettingsAsync(CancellationToken cancellationToken = default);

    /// <summary>设置 MQTT Broker 参数</summary>
    Task<Result> SetMqttBrokerSettingsAsync(int messageCapacity, int messageExpirySeconds, int sessionExpirySeconds, CancellationToken cancellationToken = default);

    /// <summary>获取消息丢弃统计信息</summary>
    Task<Result<MqttDiscardedStatsResponse>> GetMqttDiscardedStatsAsync(CancellationToken cancellationToken = default);

    /// <summary>重置消息丢弃统计计数</summary>
    Task<Result> ResetMqttDiscardedStatsAsync(CancellationToken cancellationToken = default);

    /// <summary>获取仪表盘综合统计数据</summary>
    Task<Result<MqttDashboardStatsResponse>> GetMqttDashboardStatsAsync(CancellationToken cancellationToken = default);

    /// <summary>获取仪表盘统计历史数据</summary>
    Task<Result<List<MqttStatsSnapshot>>> GetDashboardStatsHistoryAsync(string granularity, CancellationToken cancellationToken = default);

    /// <summary>获取所有 SSL 证书列表</summary>
    Task<Result<List<MqttSslCertInfo>>> GetMqttSslCertsAsync(CancellationToken cancellationToken = default);

    /// <summary>添加 SSL 证书</summary>
    Task<Result<MqttSslCertInfo>> AddMqttSslCertAsync(string name, MqttSslCertType type, string certData, string? keyData, string? password, CancellationToken cancellationToken = default);

    /// <summary>删除 SSL 证书</summary>
    Task<Result> DeleteMqttSslCertAsync(string certId, CancellationToken cancellationToken = default);
}
