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

public interface IUserManagerFromDB
{
    /// <summary>撤销指定登录会话。</summary>
    Task LogoutAsync(string loginToken);
    /// <summary>
    /// 获取账号的注册信息
    /// </summary>
    /// <param name="account"></param>
    /// <returns></returns>
    Task<(bool success, UserInfo? userInfo)> TryGetRegisteredInfoAsync(string account);

    /// <summary>
    /// 注册账号
    /// </summary>
    /// <param name="userInfo"></param>
    /// <returns></returns>
    Task RegisterAsync(UserInfo userInfo);

    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="loginedToken"></param>
    /// <param name="userInfo"></param>
    Task LoginAsync(string loginedToken, UserInfo userInfo);

    /// <summary>
    /// 获取登录账号的信息
    /// </summary>
    /// <param name="loginedToken"></param>
    /// <returns></returns>
    Task<(bool success, UserInfo? userInfo)> TryGetLoginedInfoAsync(string loginedToken);

    /// <summary>
    /// 根据用户ID获取用户信息
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<UserInfo?> GetUserByIdAsync(long userId);

    /// <summary>
    /// 为用户分配角色
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="roleIds">逗号分隔的角色ID列表</param>
    /// <returns></returns>
    Task<bool> AssignUserRolesAsync(long userId, string roleIds);

    /// <summary>
    /// 获取所有用户列表
    /// </summary>
    Task<IEnumerable<UserInfo>> GetUserListAsync();

    /// <summary>
    /// 修改用户密码
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="newPassword"></param>
    Task<bool> ChangePasswordAsync(long userId, string newPassword);

    /// <summary>
    /// 删除用户
    /// </summary>
    /// <param name="userId"></param>
    Task<bool> DeleteUserAsync(long userId);
}
