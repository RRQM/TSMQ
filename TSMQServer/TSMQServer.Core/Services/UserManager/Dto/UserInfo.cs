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
/// 用户信息
/// </summary>
public class UserInfo
{
    /// <summary>
    /// 账号
    /// </summary>
    public string Account { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 最后登录时间
    /// </summary>
    public DateTime? LastLoginTime { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 手机号
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// 角色ID列表(逗号分隔)
    /// </summary>
    public string? RoleIds { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string? UserName { get; set; }
}
