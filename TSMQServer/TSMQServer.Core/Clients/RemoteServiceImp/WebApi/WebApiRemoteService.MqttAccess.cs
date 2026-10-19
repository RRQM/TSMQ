using TouchSocket.Core;
using TouchSocket.Rpc.WebApi.Generators;

namespace TSMQServer.Core.Clients;

public partial class WebApiRemoteService
{
    /// <inheritdoc/>
    public async Task<Result<MqttAccessPolicy>> GetMqttAccessPolicyAsync(LoginedRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            request.LoginedToken = this.m_loginedToken;
            var response = await this.m_webApiClient.GetMqttAccessPolicyAsync(request, CreateInvokeOption(cancellationToken));
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
            var response = await this.m_webApiClient.SetMqttAccessPolicyAsync(request, CreateInvokeOption(cancellationToken));
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
            var response = await this.m_webApiClient.TestMqttAccessAsync(request, CreateInvokeOption(cancellationToken));
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
            var response = await this.m_webApiClient.GetMqttDiagnosticsAsync(request, CreateInvokeOption(cancellationToken));
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
            var response = await this.m_webApiClient.PublishMqttMessageAsync(request, CreateInvokeOption(cancellationToken));
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
            var response = await this.m_webApiClient.LogoutAsync(request, CreateInvokeOption(cancellationToken));
            return response.Code == ResponseCode.Success ? Result.Success : Result.FromFail(response.Message);
        }
        catch (Exception ex) { return Result.FromException(ex); }
    }
}
