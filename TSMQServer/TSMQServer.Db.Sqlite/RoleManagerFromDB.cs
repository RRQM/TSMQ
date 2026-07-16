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

using Microsoft.Extensions.Logging;
using TSMQServer.Core;
using TSMQServer.Db.Sqlite.Entities;
using TSMQServer.Db.Sqlite.Repositories;

namespace TSMQServer.Db.Sqlite;

/// <summary>
/// 角色管理数据库实现
/// </summary>
public class RoleManagerFromDB : IRoleManagerFromDB
{
    private readonly ILogger<RoleManagerFromDB> m_logger;
    private readonly RoleRepository m_roleRepository;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public RoleManagerFromDB(ILogger<RoleManagerFromDB> logger, RoleRepository roleRepository)
    {
        this.m_logger = logger;
        this.m_roleRepository = roleRepository;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<RoleInfo> CreateRoleAsync(RoleInfo role)
    {
        var entity = new RoleEntity
        {
            RoleName = role.RoleName,
            RoleCode = role.RoleCode,
            Description = role.Description,
            IsEnabled = role.IsEnabled ? 1 : 0,
            CreateTime = DateTime.Now
        };

        var id = await this.m_roleRepository.InsertAsync(entity);
        role.Id = id;
        role.CreateTime = entity.CreateTime;

        this.m_logger.LogInformation($"创建角色: {role.RoleName}");
        return role;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<bool> UpdateRoleAsync(RoleInfo role)
    {
        var entity = new RoleEntity
        {
            Id = role.Id,
            RoleName = role.RoleName,
            RoleCode = role.RoleCode,
            Description = role.Description,
            IsEnabled = role.IsEnabled ? 1 : 0
        };

        var result = await this.m_roleRepository.UpdateAsync(entity);
        if (result)
        {
            this.m_logger.LogInformation($"更新角色: {role.RoleName}");
        }
        return result;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<bool> DeleteRoleAsync(long roleId)
    {
        var result = await this.m_roleRepository.DeleteAsync(roleId);
        if (result)
        {
            this.m_logger.LogInformation($"删除角色: RoleId={roleId}");
        }
        return result;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<RoleInfo?> GetRoleByIdAsync(long roleId)
    {
        var entity = await this.m_roleRepository.GetByIdAsync(roleId);
        if (entity == null)
        {
            return null;
        }

        return this.EntityToDto(entity);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<RoleInfo?> GetRoleByCodeAsync(string roleCode)
    {
        var entity = await this.m_roleRepository.GetByCodeAsync(roleCode);
        if (entity == null)
        {
            return null;
        }

        return this.EntityToDto(entity);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<List<RoleInfo>> GetAllRolesAsync()
    {
        var entities = await this.m_roleRepository.GetAllAsync();
        return entities.Select(this.EntityToDto).ToList();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<List<RoleInfo>> GetRolesByIdsAsync(IEnumerable<long> roleIds)
    {
        var entities = await this.m_roleRepository.GetByIdsAsync(roleIds);
        return entities.Select(this.EntityToDto).ToList();
    }

    private RoleInfo EntityToDto(RoleEntity entity)
    {
        return new RoleInfo
        {
            Id = entity.Id,
            RoleName = entity.RoleName,
            RoleCode = entity.RoleCode,
            Description = entity.Description,
            IsEnabled = entity.IsEnabled == 1,
            CreateTime = entity.CreateTime
        };
    }
}
