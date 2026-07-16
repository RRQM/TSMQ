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
/// 角色管理服务接口
/// </summary>
[GeneratorRpcProxy]
public interface IRoleManagerService : ITransientRpcServer
{
    /// <summary>
    /// 创建角色
    /// </summary>
    [Description("创建角色")]
    [WebApi(Method = HttpMethodType.Post)]
    [DmtpRpc]
    [JsonRpc(MethodInvoke = true)]
    Task<RoleResponse> CreateRole(CreateRoleRequest request);

    /// <summary>
    /// 删除角色
    /// </summary>
    [Description("删除角色")]
    [WebApi(Method = HttpMethodType.Post)]
    [DmtpRpc]
    [JsonRpc(MethodInvoke = true)]
    Task<ResponseBase> DeleteRole(DeleteRoleRequest request);

    /// <summary>
    /// 获取角色列表
    /// </summary>
    [Description("获取角色列表")]
    [WebApi(Method = HttpMethodType.Post)]
    [DmtpRpc]
    [JsonRpc(MethodInvoke = true)]
    Task<RoleListResponse> GetRoleList(GetRoleListRequest request);

    /// <summary>
    /// 更新角色
    /// </summary>
    [Description("更新角色")]
    [WebApi(Method = HttpMethodType.Post)]
    [DmtpRpc]
    [JsonRpc(MethodInvoke = true)]
    Task<ResponseBase> UpdateRole(UpdateRoleRequest request);
}