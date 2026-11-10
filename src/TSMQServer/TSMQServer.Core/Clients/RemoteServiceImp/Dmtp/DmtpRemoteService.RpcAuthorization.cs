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
using TouchSocket.Rpc.DmtpRpc.Generators;

namespace TSMQServer.Core.Clients;

public partial class DmtpRemoteService : IRemoteService
{
    public async Task<Result<List<RpcMethodRoleInfo>>> GetAllRpcRolesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var actor = this.m_actorFactory();
            var response = await actor.GetAllRpcRolesAsync(new LoginedRequest
            {
                LoginedToken = this.m_loginedToken
            }, this.CreateInvokeOption(cancellationToken));
            return response.Code == ResponseCode.Success
                ? new Result<List<RpcMethodRoleInfo>>(response.Items)
                : Result.FromFail(response.Message);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    public async Task<Result<string[]>> GetRpcRolesAsync(string targetKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var actor = this.m_actorFactory();
            var response = await actor.GetRpcRolesAsync(new GetRpcRolesRequest
            {
                LoginedToken = this.m_loginedToken,
                TargetKey = targetKey
            }, this.CreateInvokeOption(cancellationToken));
            return response.Code == ResponseCode.Success
                ? new Result<string[]>(response.RoleCodes)
                : Result.FromFail(response.Message);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    public async Task<Result> SetRpcRolesAsync(string targetKey, string[] roleCodes, string? description = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var actor = this.m_actorFactory();
            var response = await actor.SetRpcRolesAsync(new SetRpcRolesRequest
            {
                LoginedToken = this.m_loginedToken,
                TargetKey = targetKey,
                RoleCodes = roleCodes,
                Description = description
            }, this.CreateInvokeOption(cancellationToken));
            return response.Code == ResponseCode.Success ? Result.Success : Result.FromFail(response.Message);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    public async Task<Result> RemoveRpcRolesAsync(string targetKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var actor = this.m_actorFactory();
            var response = await actor.RemoveRpcRolesAsync(new RemoveRpcRolesRequest
            {
                LoginedToken = this.m_loginedToken,
                TargetKey = targetKey
            }, this.CreateInvokeOption(cancellationToken));
            return response.Code == ResponseCode.Success ? Result.Success : Result.FromFail(response.Message);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    public async Task<Result<List<RpcMethodInfo>>> GetAllRpcMethodsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var actor = this.m_actorFactory();
            var response = await actor.GetAllRpcMethodsAsync(new LoginedRequest
            {
                LoginedToken = this.m_loginedToken
            }, this.CreateInvokeOption(cancellationToken));
            return response.Code == ResponseCode.Success
                ? new Result<List<RpcMethodInfo>>(response.Methods)
                : Result.FromFail(response.Message);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }
}
