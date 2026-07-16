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
using TSMQServer.Core.Imp.Attributes;
using TSMQServer.Db;
using TouchSocket.Rpc;

namespace TSMQServer.Core.Imp.Services;

/// <summary>
/// 角色管理服务实现
/// </summary>
[ExceptionFilter]
[LoggerFilter]
[Authorize]
public partial class RoleManagerService : TransientRpcServer, IRoleManagerService
{
    private readonly ILogger<RoleManagerService> m_logger;
    private readonly IRoleManagerFromDB m_roleManager;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public RoleManagerService(ILogger<RoleManagerService> logger, IRoleManagerFromDB roleManager)
    {
        this.m_logger = logger;
        this.m_roleManager = roleManager;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<RoleResponse> CreateRole(CreateRoleRequest request)
    {
        LogCreatingRole(this.m_logger, request.RoleName, request.RoleCode);

        var response = new RoleResponse();

        if (string.IsNullOrEmpty(request.RoleName) || string.IsNullOrEmpty(request.RoleCode))
        {
            LogCreateRoleEmptyNameOrCode(this.m_logger);
            return response.FromFail("角色名称和角色编码不能为空");
        }

        var existing = await this.m_roleManager.GetRoleByCodeAsync(request.RoleCode);
        if (existing != null)
        {
            LogCreateRoleCodeExists(this.m_logger, request.RoleCode);
            return response.FromFail("角色编码已存在");
        }

        var roleDto = new RoleInfo
        {
            RoleName = request.RoleName,
            RoleCode = request.RoleCode,
            Description = request.Description
        };

        var created = await this.m_roleManager.CreateRoleAsync(roleDto);

        response.Role = new RoleInfo
        {
            Id = created.Id,
            RoleName = created.RoleName,
            RoleCode = created.RoleCode,
            Description = created.Description,
            IsEnabled = created.IsEnabled,
            CreateTime = created.CreateTime
        };

        LogCreateRoleSuccess(this.m_logger, created.Id, created.RoleName, created.RoleCode);
        return response.FromSuccess("创建角色成功");
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<ResponseBase> UpdateRole(UpdateRoleRequest request)
    {
        LogUpdatingRole(this.m_logger, request.RoleId);

        var response = new ResponseBase();

        var roleDto = await this.m_roleManager.GetRoleByIdAsync(request.RoleId);
        if (roleDto == null)
        {
            LogUpdateRoleNotFound(this.m_logger, request.RoleId);
            return response.FromFail("角色不存在");
        }

        if (!string.IsNullOrEmpty(request.RoleName))
        {
            LogUpdateRoleName(this.m_logger, roleDto.RoleName, request.RoleName);
            roleDto.RoleName = request.RoleName;
        }

        if (request.Description != null)
        {
            LogUpdateRoleDescription(this.m_logger, roleDto.RoleName);
            roleDto.Description = request.Description;
        }

        if (request.IsEnabled.HasValue)
        {
            LogUpdateRoleStatus(this.m_logger, roleDto.RoleName, request.IsEnabled.Value);
            roleDto.IsEnabled = request.IsEnabled.Value;
        }

        await this.m_roleManager.UpdateRoleAsync(roleDto);

        LogUpdateRoleSuccess(this.m_logger, roleDto.Id, roleDto.RoleName);
        return response.FromSuccess("更新角色成功");
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<ResponseBase> DeleteRole(DeleteRoleRequest request)
    {
        LogDeletingRole(this.m_logger, request.RoleId);

        var response = new ResponseBase();

        var result = await this.m_roleManager.DeleteRoleAsync(request.RoleId);
        if (!result)
        {
            LogDeleteRoleNotFound(this.m_logger, request.RoleId);
            return response.FromFail("角色不存在");
        }

        LogDeleteRoleSuccess(this.m_logger, request.RoleId);
        return response.FromSuccess("删除角色成功");
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<RoleListResponse> GetRoleList(GetRoleListRequest request)
    {
        LogGettingRoleList(this.m_logger);

        var roleDtos = await this.m_roleManager.GetAllRolesAsync();

        var response = new RoleListResponse
        {
            Roles = roleDtos.Select(dto => new RoleInfo
            {
                Id = dto.Id,
                RoleName = dto.RoleName,
                RoleCode = dto.RoleCode,
                Description = dto.Description,
                IsEnabled = dto.IsEnabled,
                CreateTime = dto.CreateTime
            }).ToList()
        };

        LogGetRoleListSuccess(this.m_logger, response.Roles.Count);
        return response.FromSuccess("获取角色列表成功");
    }

}
