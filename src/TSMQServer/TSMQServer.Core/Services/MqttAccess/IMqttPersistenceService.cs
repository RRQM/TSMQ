using System.ComponentModel;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.JsonRpc;
using TouchSocket.Rpc;
using TouchSocket.WebApi;

namespace TSMQServer.Core;

/// <summary>主题消息持久化管理。</summary>
[GeneratorRpcProxy]
public interface IMqttPersistenceService : ITransientRpcServer
{
    /// <summary>获取持久化策略。</summary>
    [Description("获取消息持久化策略"), WebApi(Method = HttpMethodType.Post), DmtpRpc, JsonRpc(MethodInvoke = true)]
    Task<MqttPersistencePolicyResponse> GetPolicy(LoginedRequest request);
    /// <summary>保存持久化策略。</summary>
    [Description("保存消息持久化策略"), WebApi(Method = HttpMethodType.Post), DmtpRpc, JsonRpc(MethodInvoke = true)]
    Task<ResponseBase> SavePolicy(SaveMqttPersistencePolicyRequest request);
    /// <summary>查询历史消息。</summary>
    [Description("查询持久化消息"), WebApi(Method = HttpMethodType.Post), DmtpRpc, JsonRpc(MethodInvoke = true)]
    Task<QueryMqttMessagesResponse> QueryMessages(QueryMqttMessagesRequest request);
}
