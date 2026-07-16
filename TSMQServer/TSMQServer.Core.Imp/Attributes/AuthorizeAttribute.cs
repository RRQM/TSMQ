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

using System.Reflection;
using System.Security.Claims;
using TSMQServer.Db;
using TouchSocket.Core;
using TouchSocket.Rpc;

namespace TSMQServer.Core.Imp.Attributes;

/// <summary>
/// 鉴权特性，集 JWT 验证与动态角色授权于一体。
/// <para>
/// 标注在服务类上时，该类所有方法均需通过鉴权；
/// 若某方法标注了 <see cref="AllowAnonymousAttribute"/>，则跳过鉴权。
/// </para>
/// <para>
/// 动态角色授权优先匹配方法级键（<c>ClassName.MethodName</c>），
/// 若无配置则回退至类级键（<c>ClassName</c>）。数据库未配置角色时不限制访问。
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class AuthorizeAttribute : RpcActionFilterAttribute
{
    /// <summary>
    /// 用于在调用上下文中存储已验证的 <see cref="ClaimsPrincipal"/>
    /// </summary>
    public static readonly DependencyProperty<ClaimsPrincipal?> JwtPrincipalProperty =
        new DependencyProperty<ClaimsPrincipal?>("JwtPrincipal", null);

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override async Task<InvokeResult> ExecutingAsync(ICallContext callContext, object[] parameters, InvokeResult invokeResult)
    {
        // 若接口方法或实现类方法标注了 AllowAnonymous，跳过鉴权
        var methodInfo = callContext.RpcMethod?.Info;
        var toMethodInfo = callContext.RpcMethod?.ToMethodInfo;
        if (methodInfo?.GetCustomAttribute<AllowAnonymousAttribute>(false) != null)
        {
            return await base.ExecutingAsync(callContext, parameters, invokeResult);
        }

        if (toMethodInfo?.GetCustomAttribute<AllowAnonymousAttribute>(false) != null)
        {
            return await base.ExecutingAsync(callContext, parameters, invokeResult);
        }

        var jwtService = callContext.Resolver.Resolve<IJwtService>();
        if (jwtService == null)
        {
            invokeResult.Status = InvokeStatus.Success;
            invokeResult.Result = new ResponseBase() { Code = ResponseCode.Error, Message = "JWT服务未配置" };
            return invokeResult;
        }

        var token = GetTokenFromParameters(parameters);
        if (string.IsNullOrEmpty(token))
        {
            invokeResult.Status = InvokeStatus.Success;
            invokeResult.Result = new ResponseBase() { Code = ResponseCode.Error, Message = "未提供JWT令牌" };
            return invokeResult;
        }

        var (isValid, principal) = jwtService.ValidateToken(token);
        if (!isValid || principal == null)
        {
            invokeResult.Status = InvokeStatus.Success;
            invokeResult.Result = new ResponseBase() { Code = ResponseCode.Error, Message = "JWT令牌无效或已过期" };
            return invokeResult;
        }

        callContext.SetValue(JwtPrincipalProperty, principal);

        // 动态角色授权
        var rpcAuthorizationFromDB = callContext.Resolver.Resolve<IRpcAuthorizationFromDB>();
        if (rpcAuthorizationFromDB != null)
        {
            var className = callContext.RpcMethod?.ServerToType?.Name;
            var methodName = callContext.RpcMethod?.Name;

            string[]? requiredRoles = null;

            if (className != null && methodName != null)
            {
                requiredRoles = await rpcAuthorizationFromDB.GetRolesForTargetAsync($"{className}.{methodName}");
            }

            if ((requiredRoles == null || requiredRoles.Length == 0) && className != null)
            {
                requiredRoles = await rpcAuthorizationFromDB.GetRolesForTargetAsync(className);
            }

            if (requiredRoles != null && requiredRoles.Length > 0)
            {
                if (!requiredRoles.Any(r => principal.IsInRole(r)))
                {
                    invokeResult.Status = InvokeStatus.Success;
                    invokeResult.Result = new ResponseBase() { Code = ResponseCode.Error, Message = "权限不足，缺少所需角色" };
                    return invokeResult;
                }
            }
        }

        return await base.ExecutingAsync(callContext, parameters, invokeResult);
    }

    private static string? GetTokenFromParameters(object[] parameters)
    {
        if (parameters == null || parameters.Length == 0)
        {
            return null;
        }

        foreach (var param in parameters)
        {
            if (param is LoginedRequest loginedRequest)
            {
                return loginedRequest.LoginedToken;
            }
        }

        return null;
    }
}
