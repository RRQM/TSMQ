namespace TSMQServer.Core;

/// <summary>MQTT 访问策略。</summary>
public sealed class MqttAccessPolicy
{
    /// <summary>是否验证连接账号密码。</summary>
    public bool AuthenticationEnabled { get; set; } = true;
    /// <summary>是否启用主题权限，启用后未匹配允许规则的请求被拒绝。</summary>
    public bool AclEnabled { get; set; }
    /// <summary>是否采集消息负载用于排障。</summary>
    public bool CapturePayload { get; set; }
    /// <summary>主题权限规则，拒绝规则优先。</summary>
    public List<MqttAclRule> Rules { get; set; } = [];
}

/// <summary>MQTT 主题权限规则。</summary>
public sealed class MqttAclRule
{
    /// <summary>用户名，空值匹配所有用户。</summary>
    public string UserName { get; set; } = string.Empty;
    /// <summary>客户端 ID，空值匹配所有客户端。</summary>
    public string ClientId { get; set; } = string.Empty;
    /// <summary>主题过滤器。</summary>
    public string TopicFilter { get; set; } = string.Empty;
    /// <summary>操作：Publish、Subscribe 或 Both。</summary>
    public string Action { get; set; } = "Both";
    /// <summary>是否允许。</summary>
    public bool Allow { get; set; } = true;
}

/// <summary>访问策略响应。</summary>
public sealed class MqttAccessPolicyResponse : ResponseBase
{
    /// <summary>当前策略。</summary>
    public MqttAccessPolicy Policy { get; set; } = new();
}

/// <summary>更新访问策略请求。</summary>
public sealed class SetMqttAccessPolicyRequest : LoginedRequest
{
    /// <summary>新策略。</summary>
    public MqttAccessPolicy Policy { get; set; } = new();
}

/// <summary>测试主题权限请求。</summary>
public sealed class TestMqttAccessRequest : LoginedRequest
{
    /// <summary>用户名。</summary>
    public string UserName { get; set; } = string.Empty;
    /// <summary>客户端 ID。</summary>
    public string ClientId { get; set; } = string.Empty;
    /// <summary>主题或订阅过滤器。</summary>
    public string Topic { get; set; } = string.Empty;
    /// <summary>操作：Publish 或 Subscribe。</summary>
    public string Action { get; set; } = "Publish";
}

/// <summary>权限测试结果。</summary>
public sealed class TestMqttAccessResponse : ResponseBase
{
    /// <summary>是否允许。</summary>
    public bool Allowed { get; set; }
}

/// <summary>消息诊断查询。</summary>
public sealed class GetMqttDiagnosticsRequest : LoginedRequest
{
    /// <summary>只查询此序号之后的事件。</summary>
    public long AfterId { get; set; }
    /// <summary>主题过滤器，空值包含连接事件。</summary>
    public string TopicFilter { get; set; } = string.Empty;
    /// <summary>客户端 ID。</summary>
    public string ClientId { get; set; } = string.Empty;
    /// <summary>最多返回事件数。</summary>
    public int Limit { get; set; } = 100;
}

/// <summary>MQTT 诊断事件。</summary>
public sealed class MqttDiagnosticEvent
{
    /// <summary>事件序号。</summary>
    public long Id { get; set; }
    /// <summary>UTC 时间。</summary>
    public DateTime Timestamp { get; set; }
    /// <summary>事件类型。</summary>
    public string Kind { get; set; } = string.Empty;
    /// <summary>客户端 ID。</summary>
    public string ClientId { get; set; } = string.Empty;
    /// <summary>用户名。</summary>
    public string UserName { get; set; } = string.Empty;
    /// <summary>主题。</summary>
    public string Topic { get; set; } = string.Empty;
    /// <summary>事件说明。</summary>
    public string Detail { get; set; } = string.Empty;
    /// <summary>QoS。</summary>
    public int Qos { get; set; }
    /// <summary>Retain 标志。</summary>
    public bool Retain { get; set; }
    /// <summary>原始负载字节数。</summary>
    public int PayloadLength { get; set; }
    /// <summary>是否截断负载。</summary>
    public bool Truncated { get; set; }
    /// <summary>最多 4096 字节的 Base64 负载。</summary>
    public string PayloadBase64 { get; set; } = string.Empty;
}

/// <summary>诊断查询响应。</summary>
public sealed class MqttDiagnosticsResponse : ResponseBase
{
    /// <summary>事件列表。</summary>
    public List<MqttDiagnosticEvent> Events { get; set; } = [];
    /// <summary>下一次查询的游标。</summary>
    public long LastId { get; set; }
}

/// <summary>发送调试消息。</summary>
public sealed class PublishMqttMessageRequest : LoginedRequest
{
    /// <summary>目标主题。</summary>
    public string Topic { get; set; } = string.Empty;
    /// <summary>文本或 Base64 负载。</summary>
    public string Payload { get; set; } = string.Empty;
    /// <summary>负载是否为 Base64。</summary>
    public bool IsBase64 { get; set; }
    /// <summary>QoS。</summary>
    public int Qos { get; set; }
    /// <summary>Retain 标志。</summary>
    public bool Retain { get; set; }
}
