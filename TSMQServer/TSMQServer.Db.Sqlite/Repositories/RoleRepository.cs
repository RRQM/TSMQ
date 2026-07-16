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
/// 角色仓储实现。
/// </summary>
public class RoleRepository : RepositoryBase<RoleEntity>
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public RoleRepository(OrmClient db) : base(db)
    {
    }

    /// <summary>
    /// 插入角色实体并返回自增主键。
    /// </summary>
    public new async Task<long> InsertAsync(RoleEntity entity)
    {
        return await this.Db.Insertable(entity)
            .ExecuteReturnIdentityAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<RoleEntity?> GetByIdAsync(long id)
    {
        return await this.Db.Queryable<RoleEntity>()
            .Where(r => r.Id == id)
            .FirstAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<RoleEntity?> GetByCodeAsync(string roleCode)
    {
        return await this.Db.Queryable<RoleEntity>()
            .Where(r => r.RoleCode == roleCode)
            .FirstAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<bool> DeleteAsync(long id)
    {
        return await this.Db.Deletable<RoleEntity>()
            .In(id)
            .ExecuteAsync()
            .ConfigureAwait(false) > 0;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<bool> CodeExistsAsync(string roleCode)
    {
        return await this.Db.Queryable<RoleEntity>()
            .Where(r => r.RoleCode == roleCode)
            .AnyAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<IEnumerable<RoleEntity>> GetByIdsAsync(IEnumerable<long> ids)
    {
        var idList = ids.ToList();
        if (idList.Count == 0)
        {
            return [];
        }

        return await this.Db.Queryable<RoleEntity>()
            .Where(r => idList.Contains(r.Id) && r.IsEnabled == 1)
            .ToListAsync()
            .ConfigureAwait(false);
    }
}