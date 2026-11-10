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
/// 已连接的 MQTT 客户端信息。
/// </summary>
public class MqttClientInfo
{
    /// <summary>
    /// 客户端 ID。
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// 用户名。
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// 连接状态。
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 客户端 IP 地址（含端口）。
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// 心跳间隔（秒）。
    /// </summary>
    public ushort KeepAlive { get; set; }

    /// <summary>
    /// 是否 Clean Start（清除会话）。
    /// </summary>
    public bool CleanStart { get; set; }

    /// <summary>
    /// 会话过期间隔（秒，0 表示连接断开后立即过期）。
    /// </summary>
    public uint SessionExpiryInterval { get; set; }

    /// <summary>
    /// 连接时间（UTC）。
    /// </summary>
    public DateTime ConnectedAt { get; set; }
}
