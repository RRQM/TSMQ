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

namespace TSMQServer.Core;

/// <summary>
/// RPC方法角色授权配置信息
/// </summary>
public class RpcMethodRoleInfo
{
    /// <summary>
    /// 目标键，格式为 <c>ClassName.MethodName</c>（方法级）或 <c>ClassName</c>（类级）
    /// </summary>
    public string TargetKey { get; set; } = string.Empty;

    /// <summary>
    /// 所需角色编码数组
    /// </summary>
    public string[] RoleCodes { get; set; } = [];

    /// <summary>
    /// 备注描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdateTime { get; set; }
}
