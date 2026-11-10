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
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TSMQServer.Core.Imp.Services.Jwt;

/// <summary>
/// JWT服务实现
/// </summary>
public partial class JwtService : IJwtService
{
    private readonly ILogger<JwtService> m_logger;
    private readonly JwtSettings m_settings;
    private readonly JwtSecurityTokenHandler m_tokenHandler;
    private readonly TokenValidationParameters m_validationParameters;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public JwtService(ILogger<JwtService> logger, IOptions<JwtSettings> options)
    {
        this.m_logger = logger;
        this.m_settings = options.Value;
        this.m_tokenHandler = new JwtSecurityTokenHandler();

        var key = Encoding.UTF8.GetBytes(this.m_settings.SecretKey ?? throw new ArgumentNullException("JWT SecretKey"));

        this.m_validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = this.m_settings.Issuer,
            ValidateAudience = true,
            ValidAudience = this.m_settings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        LogJwtServiceInitialized(this.m_logger, this.m_settings.Issuer, this.m_settings.Audience, this.m_settings.ExpirationMinutes);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string GenerateToken(IEnumerable<Claim> claims)
    {
        LogGeneratingJwtToken(this.m_logger, claims.Count());

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.m_settings.SecretKey!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: this.m_settings.Issuer,
            audience: this.m_settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(this.m_settings.ExpirationMinutes),
            signingCredentials: credentials
        );

        var tokenString = this.m_tokenHandler.WriteToken(token);
        LogJwtTokenGenerated(this.m_logger, token.ValidTo);
        return tokenString;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public (bool IsValid, ClaimsPrincipal? Principal) ValidateToken(string token)
    {
        try
        {
            LogValidatingJwtToken(this.m_logger);
            var principal = this.m_tokenHandler.ValidateToken(token, this.m_validationParameters, out _);
            LogJwtTokenValidated(this.m_logger);
            return (true, principal);
        }
        catch (Exception ex)
        {
            LogJwtTokenValidationFailed(this.m_logger, ex);
            return (false, null);
        }
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string? GetUserIdFromToken(string token)
    {
        LogGettingUserIdFromToken(this.m_logger);

        var (isValid, principal) = this.ValidateToken(token);
        if (!isValid || principal == null)
        {
            LogGetUserIdFromTokenInvalidToken(this.m_logger);
            return null;
        }

        var userId = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            LogGetUserIdFromTokenSuccess(this.m_logger, userId);
        }
        else
        {
            LogGetUserIdFromTokenMissingClaim(this.m_logger);
        }

        return userId;
    }
}
