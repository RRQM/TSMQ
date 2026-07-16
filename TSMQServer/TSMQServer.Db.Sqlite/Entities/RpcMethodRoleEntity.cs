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

using TORM;

namespace TSMQServer.Db.Sqlite.Entities;

/// <summary>
/// RPC方法角色授权实体
/// </summary>
[OrmTable("RpcMethodRoles")]
public class RpcMethodRoleEntity
{
    /// <summary>
    /// 目标键，格式为 ClassName.MethodName 或 ClassName
    /// </summary>
    [OrmColumn(IsPrimaryKey = true)]
    public string TargetKey { get; set; } = string.Empty;

    /// <summary>
    /// 所需角色编码（逗号分隔）
    /// </summary>
    public string RoleCodes { get; set; } = string.Empty;

    /// <summary>
    /// 备注描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public string UpdateTime { get; set; } = string.Empty;
}
