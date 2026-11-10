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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Text.Json;
using TouchSocket.Core;
using TSMQClient.Mvvm.Base;
using TSMQServer.Core;
using TSMQServer.Core.Clients;

namespace TSMQClient.Mvvm;

public partial class MainViewModel : ViewModelBase
{
    public MainViewModel(IMessenger messenger, IRemoteService remoteService) : base(messenger)
    {
        this.m_remoteService = remoteService;
    }

    #region 字段

    private readonly IRemoteService m_remoteService;
    private static readonly JsonSerializerOptions s_jsonOptions = new JsonSerializerOptions { WriteIndented = true };

    #endregion 字段

    #region 认证属性

    [ObservableProperty]
    private string? account;

    [ObservableProperty]
    private string? passsword;

    [ObservableProperty]
    private string? statusMessage;

    #endregion 认证属性

    #region 用户管理属性

    [ObservableProperty]
    private string? userResult;

    [ObservableProperty]
    private string? oldPassword;

    [ObservableProperty]
    private string? newPassword;

    [ObservableProperty]
    private string? deleteUserId;

    #endregion 用户管理属性

    #region 角色管理属性

    [ObservableProperty]
    private string? roleResult;

    [ObservableProperty]
    private string? newRoleName;

    [ObservableProperty]
    private string? newRoleCode;

    [ObservableProperty]
    private string? newRoleDescription;

    [ObservableProperty]
    private string? updateRoleId;

    [ObservableProperty]
    private string? updateRoleName;

    [ObservableProperty]
    private string? updateRoleDescription;

    [ObservableProperty]
    private bool updateRoleIsEnabled = true;

    [ObservableProperty]
    private string? deleteRoleId;

    [ObservableProperty]
    private string? assignUserId;

    [ObservableProperty]
    private string? assignRoleIds;

    #endregion 角色管理属性

    #region RPC授权属性

    [ObservableProperty]
    private string? rpcResult;

    [ObservableProperty]
    private string? getRpcTargetKey;

    [ObservableProperty]
    private string? setRpcTargetKey;

    [ObservableProperty]
    private string? setRpcRoleCodes;

    [ObservableProperty]
    private string? setRpcDescription;

    [ObservableProperty]
    private string? removeRpcTargetKey;

    #endregion RPC授权属性

    #region 日志管理属性

    [ObservableProperty]
    private string? logResult;

    [ObservableProperty]
    private string? logLevel;

    [ObservableProperty]
    private string? logCategory;

    [ObservableProperty]
    private string? logKeyword;

    [ObservableProperty]
    private int logTake = 100;

    #endregion 日志管理属性

    #region 认证 Command

    [RelayCommand]
    private async Task LoginAsync(CancellationToken cancellationToken)
    {
        var account = this.Account;
        var passsword = this.Passsword;
        if (account.IsNullOrEmpty())
        {
            this.StatusMessage = "请输入账号";
            return;
        }
        if (passsword.IsNullOrEmpty())
        {
            this.StatusMessage = "请输入密码";
            return;
        }
        this.StatusMessage = "正在登录...";
        var result = await this.m_remoteService.LoginAsync(account, passsword, cancellationToken);
        this.StatusMessage = result.IsSuccess ? "登录成功" : $"登录失败: {result.Message}";
    }

    [RelayCommand]
    private async Task RegisterAsync(CancellationToken cancellationToken)
    {
        var account = this.Account;
        var passsword = this.Passsword;
        if (account.IsNullOrEmpty())
        {
            this.StatusMessage = "请输入账号";
            return;
        }
        if (passsword.IsNullOrEmpty())
        {
            this.StatusMessage = "请输入密码";
            return;
        }
        this.StatusMessage = "正在注册...";
        var result = await this.m_remoteService.RegisterAsync(account, passsword, cancellationToken);
        this.StatusMessage = result.IsSuccess ? "注册成功" : $"注册失败: {result.Message}";
    }

    #endregion 认证 Command

    #region 用户管理 Command

