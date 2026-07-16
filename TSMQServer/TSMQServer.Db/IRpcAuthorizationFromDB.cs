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
/// RPC授权数据库接口
/// </summary>
public interface IRpcAuthorizationFromDB
{
    /// <summary>
    /// 获取指定目标键所需角色编码列表，目标键可为方法键或类键
    /// </summary>
    Task<string[]> GetRolesForTargetAsync(string targetKey);

    /// <summary>
    /// 设置指定目标键所需角色编码列表
    /// </summary>
    Task SetRolesForTargetAsync(string targetKey, string[] roleCodes, string? description);

    /// <summary>
    /// 删除指定目标键的角色授权配置
    /// </summary>
    Task<bool> RemoveRolesForTargetAsync(string targetKey);

    /// <summary>
    /// 获取所有RPC授权配置
    /// </summary>
    Task<List<RpcMethodRoleInfo>> GetAllAsync();
}
