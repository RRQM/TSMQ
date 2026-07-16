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
using TSMQServer.Db;
using TSMQServer.Db.Sqlite.Entities;
using TSMQServer.Db.Sqlite.Repositories;

namespace TSMQServer.Db.Sqlite;

/// <summary>
/// MQTT 连接授权数据库实现（Sqlite）。
/// </summary>
public class MqttConnectAuthFromDB : IMqttConnectAuthFromDB
{
    private readonly MqttAuthUserRepository m_repository;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public MqttConnectAuthFromDB(MqttAuthUserRepository repository)
    {
        this.m_repository = repository;
    }

    /// <inheritdoc/>
    public async Task<List<MqttAuthUserInfo>> GetAllAsync()
    {
        var entities = await this.m_repository.GetAllAsync();
        return entities.Select(ToInfo).ToList();
    }

    /// <inheritdoc/>
    public async Task<bool> AddAsync(MqttAuthUserInfo info)
    {
        var existing = await this.m_repository.GetByUserNameAsync(info.UserName);
        if (existing != null)
        {
            return false;
        }
        return await this.m_repository.InsertAsync(ToEntity(info));
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateAsync(MqttAuthUserInfo info)
    {
        var existing = await this.m_repository.GetByUserNameAsync(info.UserName);
        if (existing == null)
        {
            return false;
        }
        existing.PasswordHash = info.Password;
        existing.Remark = info.Remark ?? string.Empty;
        return await this.m_repository.UpdateAsync(existing);
    }

    /// <inheritdoc/>
    public Task<bool> DeleteAsync(string userName)
    {
        return this.m_repository.DeleteAsync(userName);
    }

    /// <inheritdoc/>
    public async Task<bool> ValidateAsync(string userName, string password)
    {
        var entity = await this.m_repository.GetByUserNameAsync(userName);
        if (entity == null)
        {
            return false;
        }
        return entity.PasswordHash == password;
    }

    private static MqttAuthUserInfo ToInfo(MqttAuthUserEntity entity)
    {
        return new MqttAuthUserInfo
        {
            UserName = entity.UserName,
            Password = entity.PasswordHash,
            Remark = entity.Remark,
            CreatedAt = entity.CreatedAt
        };
    }

    private static MqttAuthUserEntity ToEntity(MqttAuthUserInfo info)
    {
        return new MqttAuthUserEntity
        {
            UserName = info.UserName,
            PasswordHash = info.Password,
            Remark = info.Remark ?? string.Empty,
            CreatedAt = info.CreatedAt == default ? DateTime.UtcNow : info.CreatedAt
        };
    }
}
