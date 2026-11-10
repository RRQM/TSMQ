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
/// MQTT Broker 全局参数配置实体（单行表，固定 Id=1）。
/// </summary>
[OrmTable("BrokerSettings")]
public class BrokerSettingsEntity
{
    /// <summary>
    /// 固定主键，值始终为 1（单行记录）。
    /// </summary>
    [OrmColumn(IsPrimaryKey = true)]
    public int Id { get; set; } = 1;

    /// <summary>
    /// 每个会话的最大消息队列容量，0 表示不限制。
    /// </summary>
    public int MessageCapacity { get; set; } = 1000;

    /// <summary>
    /// 消息在队列中的最大存活时间（秒），0 表示永不过期。
    /// </summary>
    public int MessageExpirySeconds { get; set; } = 0;

    /// <summary>
    /// 离线会话的保留时长（秒），0 表示不自动清除。
    /// </summary>
    public int SessionExpirySeconds { get; set; } = 0;
}
