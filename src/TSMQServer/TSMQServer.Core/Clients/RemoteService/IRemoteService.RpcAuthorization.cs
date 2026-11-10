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

namespace TSMQServer.Core.Clients;

public partial interface IRemoteService
{
    /// <summary>
    /// 获取所有RPC目标的角色授权配置列表
    /// </summary>
    Task<Result<List<RpcMethodRoleInfo>>> GetAllRpcRolesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取指定RPC目标的角色授权配置
    /// </summary>
    Task<Result<string[]>> GetRpcRolesAsync(string targetKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置指定RPC目标的所需角色
    /// </summary>
    Task<Result> SetRpcRolesAsync(string targetKey, string[] roleCodes, string? description = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 移除指定RPC目标的角色授权配置
    /// </summary>
    Task<Result> RemoveRpcRolesAsync(string targetKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有已注册的RPC方法列表
    /// </summary>
    Task<Result<List<RpcMethodInfo>>> GetAllRpcMethodsAsync(CancellationToken cancellationToken = default);
}
