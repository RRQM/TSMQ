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
using TSMQServer.Db.Sqlite.Entities;

namespace TSMQServer.Db.Sqlite.Repositories;

/// <summary>
/// MQTT 统计快照仓储。
/// </summary>
public class MqttStatsSnapshotRepository : RepositoryBase<MqttStatsSnapshotEntity>
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public MqttStatsSnapshotRepository(OrmClient db) : base(db)
    {
    }

    /// <summary>
    /// 插入一条快照记录。
    /// </summary>
    public async Task InsertSnapshotAsync(MqttStatsSnapshotEntity entity)
    {
        await this.Db.Insertable(entity)
            .ExecuteAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// 查询指定 Ticks 范围内的快照，按 SnapshotAtTicks 升序。
    /// </summary>
    public async Task<List<MqttStatsSnapshotEntity>> QueryRangeAsync(long fromTicks, long toTicks)
    {
        return await this.Db.Queryable<MqttStatsSnapshotEntity>()
            .Where(e => e.SnapshotAtTicks >= fromTicks && e.SnapshotAtTicks <= toTicks)
            .OrderBy(e => e.SnapshotAtTicks)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// 删除指定 Ticks 之前的快照。
    /// </summary>
    public async Task DeleteBeforeAsync(long beforeTicks)
    {
        await this.Db.Deletable<MqttStatsSnapshotEntity>()
            .Where(e => e.SnapshotAtTicks < beforeTicks)
            .ExecuteAsync()
            .ConfigureAwait(false);
    }
}
