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

using TSMQServer.Core;

namespace TSMQServer.Db;

/// <summary>
/// 角色管理数据库接口
/// </summary>
public interface IRoleManagerFromDB
{
    /// <summary>
    /// 创建角色
    /// </summary>
    Task<RoleInfo> CreateRoleAsync(RoleInfo role);

    /// <summary>
    /// 更新角色
    /// </summary>
    Task<bool> UpdateRoleAsync(RoleInfo role);

    /// <summary>
    /// 删除角色
    /// </summary>
    Task<bool> DeleteRoleAsync(long roleId);

    /// <summary>
    /// 根据ID获取角色
    /// </summary>
    Task<RoleInfo?> GetRoleByIdAsync(long roleId);

    /// <summary>
    /// 根据编码获取角色
    /// </summary>
    Task<RoleInfo?> GetRoleByCodeAsync(string roleCode);

    /// <summary>
    /// 获取所有角色列表
    /// </summary>
    Task<List<RoleInfo>> GetAllRolesAsync();

    /// <summary>
    /// 根据多个ID批量获取角色
    /// </summary>
    Task<List<RoleInfo>> GetRolesByIdsAsync(IEnumerable<long> roleIds);
}