    [RelayCommand]
    private async Task GetUserListAsync(CancellationToken cancellationToken)
    {
        this.UserResult = "正在获取用户列表...";
        var result = await this.m_remoteService.GetUserListAsync(cancellationToken);
        this.UserResult = result.IsSuccess
            ? JsonSerializer.Serialize(result.Value, s_jsonOptions)
            : $"失败: {result.Message}";
    }

    [RelayCommand]
    private async Task GetUserInfoAsync(CancellationToken cancellationToken)
    {
        this.UserResult = "正在获取用户信息...";
        var result = await this.m_remoteService.GetUserInfoAsync(cancellationToken);
        this.UserResult = result.IsSuccess
            ? JsonSerializer.Serialize(result.Value, s_jsonOptions)
            : $"失败: {result.Message}";
    }

    [RelayCommand]
    private async Task ChangePasswordAsync(CancellationToken cancellationToken)
    {
        if (this.OldPassword.IsNullOrEmpty() || this.NewPassword.IsNullOrEmpty())
        {
            this.UserResult = "请输入旧密码和新密码";
            return;
        }
        this.UserResult = "正在修改密码...";
        var result = await this.m_remoteService.ChangePasswordAsync(this.OldPassword!, this.NewPassword!, cancellationToken);
        this.UserResult = result.IsSuccess ? "修改密码成功" : $"失败: {result.Message}";
    }

    [RelayCommand]
    private async Task DeleteUserAsync(CancellationToken cancellationToken)
    {
        if (!long.TryParse(this.DeleteUserId, out var userId))
        {
            this.UserResult = "请输入有效的用户ID";
            return;
        }
        this.UserResult = "正在删除用户...";
        var result = await this.m_remoteService.DeleteUserAsync(userId, cancellationToken);
        this.UserResult = result.IsSuccess ? "删除用户成功" : $"失败: {result.Message}";
    }

    #endregion 用户管理 Command

    #region 角色管理 Command

    [RelayCommand]
    private async Task GetRoleListAsync(CancellationToken cancellationToken)
    {
        this.RoleResult = "正在获取角色列表...";
        var result = await this.m_remoteService.GetRoleListAsync(cancellationToken);
        this.RoleResult = result.IsSuccess
            ? JsonSerializer.Serialize(result.Value, s_jsonOptions)
            : $"失败: {result.Message}";
    }

    [RelayCommand]
    private async Task CreateRoleAsync(CancellationToken cancellationToken)
    {
        this.RoleResult = "正在创建角色...";
        var result = await this.m_remoteService.CreateRoleAsync(this.NewRoleName, this.NewRoleCode, this.NewRoleDescription, cancellationToken);
        this.RoleResult = result.IsSuccess
            ? $"创建成功:\n{JsonSerializer.Serialize(result.Value, s_jsonOptions)}"
            : $"失败: {result.Message}";
    }

    [RelayCommand]
    private async Task UpdateRoleAsync(CancellationToken cancellationToken)
    {
        if (!long.TryParse(this.UpdateRoleId, out var roleId))
        {
            this.RoleResult = "请输入有效的角色ID";
            return;
        }
        this.RoleResult = "正在更新角色...";
        var result = await this.m_remoteService.UpdateRoleAsync(roleId, this.UpdateRoleName, this.UpdateRoleDescription, this.UpdateRoleIsEnabled, cancellationToken);
        this.RoleResult = result.IsSuccess ? "更新角色成功" : $"失败: {result.Message}";
    }

    [RelayCommand]
    private async Task DeleteRoleAsync(CancellationToken cancellationToken)
    {
        if (!long.TryParse(this.DeleteRoleId, out var roleId))
        {
            this.RoleResult = "请输入有效的角色ID";
            return;
        }
        this.RoleResult = "正在删除角色...";
        var result = await this.m_remoteService.DeleteRoleAsync(roleId, cancellationToken);
        this.RoleResult = result.IsSuccess ? "删除角色成功" : $"失败: {result.Message}";
    }

