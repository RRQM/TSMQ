namespace TSMQServer.Core.Imp.Services;

/// <summary>有容量限制的 MQTT 内存诊断记录。</summary>
public sealed class MqttDiagnostics
{
    private readonly object m_lock = new();
    private readonly Queue<MqttDiagnosticEvent> m_events = new();
    private long m_id;

    /// <summary>记录事件，不采集密码或令牌。</summary>
    public void Record(string kind, string clientId = "", string userName = "", string topic = "", string detail = "",
        ReadOnlyMemory<byte> payload = default, int qos = 0, bool retain = false, bool capturePayload = false, int? originalLength = null)
    {
        lock (this.m_lock)
        {
            var length = capturePayload ? Math.Min(payload.Length, 4096) : 0;
            this.m_events.Enqueue(new MqttDiagnosticEvent
            {
                Id = ++this.m_id, Timestamp = DateTime.UtcNow, Kind = kind, ClientId = clientId,
                UserName = userName, Topic = topic, Detail = detail, Qos = qos, Retain = retain,
                PayloadLength = originalLength ?? payload.Length, Truncated = capturePayload && length < (originalLength ?? payload.Length),
                PayloadBase64 = Convert.ToBase64String(payload.Span[..length])
            });
            while (this.m_events.Count > 1000) this.m_events.Dequeue();
        }
    }

    /// <summary>按序号、主题和客户端查询事件。</summary>
    public MqttDiagnosticsResponse Query(GetMqttDiagnosticsRequest request)
    {
        lock (this.m_lock)
        {
            var events = this.m_events.Where(e => e.Id > request.AfterId
                && (request.ClientId.Length == 0 || e.ClientId == request.ClientId)
                && (request.TopicFilter.Length == 0 || MqttTopicAccess.Matches(request.TopicFilter, e.Topic)))
                .Take(Math.Clamp(request.Limit, 1, 200)).ToList();
            return new MqttDiagnosticsResponse
            {
                Events = events,
                LastId = events.Count == Math.Clamp(request.Limit, 1, 200) ? events[^1].Id : this.m_id
            };
        }
    }
}
