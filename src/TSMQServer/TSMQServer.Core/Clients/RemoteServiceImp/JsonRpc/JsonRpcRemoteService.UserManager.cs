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

using TouchSocket.Core;
using TouchSocket.Rpc.JsonRpc.Generators;
namespace TSMQServer.Core.Clients;

public partial class JsonRpcRemoteService : IRemoteService
{
    public async Task<Result> LoginAsync(string account, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await this.m_jsonRpcClient.LoginAsync(new LoginRequest
            {
                Account = account,
                Password = password
            }, CreateInvokeOption(cancellationToken));
            if (response.Code == ResponseCode.Success)
            {
                this.m_loginedToken = response.LoginedToken;
                return Result.Success;
            }

            return Result.FromFail(response.Message);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    public async Task<Result> RegisterAsync(string account, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await this.m_jsonRpcClient.RegisterAsync(new RegisterRequest
            {
                Account = account,
                Password = password,
                LoginedToken = this.m_loginedToken
            }, CreateInvokeOption(cancellationToken));
            return response.Code == ResponseCode.Success ? Result.Success : Result.FromFail(response.Message);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    public async Task<Result<List<UserInfo>>> GetUserListAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await this.m_jsonRpcClient.GetUserListAsync(new LoginedRequest
            {
                LoginedToken = this.m_loginedToken
            }, CreateInvokeOption(cancellationToken));
            return response.Code == ResponseCode.Success
                ? new Result<List<UserInfo>>(response.Users)
                : Result.FromFail(response.Message);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    public async Task<Result<UserInfo>> GetUserInfoAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await this.m_jsonRpcClient.GetUserInfoAsync(new LoginedRequest
            {
                LoginedToken = this.m_loginedToken
            }, CreateInvokeOption(cancellationToken));
            return response.Code == ResponseCode.Success
                ? new Result<UserInfo>(response.User!)
                : Result.FromFail(response.Message);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    public async Task<Result> ChangePasswordAsync(string oldPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await this.m_jsonRpcClient.ChangePasswordAsync(new ChangePasswordRequest
            {
                LoginedToken = this.m_loginedToken,
                OldPassword = oldPassword,
                NewPassword = newPassword
            }, CreateInvokeOption(cancellationToken));
            return response.Code == ResponseCode.Success ? Result.Success : Result.FromFail(response.Message);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    public async Task<Result> DeleteUserAsync(long userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await this.m_jsonRpcClient.DeleteUserAsync(new DeleteUserRequest
            {
                LoginedToken = this.m_loginedToken,
                UserId = userId
            }, CreateInvokeOption(cancellationToken));
            return response.Code == ResponseCode.Success ? Result.Success : Result.FromFail(response.Message);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }
}
