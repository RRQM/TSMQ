using System.Text.Json;
using TORM;
using TSMQServer.Core;
using TSMQServer.Core.Json;
using TSMQServer.Db.Sqlite.Entities;

namespace TSMQServer.Db.Sqlite;

/// <summary>SQLite MQTT 访问策略实现。</summary>
public sealed class MqttAccessPolicyFromDB(OrmClient db) : IMqttAccessPolicyFromDB
{
    /// <inheritdoc/>
    public async Task<MqttAccessPolicy> GetAsync()
    {
        var entity = await db.Queryable<MqttAccessPolicyEntity>().Where(x => x.Id == 1).FirstAsync();
        return entity == null ? new MqttAccessPolicy() :
            JsonSerializer.Deserialize(entity.PolicyJson, TSMQServerCoreJsonSerializerContext.Default.MqttAccessPolicy)
            ?? throw new InvalidDataException("MQTT 访问策略为空");
    }

    /// <inheritdoc/>
    public async Task SaveAsync(MqttAccessPolicy policy)
    {
        await db.Savable(new MqttAccessPolicyEntity
        {
            PolicyJson = JsonSerializer.Serialize(policy, TSMQServerCoreJsonSerializerContext.Default.MqttAccessPolicy)
        }).ExecuteAsync();
    }
}
