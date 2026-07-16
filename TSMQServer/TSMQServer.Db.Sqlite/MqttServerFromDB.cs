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

/// <summary>
/// MQTT 服务器配置数据库实现（Sqlite）。
/// </summary>
public class MqttServerFromDB : IMqttServerFromDB
{
    private readonly MqttServerRepository m_repository;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public MqttServerFromDB(MqttServerRepository repository)
    {
        this.m_repository = repository;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<List<MqttServerInfo>> GetAllAsync()
    {
        var entities = await this.m_repository.GetAllAsync();
        return entities.Select(ToInfo).ToList();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task AddAsync(MqttServerInfo info)
    {
        await this.m_repository.InsertAsync(ToEntity(info));
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task UpdateAsync(MqttServerInfo info)
    {
        await this.m_repository.SaveAsync(ToEntity(info));
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<bool> DeleteAsync(string id)
    {
        return await this.m_repository.DeleteAsync(id);
    }

    private static MqttServerInfo ToInfo(MqttServerEntity entity)
    {
        return new MqttServerInfo
        {
            Id = entity.Id,
            Type = (MqttServerType)entity.Type,
            Host = entity.Host,
            Port = entity.Port,
            Path = entity.Path,
            IsRunning = entity.IsRunning == 1,
            CreatedAt = entity.CreatedAt,
            SslCertId = entity.SslCertId
        };
    }

    private static MqttServerEntity ToEntity(MqttServerInfo info)
    {
        return new MqttServerEntity
        {
            Id = info.Id,
            Type = (int)info.Type,
            Host = info.Host,
            Port = info.Port,
            Path = info.Path,
            IsRunning = info.IsRunning ? 1 : 0,
            CreatedAt = info.CreatedAt,
            SslCertId = info.SslCertId
        };
    }
}
