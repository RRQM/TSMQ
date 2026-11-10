using TSMQServer.Core;

namespace TSMQServer.Db;

/// <summary>主题消息持久化存储。</summary>
public interface IMqttMessageStore
{
    /// <summary>读取持久化策略。</summary>
    Task<MqttPersistencePolicy> GetPolicyAsync();
    /// <summary>保存持久化策略。</summary>
    Task SavePolicyAsync(MqttPersistencePolicy policy);
    /// <summary>保存完整消息。</summary>
    Task AppendAsync(PersistedMqttMessage message);
    /// <summary>按条件查询消息。</summary>
    Task<QueryMqttMessagesResponse> QueryAsync(QueryMqttMessagesRequest request);
    /// <summary>清理指定 UTC 时间之前的消息。</summary>
    Task DeleteBeforeAsync(DateTime cutoff);
}
