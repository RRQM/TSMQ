// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using TouchSocket.Core;
using TouchSocket.Rpc.JsonRpc.Generators;

namespace TSMQServer.Core.Clients;

public partial class JsonRpcRemoteService : IRemoteService
{
    public async Task<Result<List<MqttServerInfo>>> GetMqttServerListAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await this.m_jsonRpcClient.GetMqttServerListAsync(new LoginedRequest
            {
                LoginedToken = this.m_loginedToken
            }, CreateInvokeOption(cancellationToken));
            return response.Code == ResponseCode.Success
                ? new Result<List<MqttServerInfo>>(response.Servers!)
                : Result.FromFail(response.Message);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    public async Task<Result<MqttServerInfo>> AddMqttServerAsync(MqttServerType type, string host, int port, string path, string? sslCertId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await this.m_jsonRpcClient.AddMqttServerAsync(new AddMqttServerRequest
            {
                LoginedToken = this.m_loginedToken,
                Type = type,
                Host = host,
                Port = port,
                Path = path,
                SslCertId = sslCertId
            }, CreateInvokeOption(cancellationToken));
            return response.Code == ResponseCode.Success
                ? new Result<MqttServerInfo>(response.Server!)
                : Result.FromFail(response.Message);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    public async Task<Result> StartMqttServerAsync(string serverId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await this.m_jsonRpcClient.StartMqttServerAsync(new MqttServerIdRequest
            {
                LoginedToken = this.m_loginedToken,
                ServerId = serverId
            }, CreateInvokeOption(cancellationToken));
            return response.Code == ResponseCode.Success ? Result.Success : Result.FromFail(response.Message);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    public async Task<Result> StopMqttServerAsync(string serverId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await this.m_jsonRpcClient.StopMqttServerAsync(new MqttServerIdRequest
            {
                LoginedToken = this.m_loginedToken,
                ServerId = serverId
            }, CreateInvokeOption(cancellationToken));
            return response.Code == ResponseCode.Success ? Result.Success : Result.FromFail(response.Message);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    public async Task<Result> RemoveMqttServerAsync(string serverId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await this.m_jsonRpcClient.RemoveMqttServerAsync(new MqttServerIdRequest
            {
                LoginedToken = this.m_loginedToken,
                ServerId = serverId
            }, CreateInvokeOption(cancellationToken));
            return response.Code == ResponseCode.Success ? Result.Success : Result.FromFail(response.Message);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    public async Task<Result<List<MqttAuthUserInfo>>> GetMqttAuthUsersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await this.m_jsonRpcClient.GetMqttAuthUsersAsync(new LoginedRequest
            {
                LoginedToken = this.m_loginedToken
            }, CreateInvokeOption(cancellationToken));
            return response.Code == ResponseCode.Success
                ? new Result<List<MqttAuthUserInfo>>(response.Users!)
                : Result.FromFail(response.Message);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    public async Task<Result> AddMqttAuthUserAsync(string userName, string password, string? remark, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await this.m_jsonRpcClient.AddMqttAuthUserAsync(new MqttAuthUserRequest
            {
                LoginedToken = this.m_loginedToken,
                UserName = userName,
                Password = password,
                Remark = remark
            }, CreateInvokeOption(cancellationToken));
            return response.Code == ResponseCode.Success ? Result.Success : Result.FromFail(response.Message);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    public async Task<Result> UpdateMqttAuthUserAsync(string userName, string password, string? remark, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await this.m_jsonRpcClient.UpdateMqttAuthUserAsync(new MqttAuthUserRequest
            {
                LoginedToken = this.m_loginedToken,
                UserName = userName,
                Password = password,
                Remark = remark
            }, CreateInvokeOption(cancellationToken));
            return response.Code == ResponseCode.Success ? Result.Success : Result.FromFail(response.Message);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    public async Task<Result> DeleteMqttAuthUserAsync(string userName, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await this.m_jsonRpcClient.DeleteMqttAuthUserAsync(new MqttAuthUserDeleteRequest
            {
                LoginedToken = this.m_loginedToken,
                UserName = userName
            }, CreateInvokeOption(cancellationToken));
            return response.Code == ResponseCode.Success ? Result.Success : Result.FromFail(response.Message);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    public async Task<Result<MqttBrokerSettingsResponse>> GetMqttBrokerSettingsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await this.m_jsonRpcClient.GetMqttBrokerSettingsAsync(new LoginedRequest
            {
                LoginedToken = this.m_loginedToken
            }, CreateInvokeOption(cancellationToken));
            return response.Code == ResponseCode.Success
                ? new Result<MqttBrokerSettingsResponse>(response)
                : Result.FromFail(response.Message);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    public async Task<Result> SetMqttBrokerSettingsAsync(int messageCapacity, int messageExpirySeconds, int sessionExpirySeconds, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await this.m_jsonRpcClient.SetMqttBrokerSettingsAsync(new SetMqttBrokerSettingsRequest
            {
                LoginedToken = this.m_loginedToken,
                MessageCapacity = messageCapacity,
                MessageExpirySeconds = messageExpirySeconds,
                SessionExpirySeconds = sessionExpirySeconds
            }, CreateInvokeOption(cancellationToken));
            return response.Code == ResponseCode.Success ? Result.Success : Result.FromFail(response.Message);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    public async Task<Result<MqttDiscardedStatsResponse>> GetMqttDiscardedStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await this.m_jsonRpcClient.GetMqttDiscardedStatsAsync(new LoginedRequest
            {
                LoginedToken = this.m_loginedToken
            }, CreateInvokeOption(cancellationToken));
            return response.Code == ResponseCode.Success
                ? new Result<MqttDiscardedStatsResponse>(response)
                : Result.FromFail(response.Message);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    public async Task<Result> ResetMqttDiscardedStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await this.m_jsonRpcClient.ResetMqttDiscardedStatsAsync(new LoginedRequest
            {
                LoginedToken = this.m_loginedToken
            }, CreateInvokeOption(cancellationToken));
            return response.Code == ResponseCode.Success ? Result.Success : Result.FromFail(response.Message);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    public async Task<Result<MqttDashboardStatsResponse>> GetMqttDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await this.m_jsonRpcClient.GetMqttDashboardStatsAsync(new LoginedRequest
            {
                LoginedToken = this.m_loginedToken
            }, CreateInvokeOption(cancellationToken));
            return response.Code == ResponseCode.Success
                ? new Result<MqttDashboardStatsResponse>(response)
                : Result.FromFail(response.Message);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    public async Task<Result<List<MqttStatsSnapshot>>> GetDashboardStatsHistoryAsync(string granularity, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await this.m_jsonRpcClient.GetDashboardStatsHistoryAsync(new GetDashboardStatsHistoryRequest
            {
                LoginedToken = this.m_loginedToken,
                Granularity = granularity
            }, CreateInvokeOption(cancellationToken));
            return response.Code == ResponseCode.Success
                ? new Result<List<MqttStatsSnapshot>>(response.Snapshots!)
                : Result.FromFail(response.Message);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    public async Task<Result<List<MqttSslCertInfo>>> GetMqttSslCertsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await this.m_jsonRpcClient.GetMqttSslCertsAsync(new LoginedRequest
            {
                LoginedToken = this.m_loginedToken
            }, CreateInvokeOption(cancellationToken));
            return response.Code == ResponseCode.Success
                ? new Result<List<MqttSslCertInfo>>(response.Certs!)
                : Result.FromFail(response.Message);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    public async Task<Result<MqttSslCertInfo>> AddMqttSslCertAsync(string name, MqttSslCertType type, string certData, string? keyData, string? password, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await this.m_jsonRpcClient.AddMqttSslCertAsync(new AddMqttSslCertRequest
            {
                LoginedToken = this.m_loginedToken,
                Name = name,
                Type = type,
                CertData = certData,
                KeyData = keyData,
                Password = password
            }, CreateInvokeOption(cancellationToken));
            return response.Code == ResponseCode.Success
                ? new Result<MqttSslCertInfo>(response.Cert!)
                : Result.FromFail(response.Message);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    public async Task<Result> DeleteMqttSslCertAsync(string certId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await this.m_jsonRpcClient.DeleteMqttSslCertAsync(new MqttSslCertIdRequest
            {
                LoginedToken = this.m_loginedToken,
                CertId = certId
            }, CreateInvokeOption(cancellationToken));
            return response.Code == ResponseCode.Success ? Result.Success : Result.FromFail(response.Message);
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }
}
