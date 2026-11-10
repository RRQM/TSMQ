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
using TSMQServer.Db.Sqlite.Entities;
using TSMQServer.Db.Sqlite.Repositories;

namespace TSMQServer.Db.Sqlite;

/// <summary>
/// RPC动态角色授权数据库实现（Sqlite）
/// </summary>
public class RpcAuthorizationFromDB : IRpcAuthorizationFromDB
{
    private readonly RpcMethodRoleRepository m_repository;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public RpcAuthorizationFromDB(RpcMethodRoleRepository repository)
    {
        this.m_repository = repository;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<string[]> GetRolesForTargetAsync(string targetKey)
    {
        var entity = await this.m_repository.GetByTargetKeyAsync(targetKey);
        if (entity == null || string.IsNullOrWhiteSpace(entity.RoleCodes))
        {
            return [];
        }

        return entity.RoleCodes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task SetRolesForTargetAsync(string targetKey, string[] roleCodes, string? description)
    {
        var entity = new RpcMethodRoleEntity
        {
            TargetKey = targetKey,
            RoleCodes = string.Join(",", roleCodes),
            Description = description,
            UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        await this.m_repository.UpsertAsync(entity);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<bool> RemoveRolesForTargetAsync(string targetKey)
    {
        return await this.m_repository.DeleteAsync(targetKey);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<List<RpcMethodRoleInfo>> GetAllAsync()
    {
        var entities = await this.m_repository.GetAllAsync();
        return entities.Select(e => new RpcMethodRoleInfo
        {
            TargetKey = e.TargetKey,
            RoleCodes = string.IsNullOrWhiteSpace(e.RoleCodes)
                ? []
                : e.RoleCodes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            Description = e.Description,
            UpdateTime = DateTime.TryParse(e.UpdateTime, out var dt) ? dt : default
        }).ToList();
    }
}
