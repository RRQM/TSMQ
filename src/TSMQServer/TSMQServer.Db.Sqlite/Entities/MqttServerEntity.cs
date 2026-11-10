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

namespace TSMQServer.Db.Sqlite.Entities;

/// <summary>
/// MQTT 服务器配置实体。
/// </summary>
[OrmTable("MqttServers")]
public class MqttServerEntity
{
    /// <summary>
    /// 服务器唯一标识。
    /// </summary>
    [OrmColumn(IsPrimaryKey = true)]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 服务器类型，0 为 TCP，1 为 WebSocket。
    /// </summary>
    public int Type { get; set; }

    /// <summary>
    /// 监听地址。
    /// </summary>
    public string Host { get; set; } = "0.0.0.0";

    /// <summary>
    /// 监听端口。
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// WebSocket 路径。
    /// </summary>
    public string Path { get; set; } = "/mqtt";

    /// <summary>
    /// 是否运行中，1 表示运行中。
    /// </summary>
    public int IsRunning { get; set; }

    /// <summary>
    /// 创建时间。
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 关联的 SSL 证书 ID，为 <see langword="null"/> 表示不使用 SSL。
    /// </summary>
    public string? SslCertId { get; set; }
}
