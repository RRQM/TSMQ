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

namespace TSMQServer.Core;

/// <summary>
/// MQTT 服务器列表响应。
/// </summary>
public class MqttServerListResponse : ResponseBase
{
    /// <summary>
    /// MQTT 服务器信息列表。
    /// </summary>
    public List<MqttServerInfo>? Servers { get; set; }
}

/// <summary>
/// 单个 MQTT 服务器响应。
/// </summary>
public class MqttServerResponse : ResponseBase
{
    /// <summary>
    /// MQTT 服务器信息。
    /// </summary>
    public MqttServerInfo? Server { get; set; }
}

/// <summary>
/// MQTT 连接授权用户列表响应。
/// </summary>
public class MqttAuthUserListResponse : ResponseBase
{
    /// <summary>
    /// 授权用户列表。
    /// </summary>
    public List<MqttAuthUserInfo>? Users { get; set; }
}

/// <summary>
/// Broker 设置响应。
/// </summary>
public class MqttBrokerSettingsResponse : ResponseBase
{
    /// <summary>
    /// 每个会话的最大消息队列容量，0 表示不限制。
    /// </summary>
    public int MessageCapacity { get; set; }

    /// <summary>
    /// 消息过期时长（秒），0 表示不过期。
    /// </summary>
    public int MessageExpirySeconds { get; set; }

    /// <summary>
    /// 离线会话过期时长（秒），0 表示不自动清除。
    /// </summary>
    public int SessionExpirySeconds { get; set; }
}

/// <summary>
/// 消息丢弃统计响应。
/// </summary>
public class MqttDiscardedStatsResponse : ResponseBase
{
    /// <summary>
    /// 总丢弃数量。
    /// </summary>
    public long Total { get; set; }

    /// <summary>
    /// 队列满导致的丢弃数量。
    /// </summary>
    public long QueueFull { get; set; }

    /// <summary>
    /// 消息过期导致的丢弃数量。
    /// </summary>
    public long Expired { get; set; }

    /// <summary>
    /// 发送失败导致的丢弃数量。
    /// </summary>
    public long PublishFailed { get; set; }

    /// <summary>
    /// 上次重置时间。
    /// </summary>
    public DateTime LastResetAt { get; set; }
}

/// <summary>
/// 仪表盘综合统计响应。
/// </summary>
public class MqttDashboardStatsResponse : ResponseBase
{
    /// <summary>
    /// 当前在线客户端数量。
    /// </summary>
    public int OnlineClients { get; set; }

    /// <summary>
    /// 当前活跃主题数量。
    /// </summary>
    public int ActiveTopics { get; set; }

    /// <summary>
    /// 当前运行中的服务器数量。
    /// </summary>
    public int RunningServers { get; set; }

    /// <summary>
    /// 历史总接收消息条数。
    /// </summary>
    public long TotalReceived { get; set; }

    /// <summary>
    /// 历史总转发消息条数。
    /// </summary>
    public long TotalForwarded { get; set; }

    /// <summary>
    /// 历史总接收字节数。
    /// </summary>
    public long TotalBytesReceived { get; set; }

    /// <summary>
    /// 历史总转发字节数。
    /// </summary>
    public long TotalBytesForwarded { get; set; }

    /// <summary>
    /// 历史总丢弃消息条数。
    /// </summary>
    public long DiscardedTotal { get; set; }

    /// <summary>
    /// 队列满导致的丢弃数量。
    /// </summary>
    public long DiscardedQueueFull { get; set; }

    /// <summary>
    /// 消息过期导致的丢弃数量。
    /// </summary>
    public long DiscardedExpired { get; set; }

    /// <summary>
    /// 发送失败导致的丢弃数量。
    /// </summary>
    public long DiscardedPublishFailed { get; set; }

    /// <summary>
    /// 服务启动时间。
    /// </summary>
    public DateTime ServerStartedAt { get; set; }

    /// <summary>
    /// 快照时间。
    /// </summary>
    public DateTime SnapshotAt { get; set; }
}

/// <summary>
/// SSL 证书列表响应。
/// </summary>
public class MqttSslCertListResponse : ResponseBase
{
    /// <summary>
    /// SSL 证书信息列表。
    /// </summary>
    public List<MqttSslCertInfo>? Certs { get; set; }
}

/// <summary>
/// 单个 SSL 证书响应。
/// </summary>
public class MqttSslCertResponse : ResponseBase
{
    /// <summary>
    /// SSL 证书信息。
    /// </summary>
    public MqttSslCertInfo? Cert { get; set; }
}

/// <summary>
/// 仪表盘统计历史数据响应。
/// </summary>
public class MqttDashboardStatsHistoryResponse : ResponseBase
{
    /// <summary>
    /// 快照列表，按时间升序排列。
    /// </summary>
    public List<MqttStatsSnapshot>? Snapshots { get; set; }
}
