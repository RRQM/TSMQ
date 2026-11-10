using TORM;

namespace TSMQServer.Db.Sqlite.Entities;

/// <summary>MQTT 访问策略配置。</summary>
[OrmTable("MqttAccessPolicy")]
public sealed class MqttAccessPolicyEntity
{
    /// <summary>固定主键。</summary>
    [OrmColumn(IsPrimaryKey = true)]
    public int Id { get; set; } = 1;
    /// <summary>策略 JSON。</summary>
    public string PolicyJson { get; set; } = string.Empty;
}
