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
/// MQTT 服务器配置仓储实现。
/// </summary>
public class MqttServerRepository : RepositoryBase<MqttServerEntity>
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public MqttServerRepository(OrmClient db) : base(db)
    {
    }

    /// <summary>
    /// 通过 ID 获取服务器配置。
    /// </summary>
    public async Task<MqttServerEntity?> GetByIdAsync(string id)
    {
        return await this.Db.Queryable<MqttServerEntity>()
            .Where(s => s.Id == id)
            .FirstAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// 通过类型和端口获取服务器配置。
    /// </summary>
    public async Task<MqttServerEntity?> GetByTypeAndPortAsync(int type, int port)
    {
        return await this.Db.Queryable<MqttServerEntity>()
            .Where(s => s.Type == type && s.Port == port)
            .FirstAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// 新增服务器配置。
    /// </summary>
    public new async Task<bool> InsertAsync(MqttServerEntity entity)
    {
        return await this.Db.Insertable(entity)
            .ExecuteAsync()
            .ConfigureAwait(false) > 0;
    }

    /// <summary>
    /// 保存服务器配置（存在则更新，不存在则插入）。
    /// </summary>
    public async Task SaveAsync(MqttServerEntity entity)
    {
        await this.Db.Saveable(entity)
            .ExecuteAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// 删除服务器配置。
    /// </summary>
    public async Task<bool> DeleteAsync(string id)
    {
        return await this.Db.Deleteable<MqttServerEntity>()
            .Where(s => s.Id == id)
            .ExecuteAsync()
            .ConfigureAwait(false) > 0;
    }
}
