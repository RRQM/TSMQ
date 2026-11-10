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
using TouchSocket.Rpc;

namespace TSMQServer.Core.Clients;

public partial interface IRemoteService
{
    IRpcClient? Client { get; }

    /// <summary>登录后的 Token，登录成功后自动设置</summary>
    string? LoginedToken { get; }

    /// <summary>登录</summary>
    Task<Result> LoginAsync(string account, string password, CancellationToken cancellationToken = default);

    /// <summary>注册账号</summary>
    Task<Result> RegisterAsync(string account, string password, CancellationToken cancellationToken = default);

    /// <summary>获取用户列表</summary>
    Task<Result<List<UserInfo>>> GetUserListAsync(CancellationToken cancellationToken = default);

    /// <summary>获取当前登录用户信息</summary>
    Task<Result<UserInfo>> GetUserInfoAsync(CancellationToken cancellationToken = default);

    /// <summary>修改密码</summary>
    Task<Result> ChangePasswordAsync(string oldPassword, string newPassword, CancellationToken cancellationToken = default);

    /// <summary>删除用户</summary>
    Task<Result> DeleteUserAsync(long userId, CancellationToken cancellationToken = default);
}