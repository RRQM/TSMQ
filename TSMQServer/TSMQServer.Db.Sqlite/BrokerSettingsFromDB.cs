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

using TSMQServer.Db;
using TSMQServer.Db.Sqlite.Entities;
using TSMQServer.Db.Sqlite.Repositories;

namespace TSMQServer.Db.Sqlite;

/// <summary>
/// Broker 全局参数数据库实现。
/// </summary>
public class BrokerSettingsFromDB : IBrokerSettingsFromDB
{
    private readonly BrokerSettingsRepository m_repository;

    /// <inheritdoc/>
    public BrokerSettingsFromDB(BrokerSettingsRepository repository)
    {
        this.m_repository = repository;
    }

    /// <inheritdoc/>
    public async Task<(int MessageCapacity, int MessageExpirySeconds, int SessionExpirySeconds)> GetAsync()
    {
        var entity = await this.m_repository.GetSingleAsync().ConfigureAwait(false);
        if (entity is null)
        {
            // 返回与 MqttBrokerOption 默认值一致的初始值
            return (1000, 0, 0);
        }
        return (entity.MessageCapacity, entity.MessageExpirySeconds, entity.SessionExpirySeconds);
    }

    /// <inheritdoc/>
    public async Task SaveAsync(int messageCapacity, int messageExpirySeconds, int sessionExpirySeconds)
    {
        var entity = new BrokerSettingsEntity
        {
            Id = 1,
            MessageCapacity = messageCapacity,
            MessageExpirySeconds = messageExpirySeconds,
            SessionExpirySeconds = sessionExpirySeconds
        };
        await this.m_repository.SaveAsync(entity).ConfigureAwait(false);
    }
}
