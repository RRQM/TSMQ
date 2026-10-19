using TSMQServer.Core;

namespace TSMQServer.Db;

/// <summary>MQTT 访问策略持久化。</summary>
public interface IMqttAccessPolicyFromDB
{
    /// <summary>读取策略。</summary>
    Task<MqttAccessPolicy> GetAsync();
    /// <summary>保存策略。</summary>
    Task SaveAsync(MqttAccessPolicy policy);
}
