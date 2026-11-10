using System.ComponentModel;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.JsonRpc;
using TouchSocket.Rpc;
using TouchSocket.WebApi;

namespace TSMQServer.Core;

/// <summary>MQTT 访问控制及消息排障服务。</summary>
[GeneratorRpcProxy]
public interface IMqttAccessService : ITransientRpcServer
{
    /// <summary>获取访问策略。</summary>
    [Description("获取MQTT访问策略"), WebApi(Method = HttpMethodType.Post), DmtpRpc, JsonRpc(MethodInvoke = true)]
    Task<MqttAccessPolicyResponse> GetMqttAccessPolicy(LoginedRequest request);
    /// <summary>保存访问策略。</summary>
    [Description("保存MQTT访问策略"), WebApi(Method = HttpMethodType.Post), DmtpRpc, JsonRpc(MethodInvoke = true)]
    Task<ResponseBase> SetMqttAccessPolicy(SetMqttAccessPolicyRequest request);
    /// <summary>测试主题权限。</summary>
    [Description("测试MQTT主题权限"), WebApi(Method = HttpMethodType.Post), DmtpRpc, JsonRpc(MethodInvoke = true)]
    Task<TestMqttAccessResponse> TestMqttAccess(TestMqttAccessRequest request);
    /// <summary>查询诊断事件。</summary>
    [Description("查询MQTT诊断事件"), WebApi(Method = HttpMethodType.Post), DmtpRpc, JsonRpc(MethodInvoke = true)]
    Task<MqttDiagnosticsResponse> GetMqttDiagnostics(GetMqttDiagnosticsRequest request);
    /// <summary>发送调试消息。</summary>
    [Description("发送MQTT调试消息"), WebApi(Method = HttpMethodType.Post), DmtpRpc, JsonRpc(MethodInvoke = true)]
    Task<ResponseBase> PublishMqttMessage(PublishMqttMessageRequest request);
}
