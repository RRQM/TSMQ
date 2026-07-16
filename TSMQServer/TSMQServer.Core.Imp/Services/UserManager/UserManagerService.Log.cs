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

public partial class UserManagerService
{
    [LoggerMessage(Level = LogLevel.Information, Message = "开始登录，账号：{Account}")]
    private static partial void LogLoggingIn(ILogger logger, string? account);

    [LoggerMessage(Level = LogLevel.Warning, Message = "登录失败，账号或密码为空")]
    private static partial void LogLoginEmptyAccountOrPassword(ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "登录失败，账号：{Account} 未注册")]
    private static partial void LogLoginAccountNotRegistered(ILogger logger, string? account);

    [LoggerMessage(Level = LogLevel.Warning, Message = "登录失败，账号：{Account} 密码错误")]
    private static partial void LogLoginWrongPassword(ILogger logger, string? account);

    [LoggerMessage(Level = LogLevel.Debug, Message = "生成登录令牌：{LoginToken}，账号：{Account}")]
    private static partial void LogGeneratingLoginToken(ILogger logger, string loginToken, string? account);

    [LoggerMessage(Level = LogLevel.Information, Message = "账号：{Account} (Id:{UserId}) 登录成功")]
    private static partial void LogLoginSuccess(ILogger logger, string? account, long userId);

    [LoggerMessage(Level = LogLevel.Information, Message = "开始注册，账号：{Account}")]
    private static partial void LogRegistering(ILogger logger, string? account);

    [LoggerMessage(Level = LogLevel.Warning, Message = "注册失败，账号或密码为空")]
    private static partial void LogRegisterEmptyAccountOrPassword(ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "注册失败，账号：{Account} 已被注册")]
    private static partial void LogRegisterAccountExists(ILogger logger, string? account);

    [LoggerMessage(Level = LogLevel.Information, Message = "账号：{Account} 注册成功")]
    private static partial void LogRegisterSuccess(ILogger logger, string? account);

    [LoggerMessage(Level = LogLevel.Information, Message = "开始获取用户列表")]
    private static partial void LogGettingUserList(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "获取用户列表成功，共 {Count} 个用户")]
    private static partial void LogGetUserListSuccess(ILogger logger, int count);

    [LoggerMessage(Level = LogLevel.Warning, Message = "修改密码失败，账号：{Account} 旧密码错误")]
    private static partial void LogChangePasswordWrongOldPassword(ILogger logger, string? account);

    [LoggerMessage(Level = LogLevel.Error, Message = "修改密码失败，账号：{Account}")]
    private static partial void LogChangePasswordFailed(ILogger logger, string? account);

    [LoggerMessage(Level = LogLevel.Information, Message = "账号：{Account} 修改密码成功")]
    private static partial void LogChangePasswordSuccess(ILogger logger, string? account);

    [LoggerMessage(Level = LogLevel.Information, Message = "开始分配用户角色，用户ID：{UserId}，角色数：{RoleCount}")]
    private static partial void LogAssigningUserRoles(ILogger logger, long userId, int roleCount);

    [LoggerMessage(Level = LogLevel.Error, Message = "分配用户角色失败，用户ID：{UserId}")]
    private static partial void LogAssignUserRolesFailed(ILogger logger, long userId);

    [LoggerMessage(Level = LogLevel.Information, Message = "分配用户角色成功，用户ID：{UserId}")]
    private static partial void LogAssignUserRolesSuccess(ILogger logger, long userId);
}
