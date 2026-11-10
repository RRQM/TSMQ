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
using TSMQServer.Core.Imp.Attributes;
using TSMQServer.Db;
using TouchSocket.Rpc;

namespace TSMQServer.Core.Imp.Services;

/// <summary>
/// RPC动态角色授权配置服务实现
/// </summary>
[ExceptionFilter]
[LoggerFilter]
[Authorize]
public partial class RpcAuthorizationService : TransientRpcServer, IRpcAuthorizationService
{
    private readonly ILogger<RpcAuthorizationService> m_logger;
    private readonly IRpcAuthorizationFromDB m_rpcAuthorizationFromDB;
    private readonly RpcMethod[] m_rpcMethods;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public RpcAuthorizationService(ILogger<RpcAuthorizationService> logger, IRpcAuthorizationFromDB rpcAuthorizationFromDB, RpcStore rpcStore)
    {
        this.m_rpcMethods = rpcStore.GetAllMethods();
        this.m_logger = logger;
        this.m_rpcAuthorizationFromDB = rpcAuthorizationFromDB;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<RpcRoleListResponse> GetAllRpcRoles(LoginedRequest request)
    {
        LogGettingAllRpcRoles(this.m_logger);

        var items = await this.m_rpcAuthorizationFromDB.GetAllAsync();

        var response = new RpcRoleListResponse { Items = items };

        LogGetAllRpcRolesSuccess(this.m_logger, items.Count);
        return response.FromSuccess("获取RPC授权配置列表成功");
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<RpcRoleResponse> GetRpcRoles(GetRpcRolesRequest request)
    {
        LogGettingRpcRoles(this.m_logger, request.TargetKey);

        if (string.IsNullOrWhiteSpace(request.TargetKey))
        {
            return new RpcRoleResponse().FromFail("目标键不能为空");
        }

        var roles = await this.m_rpcAuthorizationFromDB.GetRolesForTargetAsync(request.TargetKey);

        var response = new RpcRoleResponse
        {
            TargetKey = request.TargetKey,
            RoleCodes = roles
        };

        LogGetRpcRolesSuccess(this.m_logger, request.TargetKey, roles.Length);
        return response.FromSuccess("获取RPC目标角色成功");
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<ResponseBase> SetRpcRoles(SetRpcRolesRequest request)
    {
        LogSettingRpcRoles(this.m_logger, request.TargetKey, request.RoleCodes.Length);

        if (string.IsNullOrWhiteSpace(request.TargetKey))
        {
            return new ResponseBase().FromFail("目标键不能为空");
        }

        await this.m_rpcAuthorizationFromDB.SetRolesForTargetAsync(request.TargetKey, request.RoleCodes, request.Description);

        LogSetRpcRolesSuccess(this.m_logger, request.TargetKey);
        return new ResponseBase().FromSuccess("设置RPC角色授权成功");
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<ResponseBase> RemoveRpcRoles(RemoveRpcRolesRequest request)
    {
        LogRemovingRpcRoles(this.m_logger, request.TargetKey);

        if (string.IsNullOrWhiteSpace(request.TargetKey))
        {
            return new ResponseBase().FromFail("目标键不能为空");
        }

        var result = await this.m_rpcAuthorizationFromDB.RemoveRolesForTargetAsync(request.TargetKey);
        if (!result)
        {
            LogRemoveRpcRolesNotFound(this.m_logger, request.TargetKey);
            return new ResponseBase().FromFail("未找到指定目标键的授权配置");
        }

        LogRemoveRpcRolesSuccess(this.m_logger, request.TargetKey);
        return new ResponseBase().FromSuccess("移除RPC角色授权成功");
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public Task<RpcMethodListResponse> GetAllRpcMethods(LoginedRequest request)
    {
        LogGettingAllRpcMethods(this.m_logger, this.m_rpcMethods.Length);

        var methods = this.m_rpcMethods.Select(m =>
        {
            var serviceName = m.ServerToType.Name;
            var methodName = m.Name;
            return new RpcMethodInfo
            {
                ServiceName = serviceName,
                MethodName = methodName,
                ClassTargetKey = serviceName,
                MethodTargetKey = $"{serviceName}.{methodName}",
                Description = m.GetDescription()
            };
        }).ToList();

        var response = new RpcMethodListResponse { Methods = methods };
        LogGetAllRpcMethodsSuccess(this.m_logger, methods.Count);
        return Task.FromResult(response.FromSuccess("获取RPC方法列表成功"));
    }
}
