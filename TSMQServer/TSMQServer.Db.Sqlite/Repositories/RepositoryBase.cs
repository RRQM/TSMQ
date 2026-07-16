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

using System.Diagnostics.CodeAnalysis;
using TORM;

namespace TSMQServer.Db.Sqlite.Repositories;

/// <summary>
/// 仓储基类，提供通用的增删改查实现。
/// </summary>
/// <typeparam name="TEntity">实体类型，需为引用类型且具备无参构造函数。</typeparam>
public abstract class RepositoryBase<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TEntity>
    where TEntity : class, new()
{
    /// <summary>
    /// ORM 客户端实例。
    /// </summary>
    protected readonly OrmClient Db;

    /// <summary>
    /// 初始化 <see cref="RepositoryBase{TEntity}"/>。
    /// </summary>
    protected RepositoryBase(OrmClient db)
    {
        this.Db = db;
    }

    /// <summary>
    /// 获取所有实体。
    /// </summary>
    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await this.Db.Queryable<TEntity>()
            .ToListAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// 插入实体。
    /// </summary>
    public virtual async Task<bool> InsertAsync(TEntity entity)
    {
        return await this.Db.Insertable(entity)
            .ExecuteAsync()
            .ConfigureAwait(false) > 0;
    }

    /// <summary>
    /// 更新实体。
    /// </summary>
    public virtual async Task<bool> UpdateAsync(TEntity entity)
    {
        return await this.Db.Updatable(entity)
            .ExecuteAsync()
            .ConfigureAwait(false) > 0;
    }
}
