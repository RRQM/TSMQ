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
/// MQTT 主题消息统计信息。
/// </summary>
public class MqttTopicStats
{
    /// <summary>
    /// 获取或设置主题名称。
    /// </summary>
    public string Topic { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置当前订阅者数量。
    /// </summary>
    public int SubscriberCount { get; set; }

    /// <summary>
    /// 获取或设置累计流入消息数量（客户端发布到该主题的消息总数）。
    /// </summary>
    public long InflowCount { get; set; }

    /// <summary>
    /// 获取或设置累计流出消息数量（向订阅者分发的消息总数）。
    /// </summary>
    public long OutflowCount { get; set; }

    /// <summary>
    /// 获取或设置统计快照时间（UTC）。
    /// </summary>
    public DateTime SnapshotAt { get; set; }
}
