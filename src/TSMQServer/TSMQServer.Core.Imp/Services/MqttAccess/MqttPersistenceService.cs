using TouchSocket.Rpc;
using TSMQServer.Core.Imp.Attributes;

namespace TSMQServer.Core.Imp.Services;

/// <summary>消息持久化管理接口实现。</summary>
[Authorize, ExceptionFilter, LoggerFilter]
public sealed class MqttPersistenceService(MqttPersistenceManager persistence) : TransientRpcServer, IMqttPersistenceService
{
    /// <inheritdoc/>
    public Task<MqttPersistencePolicyResponse> GetPolicy(LoginedRequest request) =>
        Task.FromResult(new MqttPersistencePolicyResponse { Policy = persistence.GetPolicy() }.FromSuccess("获取成功"));

    /// <inheritdoc/>
    public async Task<ResponseBase> SavePolicy(SaveMqttPersistencePolicyRequest request)
    {
        if (MqttPersistenceManager.Validate(request.Policy) is { } error) return new ResponseBase().FromFail(error);
        await persistence.SaveAsync(request.Policy);
        return new ResponseBase().FromSuccess("策略已保存并生效");
    }

    /// <inheritdoc/>
    public async Task<QueryMqttMessagesResponse> QueryMessages(QueryMqttMessagesRequest request)
    {
        if (request.TopicFilter == null || request.ClientId == null || request.BeforeId < 0
            || (request.TopicFilter.Length > 0 && !MqttTopicAccess.IsValid(request.TopicFilter, true))
            || request.Limit is < 1 or > 200 || request.StartTime > request.EndTime)
            return new QueryMqttMessagesResponse().FromFail("主题、时间范围或分页参数无效");
        return (await persistence.QueryAsync(request)).FromSuccess("查询成功");
    }
}
