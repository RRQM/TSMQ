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
/// SSL 证书原始数据，用于内部加载 X509 证书。
/// </summary>
public class MqttSslCertData
{
    /// <summary>
    /// 证书类型。
    /// </summary>
    public MqttSslCertType Type { get; set; }

    /// <summary>
    /// 证书原始数据（PFX 格式为二进制 Base64；PEM 格式为 PEM 文本）。
    /// </summary>
    public string CertData { get; set; } = string.Empty;

    /// <summary>
    /// 私钥数据，仅 <see cref="MqttSslCertType.Pem"/> 时使用（PEM 格式文本）。
    /// </summary>
    public string? KeyData { get; set; }

    /// <summary>
    /// 证书密码，仅 <see cref="MqttSslCertType.Pfx"/> 时使用。
    /// </summary>
    public string? Password { get; set; }
}
