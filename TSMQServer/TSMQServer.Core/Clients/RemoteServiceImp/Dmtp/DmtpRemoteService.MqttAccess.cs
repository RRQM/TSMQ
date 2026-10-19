using TouchSocket.Core;
using TouchSocket.Rpc.DmtpRpc.Generators;

namespace TSMQServer.Core.Clients;

public partial class DmtpRemoteService
{
    /// <inheritdoc/>
    public async Task<Result<MqttAccessPolicy>> GetMqttAccessPolicyAsync(LoginedRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            request.LoginedToken = this.m_loginedToken;
            var response = await this.m_actorFactory().GetMqttAccessPolicyAsync(request, this.CreateInvokeOption(cancellationToken));
            return response.Code == ResponseCode.Success ? new Result<MqttAccessPolicy>(response.Policy) : Result.FromFail(response.Message);
        }
        catch (Exception ex) { return Result.FromException(ex); }
    }
    /// <inheritdoc/>
    public async Task<Result> SetMqttAccessPolicyAsync(SetMqttAccessPolicyRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            request.LoginedToken = this.m_loginedToken;
            var response = await this.m_actorFactory().SetMqttAccessPolicyAsync(request, this.CreateInvokeOption(cancellationToken));
            return response.Code == ResponseCode.Success ? Result.Success : Result.FromFail(response.Message);
        }
        catch (Exception ex) { return Result.FromException(ex); }
    }
    /// <inheritdoc/>
    public async Task<Result<bool>> TestMqttAccessAsync(TestMqttAccessRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            request.LoginedToken = this.m_loginedToken;
            var response = await this.m_actorFactory().TestMqttAccessAsync(request, this.CreateInvokeOption(cancellationToken));
            return response.Code == ResponseCode.Success ? new Result<bool>(response.Allowed) : Result.FromFail(response.Message);
        }
        catch (Exception ex) { return Result.FromException(ex); }
    }
    /// <inheritdoc/>
    public async Task<Result<MqttDiagnosticsResponse>> GetMqttDiagnosticsAsync(GetMqttDiagnosticsRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            request.LoginedToken = this.m_loginedToken;
            var response = await this.m_actorFactory().GetMqttDiagnosticsAsync(request, this.CreateInvokeOption(cancellationToken));
            return response.Code == ResponseCode.Success ? new Result<MqttDiagnosticsResponse>(response) : Result.FromFail(response.Message);
        }
        catch (Exception ex) { return Result.FromException(ex); }
    }
    /// <inheritdoc/>
    public async Task<Result> PublishMqttMessageAsync(PublishMqttMessageRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            request.LoginedToken = this.m_loginedToken;
            var response = await this.m_actorFactory().PublishMqttMessageAsync(request, this.CreateInvokeOption(cancellationToken));
            return response.Code == ResponseCode.Success ? Result.Success : Result.FromFail(response.Message);
        }
        catch (Exception ex) { return Result.FromException(ex); }
    }
    /// <inheritdoc/>
    public async Task<Result> LogoutAsync(LoginedRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            request.LoginedToken = this.m_loginedToken;
            var response = await this.m_actorFactory().LogoutAsync(request, this.CreateInvokeOption(cancellationToken));
            return response.Code == ResponseCode.Success ? Result.Success : Result.FromFail(response.Message);
        }
        catch (Exception ex) { return Result.FromException(ex); }
    }
}
