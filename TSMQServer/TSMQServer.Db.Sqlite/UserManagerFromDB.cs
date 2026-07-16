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
using TSMQServer.Db.Sqlite.Entities;
using TSMQServer.Db.Sqlite.Repositories;

namespace TSMQServer.Db.Sqlite;

public class UserManagerFromDB: IUserManagerFromDB
{
    private readonly LoginSessionRepository m_loginSessionRepository;
    private readonly UserRepository m_userRepository;

    public UserManagerFromDB(
        UserRepository userRepository,
        LoginSessionRepository loginSessionRepository)
    {
        this.m_userRepository = userRepository;
        this.m_loginSessionRepository = loginSessionRepository;
    }

    public async Task<bool> AssignUserRolesAsync(long userId, string roleIds)
    {
        return await this.m_userRepository.UpdateRoleIdsAsync(userId, roleIds);
    }

    public async Task<bool> ChangePasswordAsync(long userId, string newPassword)
    {
        var entity = await this.m_userRepository.GetByIdAsync(userId);
        if (entity == null)
        {
            return false;
        }
        entity.Password = newPassword;
        entity.UpdateTime = DateTime.Now;
        return await this.m_userRepository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteUserAsync(long userId)
    {
        var entity = await this.m_userRepository.GetByIdAsync(userId);
        if (entity == null)
        {
            return false;
        }
        return await this.m_userRepository.DeleteAsync(entity.Account);
    }

    public async Task<UserInfo?> GetUserByIdAsync(long userId)
    {
        var userEntity = await this.m_userRepository.GetByIdAsync(userId);
        if (userEntity == null)
        {
            return null;
        }

        return new UserInfo
        {
            Id = userEntity.Id,
            Account = userEntity.Account,
            Password = userEntity.Password,
            RoleIds = userEntity.RoleIds
        };
    }

    public async Task<IEnumerable<UserInfo>> GetUserListAsync()
    {
        var entities = await this.m_userRepository.GetAllAsync();
        return entities.Select(e => new UserInfo
        {
            Id = e.Id,
            Account = e.Account,
            UserName = e.NickName,
            Email = e.Email,
            Phone = e.Phone,
            RoleIds = e.RoleIds,
            IsEnabled = e.Status == 1,
            CreateTime = e.CreateTime
        });
    }

    public async Task LoginAsync(string loginedToken, UserInfo userInfo)
    {
        if (await this.m_loginSessionRepository.TokenExistsAsync(loginedToken))
        {
            throw new Exception("账号已登录");
        }

        var sessionEntity = new LoginSessionEntity
        {
            LoginedToken = loginedToken,
            Account = userInfo.Account
        };

        await this.m_loginSessionRepository.InsertAsync(sessionEntity);
    }

    public async Task RegisterAsync(UserInfo userInfo)
    {
        if (await this.m_userRepository.AccountExistsAsync(userInfo.Account))
        {
            throw new Exception("账号已被注册");
        }

        var userEntity = new UserEntity
        {
            Account = userInfo.Account,
            Password = userInfo.Password,
            CreateTime = DateTime.Now
        };

        await this.m_userRepository.InsertAsync(userEntity);
    }

    public async Task<(bool success, UserInfo? userInfo)> TryGetLoginedInfoAsync(string loginedToken)
    {
        var sessionEntity = await this.m_loginSessionRepository.GetByTokenAsync(loginedToken);

        if (sessionEntity != null)
        {
            var userEntity = await this.m_userRepository.GetByAccountAsync(sessionEntity.Account);

            if (userEntity != null)
            {
                var userInfo = new UserInfo
                {
                    Id = userEntity.Id,
                    Account = userEntity.Account,
                    Password = userEntity.Password,
                    RoleIds = userEntity.RoleIds
                };
                return (true, userInfo);
            }
        }

        return (false, null);
    }

    public async Task<(bool success, UserInfo? userInfo)> TryGetRegisteredInfoAsync(string account)
    {
        var userEntity = await this.m_userRepository.GetByAccountAsync(account);

        if (userEntity != null)
        {
            var userInfo = new UserInfo
            {
                Id = userEntity.Id,
                Account = userEntity.Account,
                Password = userEntity.Password,
                RoleIds = userEntity.RoleIds
            };
            return (true, userInfo);
        }

        return (false, null);
    }
}