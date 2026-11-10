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
    /// 创建角色
    /// </summary>
    Task<Result<RoleInfo>> CreateRoleAsync(string? roleName, string? roleCode, string? description, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新角色
    /// </summary>
    Task<Result> UpdateRoleAsync(long roleId, string? roleName, string? description, bool? isEnabled, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除角色
    /// </summary>
    Task<Result> DeleteRoleAsync(long roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取角色列表
    /// </summary>
    Task<Result<List<RoleInfo>>> GetRoleListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 分配用户角色
    /// </summary>
    Task<Result> AssignUserRolesAsync(long userId, long[] roleIds, CancellationToken cancellationToken = default);
}
