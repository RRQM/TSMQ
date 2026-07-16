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

namespace TSMQServer.Core.Imp.Services;

public partial class RoleManagerService
{
    [LoggerMessage(Level = LogLevel.Information, Message = "开始创建角色，角色名称：{RoleName}，角色编码：{RoleCode}")]
    private static partial void LogCreatingRole(ILogger logger, string? roleName, string? roleCode);

    [LoggerMessage(Level = LogLevel.Warning, Message = "创建角色失败，角色名称或编码为空")]
    private static partial void LogCreateRoleEmptyNameOrCode(ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "创建角色失败，角色编码：{RoleCode} 已存在")]
    private static partial void LogCreateRoleCodeExists(ILogger logger, string? roleCode);

    [LoggerMessage(Level = LogLevel.Information, Message = "创建角色成功，角色Id：{RoleId}，角色名称：{RoleName}，角色编码：{RoleCode}")]
    private static partial void LogCreateRoleSuccess(ILogger logger, long roleId, string? roleName, string? roleCode);

    [LoggerMessage(Level = LogLevel.Information, Message = "开始更新角色，角色Id：{RoleId}")]
    private static partial void LogUpdatingRole(ILogger logger, long roleId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "更新角色失败，角色Id：{RoleId} 不存在")]
    private static partial void LogUpdateRoleNotFound(ILogger logger, long roleId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "更新角色名称，原名称：{OldName}，新名称：{NewName}")]
    private static partial void LogUpdateRoleName(ILogger logger, string? oldName, string? newName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "更新角色描述，角色：{RoleName}")]
    private static partial void LogUpdateRoleDescription(ILogger logger, string? roleName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "更新角色状态，角色：{RoleName}，状态：{IsEnabled}")]
    private static partial void LogUpdateRoleStatus(ILogger logger, string? roleName, bool isEnabled);

    [LoggerMessage(Level = LogLevel.Information, Message = "更新角色成功，角色Id：{RoleId}，角色名称：{RoleName}")]
    private static partial void LogUpdateRoleSuccess(ILogger logger, long roleId, string? roleName);

    [LoggerMessage(Level = LogLevel.Information, Message = "开始删除角色，角色Id：{RoleId}")]
    private static partial void LogDeletingRole(ILogger logger, long roleId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "删除角色失败，角色Id：{RoleId} 不存在")]
    private static partial void LogDeleteRoleNotFound(ILogger logger, long roleId);

    [LoggerMessage(Level = LogLevel.Information, Message = "删除角色成功，角色Id：{RoleId}")]
    private static partial void LogDeleteRoleSuccess(ILogger logger, long roleId);

    [LoggerMessage(Level = LogLevel.Information, Message = "开始获取角色列表")]
    private static partial void LogGettingRoleList(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "获取角色列表成功，总数：{Count}")]
    private static partial void LogGetRoleListSuccess(ILogger logger, int count);
}
