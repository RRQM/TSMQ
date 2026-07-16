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
/// 添加 MQTT 服务器请求。
/// </summary>
public class AddMqttServerRequest : LoginedRequest
{
    /// <summary>
    /// 服务器类型。
    /// </summary>
    public MqttServerType Type { get; set; }

    /// <summary>
    /// 监听地址（IP）。
    /// </summary>
    public string Host { get; set; } = "0.0.0.0";

    /// <summary>
    /// 监听端口。
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// WebSocket 路径，仅 <see cref="MqttServerType.WebSocket"/> 类型有效。
    /// </summary>
    public string Path { get; set; } = "/mqtt";

    /// <summary>
    /// 关联的 SSL 证书 ID，为 <see langword="null"/> 表示不使用 SSL。
    /// </summary>
    public string? SslCertId { get; set; }
}

/// <summary>
/// 通过服务器 ID 操作的请求。
/// </summary>
public class MqttServerIdRequest : LoginedRequest
{
    /// <summary>
    /// 服务器唯一标识。
    /// </summary>
    public string ServerId { get; set; } = string.Empty;
}

/// <summary>
/// 添加或更新 MQTT 连接授权用户请求。
/// </summary>
public class MqttAuthUserRequest : LoginedRequest
{
    /// <summary>
    /// 用户名。
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 密码。
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 备注。
    /// </summary>
    public string? Remark { get; set; }
}

/// <summary>
/// 删除 MQTT 连接授权用户请求。
/// </summary>
public class MqttAuthUserDeleteRequest : LoginedRequest
{
    /// <summary>
    /// 要删除的用户名。
    /// </summary>
    public string UserName { get; set; } = string.Empty;
}

/// <summary>
/// 添加 SSL 证书请求。
/// </summary>
public class AddMqttSslCertRequest : LoginedRequest
{
    /// <summary>
    /// 证书名称。
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 证书类型。
    /// </summary>
    public MqttSslCertType Type { get; set; }

    /// <summary>
    /// 证书数据（PFX 为 Base64；PEM 为 PEM 文本）。
    /// </summary>
    public string CertData { get; set; } = string.Empty;

    /// <summary>
    /// 私鑰数据，仅 <see cref="MqttSslCertType.Pem"/> 类型使用（PEM 文本）。
    /// </summary>
    public string? KeyData { get; set; }

    /// <summary>
    /// 证书密码，仅 <see cref="MqttSslCertType.Pfx"/> 类型使用。
    /// </summary>
    public string? Password { get; set; }
}

/// <summary>
/// 通过 SSL 证书 ID 操作的请求。
/// </summary>
public class MqttSslCertIdRequest : LoginedRequest
{
    /// <summary>
    /// SSL 证书唯一标识。
    /// </summary>
    public string CertId { get; set; } = string.Empty;
}

/// <summary>
/// 设置 MQTT Broker 参数请求。
/// </summary>
public class SetMqttBrokerSettingsRequest : LoginedRequest
{
    /// <summary>
    /// 每个会话的最大消息队列容量，0 表示不限制。
    /// </summary>
    public int MessageCapacity { get; set; }

    /// <summary>
    /// 消息在队列中的最大存活时间（秒），0 表示不过期。
    /// </summary>
    public int MessageExpirySeconds { get; set; }

    /// <summary>
    /// 离线会话的过期时长（秒），0 表示不自动清除。
    /// </summary>
    public int SessionExpirySeconds { get; set; }
}

/// <summary>
/// 获取仪表盘统计历史数据请求。
/// </summary>
public class GetDashboardStatsHistoryRequest : LoginedRequest
{
    /// <summary>
    /// 统计粒度：minute（按分钟，最近1小时）/ hour（按小时，最近24小时）/ day（按天，最近30天）/ all（全部，按天聚合）。
    /// </summary>
    public string Granularity { get; set; } = "minute";
}
