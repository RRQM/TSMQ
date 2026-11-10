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

using System.ComponentModel;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.JsonRpc;
using TouchSocket.Rpc;
using TouchSocket.WebApi;

namespace TSMQServer.Core;

/// <summary>
/// RPC动态角色授权配置服务接口
/// </summary>
[GeneratorRpcProxy]
public interface IRpcAuthorizationService : ITransientRpcServer
{
    /// <summary>
    /// 获取所有已注册的RPC方法列表
    /// </summary>
    [Description("获取所有已注册的RPC方法列表")]
    [WebApi(Method = HttpMethodType.Post)]
    [DmtpRpc]
    [JsonRpc(MethodInvoke = true)]
    Task<RpcMethodListResponse> GetAllRpcMethods(LoginedRequest request);

    /// <summary>
    /// 获取所有RPC目标的角色授权配置列表
    /// </summary>
    [Description("获取所有RPC角色授权配置")]
    [WebApi(Method = HttpMethodType.Post)]
    [DmtpRpc]
    [JsonRpc(MethodInvoke = true)]
    Task<RpcRoleListResponse> GetAllRpcRoles(LoginedRequest request);

    /// <summary>
    /// 获取指定RPC目标的角色授权配置
    /// </summary>
    [Description("获取指定RPC目标的角色授权配置")]
    [WebApi(Method = HttpMethodType.Post)]
    [DmtpRpc]
    [JsonRpc(MethodInvoke = true)]
    Task<RpcRoleResponse> GetRpcRoles(GetRpcRolesRequest request);

    /// <summary>
    /// 移除指定RPC目标的角色授权配置
    /// </summary>
    [Description("移除指定RPC目标的角色授权配置")]
    [WebApi(Method = HttpMethodType.Post)]
    [DmtpRpc]
    [JsonRpc(MethodInvoke = true)]
    Task<ResponseBase> RemoveRpcRoles(RemoveRpcRolesRequest request);

    /// <summary>
    /// 设置指定RPC目标（方法或类）的所需角色，<see langword="null"/>或空数组表示移除限制
    /// </summary>
    [Description("设置指定RPC目标的角色授权")]
    [WebApi(Method = HttpMethodType.Post)]
    [DmtpRpc]
    [JsonRpc(MethodInvoke = true)]
    Task<ResponseBase> SetRpcRoles(SetRpcRolesRequest request);
}