    [RelayCommand]
    private async Task AssignUserRolesAsync(CancellationToken cancellationToken)
    {
        if (!long.TryParse(this.AssignUserId, out var userId))
        {
            this.RoleResult = "请输入有效的用户ID";
            return;
        }
        var roleIds = (this.AssignRoleIds ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => long.TryParse(s, out var id) ? (long?)id : null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToArray();
        this.RoleResult = "正在分配用户角色...";
        var result = await this.m_remoteService.AssignUserRolesAsync(userId, roleIds, cancellationToken);
        this.RoleResult = result.IsSuccess ? "分配用户角色成功" : $"失败: {result.Message}";
    }

    #endregion 角色管理 Command

    #region RPC授权 Command

    [RelayCommand]
    private async Task GetAllRpcRolesAsync(CancellationToken cancellationToken)
    {
        this.RpcResult = "正在获取所有RPC角色配置...";
        var result = await this.m_remoteService.GetAllRpcRolesAsync(cancellationToken);
        this.RpcResult = result.IsSuccess
            ? JsonSerializer.Serialize(result.Value, s_jsonOptions)
            : $"失败: {result.Message}";
    }

    [RelayCommand]
    private async Task GetAllRpcMethodsAsync(CancellationToken cancellationToken)
    {
        this.RpcResult = "正在获取所有RPC方法...";
        var result = await this.m_remoteService.GetAllRpcMethodsAsync(cancellationToken);
        this.RpcResult = result.IsSuccess
            ? JsonSerializer.Serialize(result.Value, s_jsonOptions)
            : $"失败: {result.Message}";
    }

    [RelayCommand]
    private async Task GetRpcRolesAsync(CancellationToken cancellationToken)
    {
        if (this.GetRpcTargetKey.IsNullOrEmpty())
        {
            this.RpcResult = "请输入目标键";
            return;
        }
        this.RpcResult = "正在获取RPC角色配置...";
        var result = await this.m_remoteService.GetRpcRolesAsync(this.GetRpcTargetKey!, cancellationToken);
        this.RpcResult = result.IsSuccess
            ? JsonSerializer.Serialize(result.Value, s_jsonOptions)
            : $"失败: {result.Message}";
    }

    [RelayCommand]
    private async Task SetRpcRolesAsync(CancellationToken cancellationToken)
    {
        if (this.SetRpcTargetKey.IsNullOrEmpty())
        {
            this.RpcResult = "请输入目标键";
            return;
        }
        var codes = (this.SetRpcRoleCodes ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        this.RpcResult = "正在设置RPC角色配置...";
        var result = await this.m_remoteService.SetRpcRolesAsync(this.SetRpcTargetKey!, codes, this.SetRpcDescription, cancellationToken);
        this.RpcResult = result.IsSuccess ? "设置RPC角色配置成功" : $"失败: {result.Message}";
    }

    [RelayCommand]
    private async Task RemoveRpcRolesAsync(CancellationToken cancellationToken)
    {
        if (this.RemoveRpcTargetKey.IsNullOrEmpty())
        {
            this.RpcResult = "请输入目标键";
            return;
        }
        this.RpcResult = "正在移除RPC角色配置...";
        var result = await this.m_remoteService.RemoveRpcRolesAsync(this.RemoveRpcTargetKey!, cancellationToken);
        this.RpcResult = result.IsSuccess ? "移除RPC角色配置成功" : $"失败: {result.Message}";
    }

    #endregion RPC授权 Command

    #region 日志管理 Command

    [RelayCommand]
    private async Task GetLogListAsync(CancellationToken cancellationToken)
    {
        this.LogResult = "正在获取日志列表...";
        var request = new GetLogListRequest
        {
            Level = this.LogLevel.IsNullOrEmpty() ? null : this.LogLevel,
            Category = this.LogCategory.IsNullOrEmpty() ? null : this.LogCategory,
            Keyword = this.LogKeyword.IsNullOrEmpty() ? null : this.LogKeyword,
            Take = this.LogTake,
        };
        var result = await this.m_remoteService.GetLogListAsync(request, cancellationToken);
        this.LogResult = result.IsSuccess
            ? $"总条数: {result.Value?.Total}\n{JsonSerializer.Serialize(result.Value?.Logs, s_jsonOptions)}"
            : $"失败: {result.Message}";
    }

    #endregion 日志管理 Command
}