using System.Collections.Concurrent;
using System.Buffers;
using TSMQServer.Core.Imp.Plugins;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using TouchSocket.Core;
using TouchSocket.Mqtt;
using TouchSocket.Http;
using TSMQServer.Core.Json;
using TSMQServer.Db;

namespace TSMQServer.Core.Imp.Services;

/// <summary>MQTT 认证、主题授权及连接撤销。</summary>
public sealed class MqttAccessManager(IServiceScopeFactory scopeFactory, MqttDiagnostics diagnostics)
{
    private readonly ConcurrentDictionary<IMqttSession, Connection> m_connections = new();
    private readonly ConcurrentDictionary<string, string> m_sessionOwners = new();
    private readonly SemaphoreSlim m_gate = new(1, 1);
    private readonly MqttOperationGate m_operations = new();
    private MqttAccessPolicy m_policy = new();
    private MqttBroker m_broker = null!;

    /// <summary>读取独立策略副本。</summary>
    public MqttAccessPolicy GetPolicy() => Clone(Volatile.Read(ref this.m_policy));

    /// <summary>在监听启动前加载策略。</summary>
    public async Task InitializeAsync(MqttBroker broker)
    {
        this.m_broker = broker;
        using var scope = scopeFactory.CreateScope();
        var policy = await scope.ServiceProvider.GetRequiredService<IMqttAccessPolicyFromDB>().GetAsync();
        Validate(policy);
        Volatile.Write(ref this.m_policy, Clone(policy));
    }

    /// <summary>保存策略并撤销现有连接及订阅。</summary>
    public async Task SaveAsync(MqttAccessPolicy policy)
    {
        Validate(policy);
        var copy = Clone(policy);
        IMqttSession[] revoked;
        using (await this.m_operations.EnterAsync(true))
        {
            using var scope = scopeFactory.CreateScope();
            await scope.ServiceProvider.GetRequiredService<IMqttAccessPolicyFromDB>().SaveAsync(copy);
            Volatile.Write(ref this.m_policy, copy);
            revoked = this.RevokeCore();
        }
        await CloseRevokedAsync(revoked);
    }

    /// <summary>认证用户变更后撤销所有连接。</summary>
    public async Task RevokeAsync()
    {
        IMqttSession[] revoked;
        using (await this.m_operations.EnterAsync(true)) revoked = this.RevokeCore();
        await CloseRevokedAsync(revoked);
    }

    /// <summary>在同一策略租期内完成校验和协议处理，避免撤销后重新注册订阅。</summary>
    public async Task ProcessAsync(IMqttSession session, MqttMessage message, Func<MqttMessage, Task> send, Func<Task> process)
    {
        using (await this.m_operations.EnterAsync())
            if (await this.CheckAsync(session, message, send)) await process();
    }

    /// <summary>在协议处理前验证连接及消息，返回是否继续处理。</summary>
    private async Task<bool> CheckAsync(IMqttSession session, MqttMessage message, Func<MqttMessage, Task> send)
    {
        if (message is MqttConnectMessage connect)
        {
            await this.m_gate.WaitAsync();
            try
            {
                var policy = Volatile.Read(ref this.m_policy);
                var user = connect.UserName ?? string.Empty;
                string? rejection = this.m_connections.ContainsKey(session) ? "同一连接重复发送 CONNECT"
                    : string.IsNullOrEmpty(connect.ClientId) ? "客户端 ID 不能为空"
                    : this.m_connections.Values.Any(c => c.ClientId == connect.ClientId) ? "客户端 ID 已被占用" : null;
                if (rejection == null && policy.AuthenticationEnabled)
                {
                    using var scope = scopeFactory.CreateScope();
                    if (user.Length == 0 || !await scope.ServiceProvider.GetRequiredService<IMqttConnectAuthFromDB>()
                        .ValidateAsync(user, connect.Password ?? string.Empty)) rejection = "账号或密码不正确";
                }
                if (rejection == null && connect.WillFlag
                    && !MqttTopicAccess.IsAllowed(policy, user, connect.ClientId, connect.WillTopic, "Publish"))
                    rejection = "遗嘱主题发布无权限";
                if (rejection != null)
                {
                    diagnostics.Record("ConnectionRejected", connect.ClientId, user, detail: rejection);
                    var ack = new MqttConnAckMessage {
                        ReturnCode = connect.Version == MqttProtocolVersion.V500 ? MqttReasonCode.NotAuthorized : MqttReasonCode.ConnectionRefusedNotAuthorized,
                        ReasonCode = MqttReasonCode.NotAuthorized };
                    MqttProtocolAccess.SetVersion(ack, connect.Version);
                    await send(ack);
                    this.RequestClose(session, "连接未授权", true);
                    return false;
                }
                if (this.m_sessionOwners.TryGetValue(connect.ClientId, out var owner) && owner != user)
                {
                    foreach (var topic in this.m_broker.GetAllTopics()) this.m_broker.UnregisterActor(connect.ClientId, topic);
                    this.m_broker.RemoveMqttSessionActor(connect.ClientId);
                }
                this.m_sessionOwners[connect.ClientId] = user;
                this.m_connections[session] = new Connection(connect.ClientId, user);
                diagnostics.Record("Connected", connect.ClientId, user, detail: "认证通过");
                return true;
            }
            finally { this.m_gate.Release(); }
        }

        if (!this.m_connections.TryGetValue(session, out var connection) || connection.Revoked)
        {
            this.RequestClose(session, "请先完成认证");
            return false;
        }
        var current = Volatile.Read(ref this.m_policy);
        if (message is MqttPublishMessage publish)
        {
            if (!MqttTopicAccess.IsAllowed(current, connection.UserName, connection.ClientId, publish.TopicName, "Publish"))
            {
                diagnostics.Record("AccessDenied", connection.ClientId, connection.UserName, publish.TopicName, "发布无权限，连接已关闭");
                this.RequestClose(session, "发布无权限");
                return false;
            }
        }
        else if (message is MqttSubscribeMessage subscribe)
        {
            if (subscribe.SubscribeRequests.Any(r => !MqttTopicAccess.IsAllowed(current, connection.UserName, connection.ClientId, r.Topic, "Subscribe")))
            {
                var ack = new MqttSubAckMessage { MessageId = subscribe.MessageId };
                MqttProtocolAccess.SetVersion(ack, subscribe.Version);
                foreach (var request in subscribe.SubscribeRequests)
                {
                    ack.AddReturnCode(subscribe.Version == MqttProtocolVersion.V500 ? MqttReasonCode.NotAuthorized : (MqttReasonCode)128);
                    diagnostics.Record("AccessDenied", connection.ClientId, connection.UserName, request.Topic, "订阅请求被拒绝，本批订阅均未生效");
                }
                await send(ack);
                return false;
            }
            foreach (var request in subscribe.SubscribeRequests)
                diagnostics.Record("Subscribed", connection.ClientId, connection.UserName, request.Topic, "订阅已授权");
        }
        return true;
    }

