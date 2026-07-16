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
/// SSL 证书数据库实现（Sqlite）。
/// </summary>
public class MqttSslCertFromDB : IMqttSslCertFromDB
{
    private readonly MqttSslCertRepository m_repository;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public MqttSslCertFromDB(MqttSslCertRepository repository)
    {
        this.m_repository = repository;
    }

    /// <inheritdoc/>
    public async Task<List<MqttSslCertInfo>> GetAllAsync()
    {
        var entities = await this.m_repository.GetAllAsync();
        return entities.Select(ToInfo).ToList();
    }

    /// <inheritdoc/>
    public async Task<MqttSslCertData?> GetCertDataAsync(string id)
    {
        var entity = await this.m_repository.GetByIdAsync(id);
        if (entity == null)
        {
            return null;
        }
        return new MqttSslCertData
        {
            Type = (MqttSslCertType)entity.Type,
            CertData = entity.CertData,
            KeyData = entity.KeyData,
            Password = entity.Password
        };
    }

    /// <inheritdoc/>
    public async Task<string?> GetNameAsync(string id)
    {
        var entity = await this.m_repository.GetByIdAsync(id);
        return entity?.Name;
    }

    /// <inheritdoc/>
    public async Task<bool> AddAsync(MqttSslCertInfo info, MqttSslCertData data)
    {
        var entity = new MqttSslCertEntity
        {
            Id = info.Id,
            Name = info.Name,
            Type = (int)info.Type,
            CertData = data.CertData,
            KeyData = data.KeyData,
            Password = data.Password,
            Subject = info.Subject,
            Thumbprint = info.Thumbprint,
            NotBefore = info.NotBefore,
            NotAfter = info.NotAfter,
            CreatedAt = info.CreatedAt
        };
        return await this.m_repository.InsertAsync(entity);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(string id)
    {
        return await this.m_repository.DeleteAsync(id);
    }

    private static MqttSslCertInfo ToInfo(MqttSslCertEntity entity)
    {
        return new MqttSslCertInfo
        {
            Id = entity.Id,
            Name = entity.Name,
            Type = (MqttSslCertType)entity.Type,
            Subject = entity.Subject,
            Thumbprint = entity.Thumbprint,
            NotBefore = entity.NotBefore,
            NotAfter = entity.NotAfter,
            CreatedAt = entity.CreatedAt
        };
    }
}
