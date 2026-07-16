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
/// SSL 证书展示信息（不含证书原始数据）。
/// </summary>
public class MqttSslCertInfo
{
    /// <summary>
    /// 证书唯一标识。
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 证书名称。
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 证书类型。
    /// </summary>
    public MqttSslCertType Type { get; set; }

    /// <summary>
    /// 证书主题（Subject）。
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// 证书指纹（Thumbprint）。
    /// </summary>
    public string Thumbprint { get; set; } = string.Empty;

    /// <summary>
    /// 证书有效期开始时间。
    /// </summary>
    public DateTime NotBefore { get; set; }

    /// <summary>
    /// 证书有效期截止时间。
    /// </summary>
    public DateTime NotAfter { get; set; }

    /// <summary>
    /// 创建时间。
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
