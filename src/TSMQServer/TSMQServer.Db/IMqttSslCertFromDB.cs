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

namespace TSMQServer.Db;

/// <summary>
/// SSL 证书数据库接口。
/// </summary>
public interface IMqttSslCertFromDB
{
    /// <summary>
    /// 获取所有 SSL 证书展示信息。
    /// </summary>
    Task<List<MqttSslCertInfo>> GetAllAsync();

    /// <summary>
    /// 通过 ID 获取证书原始数据，用于加载 X509 证书。
    /// </summary>
    /// <param name="id">证书唯一标识。</param>
    Task<MqttSslCertData?> GetCertDataAsync(string id);

    /// <summary>
    /// 通过 ID 获取证书名称，返回 <see langword="null"/> 表示不存在。
    /// </summary>
    /// <param name="id">证书唯一标识。</param>
    Task<string?> GetNameAsync(string id);

    /// <summary>
    /// 新增 SSL 证书。
    /// </summary>
    /// <param name="info">证书展示信息。</param>
    /// <param name="data">证书原始数据。</param>
    Task<bool> AddAsync(MqttSslCertInfo info, MqttSslCertData data);

    /// <summary>
    /// 删除 SSL 证书。
    /// </summary>
    /// <param name="id">证书唯一标识。</param>
    Task<bool> DeleteAsync(string id);
}
