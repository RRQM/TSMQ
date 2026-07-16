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
/// 登录会话仓储实现。
/// </summary>
public class LoginSessionRepository : RepositoryBase<LoginSessionEntity>
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public LoginSessionRepository(OrmClient db) : base(db)
    {
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<LoginSessionEntity?> GetByTokenAsync(string loginedToken)
    {
        return await this.Db.Queryable<LoginSessionEntity>()
            .Where(l => l.LoginedToken == loginedToken)
            .FirstAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<bool> DeleteAsync(string loginedToken)
    {
        return await this.Db.Deleteable<LoginSessionEntity>()
            .Where(l => l.LoginedToken == loginedToken)
            .ExecuteAsync()
            .ConfigureAwait(false) > 0;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<bool> DeleteByAccountAsync(string account)
    {
        return await this.Db.Deleteable<LoginSessionEntity>()
            .Where(l => l.Account == account)
            .ExecuteAsync()
            .ConfigureAwait(false) > 0;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<bool> TokenExistsAsync(string loginedToken)
    {
        return await this.Db.Queryable<LoginSessionEntity>()
            .Where(l => l.LoginedToken == loginedToken)
            .AnyAsync()
            .ConfigureAwait(false);
    }
}
