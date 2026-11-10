using System.Security.Claims;
using TSMQServer.Db;

namespace TSMQServer.Core.Imp.Services;

/// <summary>使用当前数据库角色计算 RPC 权限，未配置时默认拒绝。</summary>
public sealed class RpcPermissionEvaluator(IRoleManagerFromDB roles, IRpcAuthorizationFromDB authorization)
{
    /// <summary>从启用角色重建身份，防止旧 JWT 角色继续生效。</summary>
    public async Task<ClaimsPrincipal> RefreshAsync(ClaimsPrincipal principal, UserInfo user)
    {
        var identity = new ClaimsIdentity(principal.Claims.Where(c => c.Type != ClaimTypes.Role), "TSMQ");
        var ids = (user.RoleIds ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => long.TryParse(s, out var id) ? id : 0).Where(id => id > 0);
        foreach (var role in await roles.GetRolesByIdsAsync(ids))
            if (role.IsEnabled && !string.IsNullOrEmpty(role.RoleCode)) identity.AddClaim(new Claim(ClaimTypes.Role, role.RoleCode));
        return new ClaimsPrincipal(identity);
    }

    /// <summary>判断当前身份是否可以调用方法。</summary>
    public async Task<bool> IsAllowedAsync(ClaimsPrincipal principal, string service, string method)
    {
        if (principal.IsInRole("admin")) return true;
        if (service == "UserManagerService" && method is "GetUserInfo" or "ChangePassword" or "Logout") return true;
        var required = await authorization.GetRolesForTargetAsync($"{service}.{method}");
        if (required.Length == 0) required = await authorization.GetRolesForTargetAsync(service);
        return required.Length > 0 && required.Any(principal.IsInRole);
    }
}
