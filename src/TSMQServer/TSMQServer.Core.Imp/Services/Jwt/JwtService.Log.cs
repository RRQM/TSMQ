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

namespace TSMQServer.Core.Imp.Services.Jwt;

public partial class JwtService
{
    [LoggerMessage(Level = LogLevel.Information, Message = "JWT服务初始化成功，发行者：{Issuer}，受众：{Audience}，过期时间：{Expiration}分钟")]
    private static partial void LogJwtServiceInitialized(ILogger logger, string? issuer, string? audience, int expiration);

    [LoggerMessage(Level = LogLevel.Debug, Message = "开始生成JWT令牌，声明数量：{ClaimsCount}")]
    private static partial void LogGeneratingJwtToken(ILogger logger, int claimsCount);

    [LoggerMessage(Level = LogLevel.Information, Message = "JWT令牌生成成功，过期时间：{ExpiresAt}")]
    private static partial void LogJwtTokenGenerated(ILogger logger, DateTime expiresAt);

    [LoggerMessage(Level = LogLevel.Debug, Message = "开始验证JWT令牌")]
    private static partial void LogValidatingJwtToken(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "JWT令牌验证成功")]
    private static partial void LogJwtTokenValidated(ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "JWT令牌验证失败")]
    private static partial void LogJwtTokenValidationFailed(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Debug, Message = "开始从令牌中获取用户ID")]
    private static partial void LogGettingUserIdFromToken(ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "从令牌中获取用户ID失败，令牌无效")]
    private static partial void LogGetUserIdFromTokenInvalidToken(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "从令牌中成功获取用户ID：{UserId}")]
    private static partial void LogGetUserIdFromTokenSuccess(ILogger logger, string? userId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "从令牌中获取用户ID失败，找不到NameIdentifier声明")]
    private static partial void LogGetUserIdFromTokenMissingClaim(ILogger logger);
}
