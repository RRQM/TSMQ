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
using System.Security.Claims;
using TSMQServer.Core.Imp.Attributes;
using TSMQServer.Db;
using TouchSocket.Core;
using TouchSocket.Rpc;

namespace TSMQServer.Core.Imp.Services;

[Authorize]
[ExceptionFilter]
[LoggerFilter]
public partial class UserManagerService : TransientRpcServer, IUserManagerService
{
    private readonly RpcPermissionEvaluator m_permissions;
    private readonly RpcStore m_rpcStore;
    private readonly IJwtService m_jwtService;
    private readonly ILogger<UserManagerService> m_logger;
    private readonly IRoleManagerFromDB m_roleManager;
    private readonly IUserManagerFromDB m_userManager;

    public UserManagerService(ILogger<UserManagerService> logger, IUserManagerFromDB userManager, IJwtService jwtService, IRoleManagerFromDB roleManager, RpcPermissionEvaluator permissions, RpcStore rpcStore)
    {
        this.m_permissions = permissions;
        this.m_rpcStore = rpcStore;
        this.m_logger = logger;
        this.m_userManager = userManager;
        this.m_jwtService = jwtService;
        this.m_roleManager = roleManager;
    }

    public async Task<ResponseBase> AssignUserRoles(AssignUserRolesRequest request)
    {
        var response = new ResponseBase();
        var target = await this.m_userManager.GetUserByIdAsync(request.UserId);
        var assignedRoles = await this.m_roleManager.GetRolesByIdsAsync(request.RoleIds ?? []);
        if (target == null || assignedRoles.Count != (request.RoleIds ?? []).Distinct().Count()) return response.FromFail("用户或角色不存在");
        if (target.Account == "admin" && !assignedRoles.Any(r => r.RoleCode == "admin" && r.IsEnabled))
            return response.FromFail("内置管理员必须保留管理员角色");
        LogAssigningUserRoles(this.m_logger, request.UserId, (request.RoleIds ?? []).Length);
        var roleIds = string.Join(",", request.RoleIds ?? []);
        var result = await this.m_userManager.AssignUserRolesAsync(request.UserId, roleIds);
        if (!result)
        {
            LogAssignUserRolesFailed(this.m_logger, request.UserId);
            return response.FromFail("分配角色失败");
        }

        LogAssignUserRolesSuccess(this.m_logger, request.UserId);
        return response.FromSuccess("分配角色成功");
    }

    public async Task<ResponseBase> ChangePassword(ChangePasswordRequest request)
    {
        var response = new ResponseBase();
        if (string.IsNullOrEmpty(request.OldPassword) || string.IsNullOrEmpty(request.NewPassword))
        {
            return response.FromFail("旧密码和新密码均不能为空");
        }

        var (isJwtValid, principal) = this.m_jwtService.ValidateToken(request.LoginedToken!);
        if (!isJwtValid || principal == null)
        {
            return response.FromFail("令牌无效");
        }

        var account = principal.FindFirst(ClaimTypes.Name)?.Value;
        var getUserResult = await this.m_userManager.TryGetRegisteredInfoAsync(account!);
        if (!getUserResult.success || getUserResult.userInfo == null)
        {
            return response.FromFail("用户不存在");
        }

        if (!request.OldPassword.Equals(getUserResult.userInfo.Password))
        {
            LogChangePasswordWrongOldPassword(this.m_logger, account);
            return response.FromFail("旧密码不正确");
        }

        var changed = await this.m_userManager.ChangePasswordAsync(getUserResult.userInfo.Id, request.NewPassword);
        if (!changed)
        {
            LogChangePasswordFailed(this.m_logger, account);
            return response.FromFail("修改密码失败");
        }

        LogChangePasswordSuccess(this.m_logger, account);
        return response.FromSuccess("修改密码成功");
    }

    public async Task<GetUserInfoResponse> GetUserInfo(LoginedRequest request)
    {
        var response = new GetUserInfoResponse();
        var (isJwtValid, principal) = this.m_jwtService.ValidateToken(request.LoginedToken!);
        if (!isJwtValid || principal == null)
        {
            return response.FromFail("令牌无效");
        }

        var userIdStr = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(userIdStr, out var userId))
        {
            return response.FromFail("用户身份无效");
        }

        var userInfo = await this.m_userManager.GetUserByIdAsync(userId);
        if (userInfo == null)
        {
            return response.FromFail("用户不存在");
        }

