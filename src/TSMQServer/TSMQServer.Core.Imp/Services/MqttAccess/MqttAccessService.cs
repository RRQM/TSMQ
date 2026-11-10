using TouchSocket.Rpc;
using TSMQServer.Core.Imp.Attributes;
using System.Security.Claims;

namespace TSMQServer.Core.Imp.Services;

/// <summary>MQTT 访问控制与排障接口实现。</summary>
[Authorize, ExceptionFilter, LoggerFilter]
public sealed class MqttAccessService(MqttAccessManager access, MqttDiagnostics diagnostics, IMqttServerHostService host)
    : TransientRpcServer, IMqttAccessService
{
    /// <inheritdoc/>
    public Task<MqttAccessPolicyResponse> GetMqttAccessPolicy(LoginedRequest request) =>
        Task.FromResult(new MqttAccessPolicyResponse { Policy = access.GetPolicy() }.FromSuccess("获取成功"));

    /// <inheritdoc/>
    public async Task<ResponseBase> SetMqttAccessPolicy(SetMqttAccessPolicyRequest request)
    {
        try { await access.SaveAsync(request.Policy); }
        catch (ArgumentException ex) { return new ResponseBase().FromFail(ex.Message); }
        return new ResponseBase().FromSuccess("策略已保存，现有客户端需重新连接并订阅");
    }

    /// <inheritdoc/>
    public Task<TestMqttAccessResponse> TestMqttAccess(TestMqttAccessRequest request) =>
        Task.FromResult(new TestMqttAccessResponse
        {
            Allowed = MqttTopicAccess.IsAllowed(access.GetPolicy(), request.UserName, request.ClientId, request.Topic, request.Action)
        }.FromSuccess("测试完成"));

    /// <inheritdoc/>
    public Task<MqttDiagnosticsResponse> GetMqttDiagnostics(GetMqttDiagnosticsRequest request)
    {
        if (request.TopicFilter.Length > 0 && !MqttTopicAccess.IsValid(request.TopicFilter, true))
            return Task.FromResult(new MqttDiagnosticsResponse().FromFail("主题过滤器无效"));
        return Task.FromResult(diagnostics.Query(request).FromSuccess("获取成功"));
    }

    /// <inheritdoc/>
    public async Task<ResponseBase> PublishMqttMessage(PublishMqttMessageRequest request)
    {
        if (!MqttTopicAccess.IsValid(request.Topic, false) || request.Qos is < 0 or > 2 || request.Payload.Length > 90000)
            return new ResponseBase().FromFail("主题、QoS 或消息大小无效");
        if (request.Retain) return new ResponseBase().FromFail("当前 Broker 未提供保留消息存储，暂不支持 Retain 调试发布");
        byte[] payload;
        try { payload = request.IsBase64 ? Convert.FromBase64String(request.Payload) : Encoding.UTF8.GetBytes(request.Payload); }
        catch (FormatException) { return new ResponseBase().FromFail("Base64 负载无效"); }
        if (payload.Length > 65536) return new ResponseBase().FromFail("消息负载最多 64 KiB");
        var count = await host.PublishMessageAsync(request.Topic, payload, request.Qos, request.Retain);
        var principal = this.CallContext.GetValue(AuthorizeAttribute.JwtPrincipalProperty);
        diagnostics.Record("DebugPublish", "[console]", principal?.FindFirst(ClaimTypes.Name)?.Value ?? "",
            request.Topic, $"管理端发布，已提交至 {count} 个订阅队列", payload, request.Qos, request.Retain, access.GetPolicy().CapturePayload);
        return new ResponseBase().FromSuccess($"已提交至 {count} 个订阅队列");
    }
}
