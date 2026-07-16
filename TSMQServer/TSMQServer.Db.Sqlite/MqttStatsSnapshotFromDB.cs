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
using TSMQServer.Db;
using TSMQServer.Db.Sqlite.Entities;
using TSMQServer.Db.Sqlite.Repositories;

namespace TSMQServer.Db.Sqlite;

/// <summary>
/// MQTT 统计快照数据库实现（Sqlite）。
/// </summary>
public class MqttStatsSnapshotFromDB : IMqttStatsSnapshotFromDB
{
    private readonly MqttStatsSnapshotRepository m_repository;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public MqttStatsSnapshotFromDB(MqttStatsSnapshotRepository repository)
    {
        this.m_repository = repository;
    }

    /// <inheritdoc/>
    public async Task InsertAsync(MqttStatsSnapshot snapshot)
    {
        await this.m_repository.InsertSnapshotAsync(ToEntity(snapshot)).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<List<MqttStatsSnapshot>> QueryRangeAsync(DateTime from, DateTime to)
    {
        var entities = await this.m_repository.QueryRangeAsync(from.Ticks, to.Ticks).ConfigureAwait(false);
        return entities.Select(ToSnapshot).ToList();
    }

    /// <inheritdoc/>
    public async Task DeleteBeforeAsync(DateTime before)
    {
        await this.m_repository.DeleteBeforeAsync(before.Ticks).ConfigureAwait(false);
    }

    private static MqttStatsSnapshotEntity ToEntity(MqttStatsSnapshot s) => new()
    {
        SnapshotAtTicks = s.SnapshotAt.Ticks,
        OnlineClients = s.OnlineClients,
        ActiveTopics = s.ActiveTopics,
        RunningServers = s.RunningServers,
        TotalReceived = s.TotalReceived,
        TotalForwarded = s.TotalForwarded,
        TotalBytesReceived = s.TotalBytesReceived,
        TotalBytesForwarded = s.TotalBytesForwarded,
        DiscardedTotal = s.DiscardedTotal,
        DiscardedQueueFull = s.DiscardedQueueFull,
        DiscardedExpired = s.DiscardedExpired,
        DiscardedPublishFailed = s.DiscardedPublishFailed
    };

    private static MqttStatsSnapshot ToSnapshot(MqttStatsSnapshotEntity e) => new()
    {
        Id = e.Id,
        SnapshotAt = new DateTime(e.SnapshotAtTicks, DateTimeKind.Utc),
        OnlineClients = e.OnlineClients,
        ActiveTopics = e.ActiveTopics,
        RunningServers = e.RunningServers,
        TotalReceived = e.TotalReceived,
        TotalForwarded = e.TotalForwarded,
        TotalBytesReceived = e.TotalBytesReceived,
        TotalBytesForwarded = e.TotalBytesForwarded,
        DiscardedTotal = e.DiscardedTotal,
        DiscardedQueueFull = e.DiscardedQueueFull,
        DiscardedExpired = e.DiscardedExpired,
        DiscardedPublishFailed = e.DiscardedPublishFailed
    };
}
