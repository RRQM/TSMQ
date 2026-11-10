using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TSMQServer.Db;

namespace TSMQServer.Core.Imp.Services;

/// <summary>持久化策略、完整消息保存及定期清理。</summary>
public sealed class MqttPersistenceManager(IServiceScopeFactory scopes, ILogger<MqttPersistenceManager> logger) : BackgroundService
{
    private readonly SemaphoreSlim m_gate = new(1, 1);
    private MqttPersistencePolicy m_policy = new();

    /// <summary>获取独立策略副本。</summary>
    public MqttPersistencePolicy GetPolicy() => Clone(Volatile.Read(ref this.m_policy));

    /// <inheritdoc/>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = scopes.CreateScope();
        var policy = await scope.ServiceProvider.GetRequiredService<IMqttMessageStore>().GetPolicyAsync();
        if (Validate(policy) is { } error) throw new InvalidDataException(error);
        Volatile.Write(ref this.m_policy, Clone(policy));
        await base.StartAsync(cancellationToken);
    }

    /// <summary>验证持久化策略。</summary>
    public static string? Validate(MqttPersistencePolicy? policy)
    {
        if (policy == null || policy.TopicFilters == null || policy.TopicFilters.Count > 500
            || policy.RetentionDays is < 1 or > 3650) return "主题过滤器最多 500 条，保留天数须在 1 至 3650 之间";
        if (policy.TopicFilters.Any(filter => !MqttTopicAccess.IsValid(filter, true))) return "主题过滤器无效";
        if (policy.Enabled && policy.TopicFilters.Count == 0) return "启用持久化时至少指定一个主题过滤器";
        return null;
    }

    /// <summary>保存策略并即时应用保留期限。</summary>
    public async Task SaveAsync(MqttPersistencePolicy policy)
    {
        if (Validate(policy) is { } error) throw new ArgumentException(error);
        var copy = Clone(policy);
        await this.m_gate.WaitAsync();
        try
        {
            using var scope = scopes.CreateScope();
            var store = scope.ServiceProvider.GetRequiredService<IMqttMessageStore>();
            await store.SavePolicyAsync(copy);
            Volatile.Write(ref this.m_policy, copy);
            await store.DeleteBeforeAsync(DateTime.UtcNow.AddDays(-copy.RetentionDays));
        }
        finally { this.m_gate.Release(); }
    }

    /// <summary>判断当前策略是否采集指定主题。</summary>
    public bool ShouldPersist(string topic) => Matches(Volatile.Read(ref this.m_policy), topic);

    /// <summary>保存匹配主题的完整负载，写入失败时向调用方传播错误。</summary>
    public async Task RecordAsync(string topic, ReadOnlyMemory<byte> payload, int qos, bool retain, string clientId, string userName)
    {
        if (!Matches(Volatile.Read(ref this.m_policy), topic)) return;
        var timestamp = DateTime.UtcNow;
        await this.m_gate.WaitAsync();
        try
        {
            if (!Matches(Volatile.Read(ref this.m_policy), topic)) return;
            using var scope = scopes.CreateScope();
            await scope.ServiceProvider.GetRequiredService<IMqttMessageStore>().AppendAsync(new PersistedMqttMessage
            {
                Timestamp = timestamp, Topic = topic, ClientId = clientId, UserName = userName,
                Qos = qos, Retain = retain, PayloadBase64 = Convert.ToBase64String(payload.Span), PayloadLength = payload.Length
            });
        }
        finally { this.m_gate.Release(); }
    }

    /// <summary>查询历史消息。</summary>
    public async Task<QueryMqttMessagesResponse> QueryAsync(QueryMqttMessagesRequest request)
    {
        using var scope = scopes.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<IMqttMessageStore>().QueryAsync(request);
    }

    /// <summary>按当前保留期限清理历史消息。</summary>
    public async Task CleanupAsync()
    {
        await this.m_gate.WaitAsync();
        try
        {
            using var scope = scopes.CreateScope();
            await scope.ServiceProvider.GetRequiredService<IMqttMessageStore>().DeleteBeforeAsync(
                DateTime.UtcNow.AddDays(-Volatile.Read(ref this.m_policy).RetentionDays));
        }
        finally { this.m_gate.Release(); }
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(1));
        do
        {
            try { await this.CleanupAsync(); }
            catch (Exception ex) { logger.LogError(ex, "清理持久化 MQTT 消息失败，将在下次周期重试"); }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private static bool Matches(MqttPersistencePolicy policy, string topic) =>
        policy.Enabled && policy.TopicFilters.Any(filter => MqttTopicAccess.Matches(filter, topic));

    private static MqttPersistencePolicy Clone(MqttPersistencePolicy policy) => new()
    {
        Enabled = policy.Enabled, RetentionDays = policy.RetentionDays, TopicFilters = [.. policy.TopicFilters.Distinct(StringComparer.Ordinal)]
    };
}
