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

namespace TSMQServer.Db;

/// <summary>
/// Broker 全局参数的数据库持久化接口。
/// </summary>
public interface IBrokerSettingsFromDB
{
    /// <summary>
    /// 读取持久化的 Broker 参数。若尚未保存，则返回默认值。
    /// </summary>
    Task<(int MessageCapacity, int MessageExpirySeconds, int SessionExpirySeconds)> GetAsync();

    /// <summary>
    /// 持久化保存 Broker 参数。
    /// </summary>
    Task SaveAsync(int messageCapacity, int messageExpirySeconds, int sessionExpirySeconds);
}
