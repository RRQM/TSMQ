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
/// MQTT 统计快照数据。
/// </summary>
public class MqttStatsSnapshot
{
    /// <summary>
    /// 快照 ID。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 快照记录时间（UTC）。
    /// </summary>
    public DateTime SnapshotAt { get; set; }

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
}
