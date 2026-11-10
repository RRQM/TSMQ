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
/// MQTT 统计快照数据库接口。
/// </summary>
public interface IMqttStatsSnapshotFromDB
{
    /// <summary>
    /// 插入一条统计快照。
    /// </summary>
    Task InsertAsync(MqttStatsSnapshot snapshot);

    /// <summary>
    /// 查询指定时间范围内的快照列表，按 SnapshotAt 升序。
    /// </summary>
    Task<List<MqttStatsSnapshot>> QueryRangeAsync(DateTime from, DateTime to);

    /// <summary>
    /// 删除指定时间之前的快照（用于清理旧数据）。
    /// </summary>
    Task DeleteBeforeAsync(DateTime before);
}
