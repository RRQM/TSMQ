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

namespace TSMQServer.Core.Imp.Services;

/// <summary>
/// MQTT 服务宿主接口，负责管理共享 Broker 及各类服务器监听器。
/// </summary>
public interface IMqttServerHostService
{
    /// <summary>
    /// 获取所有 MQTT 服务器配置信息。
    /// </summary>
    IReadOnlyList<MqttServerInfo> GetServers();

    /// <summary>
    /// 添加 TCP MQTT 服务器监听。
    /// </summary>
    /// <param name="host">监听地址。</param>
    /// <param name="port">监听端口。</param>
    /// <param name="sslCertId">SSL 证书 ID，为 <see langword="null"/> 表示不使用 SSL。</param>
    /// <returns>创建成功的服务器信息；若端口已占用则返回 <see langword="null"/>。</returns>
    Task<MqttServerInfo?> AddTcpServerAsync(string host, int port, string? sslCertId = null);

    /// <summary>
    /// 添加 WebSocket MQTT 服务器监听。
    /// </summary>
    /// <param name="host">监听地址。</param>
    /// <param name="port">监听端口。</param>
    /// <param name="path">WebSocket 路径。</param>
    /// <param name="sslCertId">SSL 证书 ID，为 <see langword="null"/> 表示不使用 SSL。</param>
    /// <returns>创建成功的服务器信息；若端口已占用则返回 <see langword="null"/>。</returns>
    Task<MqttServerInfo?> AddWebSocketServerAsync(string host, int port, string path, string? sslCertId = null);

    /// <summary>
    /// 启动指定 ID 的服务器监听。
    /// </summary>
    /// <param name="serverId">服务器唯一标识。</param>
    /// <returns><see langword="true"/> 表示启动成功。</returns>
    Task<bool> StartServerAsync(string serverId);

    /// <summary>
    /// 停止指定 ID 的服务器监听。
    /// </summary>
    /// <param name="serverId">服务器唯一标识。</param>
    /// <returns><see langword="true"/> 表示停止成功。</returns>
    Task<bool> StopServerAsync(string serverId);

    /// <summary>
    /// 移除指定 ID 的服务器配置（若正在运行则先停止）。
    /// </summary>
    /// <param name="serverId">服务器唯一标识。</param>
    /// <returns><see langword="true"/> 表示移除成功。</returns>
    Task<bool> RemoveServerAsync(string serverId);

    /// <summary>
    /// 获取当前所有已连接的 MQTT 客户端信息。
    /// </summary>
    IReadOnlyList<MqttClientInfo> GetConnectedClients();

    /// <summary>
    /// 踢除指定的 MQTT 客户端。
    /// </summary>
    /// <param name="clientIds">要踢除的客户端 ID 集合。</param>
    /// <returns>实际踢除的客户端数量。</returns>
    Task<int> KickClientsAsync(IEnumerable<string> clientIds);

    /// <summary>
    /// 获取当前所有已订阅的主题及其订阅者信息。
    /// </summary>
    IReadOnlyList<MqttTopicInfo> GetTopicList();

    /// <summary>
    /// 获取指定主题的消息统计信息；<paramref name="topics"/> 为空时返回所有已统计主题的信息。
    /// </summary>
    /// <param name="topics">要查询的主题集合。</param>
    IReadOnlyList<MqttTopicStats> GetTopicStats(IEnumerable<string> topics);

    /// <summary>
    /// 重置指定主题的消息统计计数；<paramref name="topics"/> 为空时重置所有主题。
    /// </summary>
    /// <param name="topics">要重置的主题集合。</param>
    void ResetTopicStats(IEnumerable<string> topics);

    /// <summary>
    /// 设置连接授权是否启用（启用时仅允许配置的用户名密码连接）。
    /// </summary>
    /// <param name="enabled">是否启用授权。</param>
    void SetAuthEnabled(bool enabled);

    /// <summary>
    /// 获取 Broker 当前参数设置。
    /// </summary>
    /// <returns>消息队列容量、消息过期秒数、会话过期秒数的元组。</returns>
    (int MessageCapacity, int MessageExpirySeconds, int SessionExpirySeconds) GetBrokerSettings();

    /// <summary>
    /// 设置 Broker 参数，立即生效并持久化保存。
    /// </summary>
    /// <param name="messageCapacity">每个会话的最大消息队列容量，0 表示不限制。</param>
    /// <param name="messageExpirySeconds">消息过期时长（秒），0 表示不过期。</param>
    /// <param name="sessionExpirySeconds">离线会话过期时长（秒），0 表示不自动清除。</param>
    Task SetBrokerSettings(int messageCapacity, int messageExpirySeconds, int sessionExpirySeconds);

    /// <summary>
    /// 获取消息丢弃统计快照。
    /// </summary>
    (long Total, long QueueFull, long Expired, long PublishFailed, DateTime LastResetAt) GetDiscardedStats();

    /// <summary>
    /// 重置消息丢弃统计计数。
    /// </summary>
    void ResetDiscardedStats();

    /// <summary>
    /// 获取仪表盘综合统计数据。
    /// </summary>
    MqttDashboardStatsResponse GetDashboardStats();

    /// <summary>
    /// 查询指定时间范围内的统计历史快照。
    /// </summary>
    Task<List<MqttStatsSnapshot>> QueryStatsHistoryAsync(DateTime from, DateTime to);

    /// <summary>
    /// 获取所有 SSL 证书展示信息。
    /// </summary>
    Task<List<MqttSslCertInfo>> GetSslCertsAsync();

    /// <summary>
    /// 添加 SSL 证书。
    /// </summary>
    /// <param name="info">证书展示信息（不含原始数据）。</param>
    /// <param name="data">证书原始数据。</param>
    Task<bool> AddSslCertAsync(MqttSslCertInfo info, MqttSslCertData data);

    /// <summary>
    /// 删除 SSL 证书。
    /// </summary>
    /// <param name="certId">证书唯一标识。</param>
    Task<bool> DeleteSslCertAsync(string certId);
}
