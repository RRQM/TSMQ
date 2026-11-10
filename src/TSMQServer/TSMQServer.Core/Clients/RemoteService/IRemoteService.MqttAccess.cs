using TouchSocket.Core;

namespace TSMQServer.Core.Clients;

public partial interface IRemoteService
{
    /// <summary>调用 GetMqttAccessPolicy 服务。</summary>
    Task<Result<MqttAccessPolicy>> GetMqttAccessPolicyAsync(LoginedRequest request, CancellationToken cancellationToken = default);
    /// <summary>调用 SetMqttAccessPolicy 服务。</summary>
    Task<Result> SetMqttAccessPolicyAsync(SetMqttAccessPolicyRequest request, CancellationToken cancellationToken = default);
    /// <summary>调用 TestMqttAccess 服务。</summary>
    Task<Result<bool>> TestMqttAccessAsync(TestMqttAccessRequest request, CancellationToken cancellationToken = default);
    /// <summary>调用 GetMqttDiagnostics 服务。</summary>
    Task<Result<MqttDiagnosticsResponse>> GetMqttDiagnosticsAsync(GetMqttDiagnosticsRequest request, CancellationToken cancellationToken = default);
    /// <summary>调用 PublishMqttMessage 服务。</summary>
    Task<Result> PublishMqttMessageAsync(PublishMqttMessageRequest request, CancellationToken cancellationToken = default);
    /// <summary>调用 Logout 服务。</summary>
    Task<Result> LogoutAsync(LoginedRequest request, CancellationToken cancellationToken = default);
}
