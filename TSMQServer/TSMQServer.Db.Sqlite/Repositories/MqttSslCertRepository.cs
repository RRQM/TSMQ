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
/// SSL 证书仓储实现。
/// </summary>
public class MqttSslCertRepository : RepositoryBase<MqttSslCertEntity>
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public MqttSslCertRepository(OrmClient db) : base(db)
    {
    }

    /// <summary>
    /// 通过 ID 获取证书实体。
    /// </summary>
    public async Task<MqttSslCertEntity?> GetByIdAsync(string id)
    {
        return await this.Db.Queryable<MqttSslCertEntity>()
            .Where(c => c.Id == id)
            .FirstAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// 新增证书实体。
    /// </summary>
    public new async Task<bool> InsertAsync(MqttSslCertEntity entity)
    {
        return await this.Db.Insertable(entity)
            .ExecuteAsync()
            .ConfigureAwait(false) > 0;
    }

    /// <summary>
    /// 删除证书实体。
    /// </summary>
    public async Task<bool> DeleteAsync(string id)
    {
        return await this.Db.Deleteable<MqttSslCertEntity>()
            .Where(c => c.Id == id)
            .ExecuteAsync()
            .ConfigureAwait(false) > 0;
    }
}
