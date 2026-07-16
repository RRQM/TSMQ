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
/// Broker 全局参数配置仓储实现。
/// </summary>
public class BrokerSettingsRepository : RepositoryBase<BrokerSettingsEntity>
{
    /// <inheritdoc/>
    public BrokerSettingsRepository(OrmClient db) : base(db)
    {
    }

    /// <summary>
    /// 读取配置（固定 Id=1 的单行记录）。
    /// </summary>
    public async Task<BrokerSettingsEntity?> GetSingleAsync()
    {
        return await this.Db.Queryable<BrokerSettingsEntity>()
            .Where(s => s.Id == 1)
            .FirstAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// 保存配置（不存在则插入，已存在则更新）。
    /// </summary>
    public async Task SaveAsync(BrokerSettingsEntity entity)
    {
        await this.Db.Saveable(entity)
            .ExecuteAsync()
            .ConfigureAwait(false);
    }
}
