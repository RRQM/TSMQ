namespace TSMQServer.Core;

/// <summary>主题消息持久化策略。</summary>
public sealed class MqttPersistencePolicy
{
    /// <summary>是否保存新消息。</summary>
    public bool Enabled { get; set; }
    /// <summary>主题过滤器，支持 MQTT 通配符。</summary>
    public List<string> TopicFilters { get; set; } = [];
    /// <summary>历史消息保留天数，范围为 1 至 3650。</summary>
    public int RetentionDays { get; set; } = 30;
}

/// <summary>持久化策略响应。</summary>
public sealed class MqttPersistencePolicyResponse : ResponseBase
{
    /// <summary>当前策略。</summary>
    public MqttPersistencePolicy Policy { get; set; } = new();
}

/// <summary>保存持久化策略请求。</summary>
public sealed class SaveMqttPersistencePolicyRequest : LoginedRequest
{
    /// <summary>新策略。</summary>
    public MqttPersistencePolicy Policy { get; set; } = new();
}

/// <summary>历史消息查询。</summary>
public sealed class QueryMqttMessagesRequest : LoginedRequest
{
    /// <summary>主题过滤器，空值表示全部。</summary>
    public string TopicFilter { get; set; } = string.Empty;
    /// <summary>客户端 ID，空值表示全部。</summary>
    public string ClientId { get; set; } = string.Empty;
    /// <summary>包含的起始 UTC 时间。</summary>
    public DateTime? StartTime { get; set; }
    /// <summary>包含的结束 UTC 时间。</summary>
    public DateTime? EndTime { get; set; }
    /// <summary>仅返回此序号之前的消息，零表示最新。</summary>
    public long BeforeId { get; set; }
    /// <summary>每页条数，范围为 1 至 200。</summary>
    public int Limit { get; set; } = 50;
}

/// <summary>持久化的完整 MQTT 消息。</summary>
public sealed class PersistedMqttMessage
{
    /// <summary>消息序号。</summary>
    public long Id { get; set; }
    /// <summary>接收 UTC 时间。</summary>
    public DateTime Timestamp { get; set; }
    /// <summary>发布客户端 ID。</summary>
    public string ClientId { get; set; } = string.Empty;
    /// <summary>发布用户名。</summary>
    public string UserName { get; set; } = string.Empty;
    /// <summary>实际主题。</summary>
    public string Topic { get; set; } = string.Empty;
    /// <summary>QoS 等级。</summary>
    public int Qos { get; set; }
    /// <summary>Retain 标志。</summary>
    public bool Retain { get; set; }
    /// <summary>完整原始负载的 Base64 表示。</summary>
    public string PayloadBase64 { get; set; } = string.Empty;
    /// <summary>原始负载字节数。</summary>
    public int PayloadLength { get; set; }
}

/// <summary>历史消息查询响应。</summary>
public sealed class QueryMqttMessagesResponse : ResponseBase
{
    /// <summary>按序号倒序排列的消息。</summary>
    public List<PersistedMqttMessage> Messages { get; set; } = [];
    /// <summary>是否还有下一页。</summary>
    public bool HasMore { get; set; }
}
