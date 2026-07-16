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
/// 用户仓储实现。
/// </summary>
public class UserRepository : RepositoryBase<UserEntity>
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public UserRepository(OrmClient db) : base(db)
    {
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<UserEntity?> GetByAccountAsync(string account)
    {
        return await this.Db.Queryable<UserEntity>()
            .Where(u => u.Account == account)
            .FirstAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<UserEntity?> GetByIdAsync(long id)
    {
        return await this.Db.Queryable<UserEntity>()
            .Where(u => u.Id == id)
            .FirstAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<bool> UpdateRoleIdsAsync(long userId, string roleIds)
    {
        return await this.Db.Updateable<UserEntity>()
            .SetColumns(u => new UserEntity { RoleIds = roleIds })
            .Where(u => u.Id == userId)
            .ExecuteAsync()
            .ConfigureAwait(false) > 0;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<bool> DeleteAsync(string account)
    {
        var affected = await this.Db.UseTranAsync(async () =>
        {
            await this.Db.Deleteable<LoginSessionEntity>()
                .Where(l => l.Account == account)
                .ExecuteAsync()
                .ConfigureAwait(false);
            return await this.Db.Deleteable<UserEntity>()
                .Where(u => u.Account == account)
                .ExecuteAsync()
                .ConfigureAwait(false);
        }).ConfigureAwait(false);
        return affected > 0;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<bool> AccountExistsAsync(string account)
    {
        return await this.Db.Queryable<UserEntity>()
            .Where(u => u.Account == account)
            .AnyAsync()
            .ConfigureAwait(false);
    }
}