    /// <summary>记录已处理的消息。</summary>
    public void RecordMessage(IMqttSession session, MqttArrivedMessage message)
    {
        if (this.m_connections.TryGetValue(session, out var connection))
        {
            var capture = Volatile.Read(ref this.m_policy).CapturePayload;
            diagnostics.Record("Message", connection.ClientId, connection.UserName, message.TopicName, "Broker 已接收",
                capture ? message.Payload.Slice(0, Math.Min(message.Payload.Length, 4096)).ToArray() : default,
                (int)message.QosLevel, message.Retain, capture, checked((int)message.Payload.Length));
        }
    }

    /// <summary>释放连接。</summary>
    public void Closed(IMqttSession session, string detail)
    {
        if (this.m_connections.TryGetValue(session, out var closing) && session switch
            {
                MqttTcpSessionClient tcp => tcp.CleanSession,
                AuthorizedMqttWebSocketSessionClient ws => ws.CleanSession,
                _ => false
            })
        {
            foreach (var topic in this.m_broker.GetAllTopics()) this.m_broker.UnregisterActor(closing.ClientId, topic);
            this.m_sessionOwners.TryRemove(closing.ClientId, out _);
        }
        if (this.m_connections.TryRemove(session, out var connection))
            diagnostics.Record("Disconnected", connection.ClientId, connection.UserName, detail: detail);
    }

    private IMqttSession[] RevokeCore()
    {
        var sessions = this.m_connections.Keys.ToArray();
        foreach (var session in sessions)
        {
            if (this.m_connections.TryGetValue(session, out var connection)) connection.Revoked = true;
            MqttProtocolAccess.ClearWill(session);
        }
        foreach (var topic in this.m_broker.GetAllTopics())
        {
            foreach (var subscriber in this.m_broker.GetSubscribers(topic))
            {
                this.m_broker.RemoveSubscriber(topic, subscriber);
                this.m_broker.RemoveMqttSessionActor(subscriber.ClientId);
            }
        }
        foreach (var clientId in this.m_sessionOwners.Keys) this.m_broker.RemoveMqttSessionActor(clientId);
        this.m_sessionOwners.Clear();
        diagnostics.Record("PolicyChanged", detail: "访问策略或认证用户变更，现有连接和订阅已撤销");
        return sessions;
    }

    // 不在策略锁内等待关闭：接收循环可能正在等待下一次策略租期。
    private static Task CloseRevokedAsync(IMqttSession[] sessions) => Task.WhenAll(
        sessions.Select(session => CloseAsync(session, "访问策略或认证用户已更新，请重新连接")));

    private static async Task CloseAsync(IMqttSession session, string reason)
    {
        if (session is IMqttWebSocketSessionClient ws) await ws.Client.CloseAsync(reason);
        else await session.CloseAsync(reason);
    }

    private void RequestClose(IMqttSession session, string reason, bool drainReply = false)
    {
        if (this.m_connections.TryGetValue(session, out var connection)) connection.Revoked = true;
        _ = Task.Run(async () =>
        {
            try
            {
                if (drainReply) await Task.Delay(100);
                await CloseAsync(session, reason);
            }
            catch { diagnostics.Record("CloseFailed", detail: "关闭 MQTT 连接失败"); }
        });
    }

    private static MqttAccessPolicy Clone(MqttAccessPolicy policy) => JsonSerializer.Deserialize(
        JsonSerializer.Serialize(policy, TSMQServerCoreJsonSerializerContext.Default.MqttAccessPolicy),
        TSMQServerCoreJsonSerializerContext.Default.MqttAccessPolicy)!;

    private static void Validate(MqttAccessPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);
        if (policy.Rules == null || policy.Rules.Count > 500) throw new ArgumentException("主题权限规则最多 500 条");
        foreach (var rule in policy.Rules)
        {
            if (rule == null || rule.UserName == null || rule.ClientId == null || !MqttTopicAccess.IsValid(rule.TopicFilter, true)
                || rule.Action is not ("Publish" or "Subscribe" or "Both"))
                throw new ArgumentException("主题权限规则无效");
        }
    }

    private sealed record Connection(string ClientId, string UserName)
    {
        public volatile bool Revoked;
    }
}
