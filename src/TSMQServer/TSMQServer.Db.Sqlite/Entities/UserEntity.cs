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

[OrmTable("Users")]
public sealed class UserEntity
{
    [OrmColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    public string Account { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string NickName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Avatar { get; set; } = string.Empty;

    public string RoleIds { get; set; } = string.Empty;

    public int Status { get; set; } = 1;

    public DateTime CreateTime { get; set; } = DateTime.Now;

    public DateTime? UpdateTime { get; set; }
}