        response.User = userInfo;
        var current = this.CallContext.GetValue(AuthorizeAttribute.JwtPrincipalProperty)!;
        foreach (var method in this.m_rpcStore.GetAllMethods())
        {
            if (await this.m_permissions.IsAllowedAsync(current, method.ServerToType.Name, method.Name))
                response.Permissions.Add($"{method.ServerToType.Name}.{method.Name}");
        }
        return response.FromSuccess("获取成功");
    }

    public async Task<GetUserListResponse> GetUserList(LoginedRequest request)
    {
        LogGettingUserList(this.m_logger);
        var response = new GetUserListResponse();
        var users = await this.m_userManager.GetUserListAsync();
        response.Users = users.Select(u => new UserInfo
        {
            Id = u.Id,
            Account = u.Account,
            UserName = u.UserName,
            Email = u.Email,
            Phone = u.Phone,
            RoleIds = u.RoleIds,
            IsEnabled = u.IsEnabled,
            CreateTime = u.CreateTime
        }).ToList();
        LogGetUserListSuccess(this.m_logger, response.Users.Count);
        return response.FromSuccess("获取成功");
    }

    [AllowAnonymous]
    public async Task<LoginResponse> Login(LoginRequest request)
    {
        LogLoggingIn(this.m_logger, request.Account);

        var response = new LoginResponse();
        if (request.Account.IsNullOrEmpty() || request.Password.IsNullOrEmpty())
        {
            LogLoginEmptyAccountOrPassword(this.m_logger);
            return response.FromFail("账号或密码均不能为空");
        }

        var (success, userInfo) = await this.m_userManager.TryGetRegisteredInfoAsync(request.Account);
        if (!success || userInfo == null)
        {
            LogLoginAccountNotRegistered(this.m_logger, request.Account);
            return response.FromFail("没有该账号的注册信息");
        }

        if (!userInfo.IsEnabled) return response.FromFail("账号或密码不正确");

        if (!request.Password.Equals(userInfo.Password))
        {
            LogLoginWrongPassword(this.m_logger, request.Account);
            return response.FromFail("账号或密码不正确");
        }

        var loginedToken = Guid.NewGuid().ToString();
        LogGeneratingLoginToken(this.m_logger, loginedToken, request.Account);

        await this.m_userManager.LoginAsync(loginedToken, userInfo);
        response.LoginedToken = loginedToken;

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userInfo.Id.ToString()),
            new Claim(ClaimTypes.Name, userInfo.Account),
            new Claim("LoginToken", loginedToken)
        };

        if (!string.IsNullOrEmpty(userInfo.RoleIds))
        {
            var roleIdList = userInfo.RoleIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(long.Parse).ToList();
            var roles = await this.m_roleManager.GetRolesByIdsAsync(roleIdList);

            foreach (var role in roles.Where(r => r.IsEnabled))
            {
                if (!string.IsNullOrEmpty(role.RoleCode))
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.RoleCode));
                }
            }
        }

        response.LoginedToken = this.m_jwtService.GenerateToken(claims);

        response.FromSuccess("登录成功");

        LogLoginSuccess(this.m_logger, request.Account, userInfo.Id);
        return response;
    }

    public async Task<RegisterResponse> Register(RegisterRequest request)
    {
        LogRegistering(this.m_logger, request.Account);

        var response = new RegisterResponse();
        if (request.Account.IsNullOrEmpty() || request.Password.IsNullOrEmpty())
        {
            LogRegisterEmptyAccountOrPassword(this.m_logger);
            return response.FromFail("账号或密码均不能为空");
        }

        var (exists, _) = await this.m_userManager.TryGetRegisteredInfoAsync(request.Account);
        if (exists)
        {
            LogRegisterAccountExists(this.m_logger, request.Account);
            return response.FromFail("该账号已被注册");
        }

        await this.m_userManager.RegisterAsync(new UserInfo()
        {
            Account = request.Account,
            Password = request.Password
        });

        LogRegisterSuccess(this.m_logger, request.Account);
        return response.FromSuccess("注册成功");
    }

    /// <inheritdoc/>
    public async Task<ResponseBase> Logout(LoginedRequest request)
    {
        var principal = this.CallContext.GetValue(AuthorizeAttribute.JwtPrincipalProperty)!;
        await this.m_userManager.LogoutAsync(principal.FindFirst("LoginToken")!.Value);
        return new ResponseBase().FromSuccess("已退出登录");
    }

    public async Task<ResponseBase> DeleteUser(DeleteUserRequest request)
    {
        var response = new ResponseBase();
        var target = await this.m_userManager.GetUserByIdAsync(request.UserId);
        if (target?.Account == "admin") return response.FromFail("不能删除内置管理员");
        var deleted = await this.m_userManager.DeleteUserAsync(request.UserId);
        if (!deleted)
        {
            return response.FromFail("用户不存在或删除失败");
        }
        return response.FromSuccess("删除成功");
    }
}
