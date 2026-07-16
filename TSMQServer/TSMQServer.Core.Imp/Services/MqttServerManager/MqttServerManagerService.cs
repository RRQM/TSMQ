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

using Microsoft.Extensions.Logging;
using TSMQServer.Core.Imp.Attributes;
using TouchSocket.Rpc;
using TSMQServer.Core;
using TSMQServer.Db;

namespace TSMQServer.Core.Imp.Services;

/// <summary>
/// MQTT 服务器管理服务实现。
/// </summary>
[Authorize]
[ExceptionFilter]
[LoggerFilter]
public partial class MqttServerManagerService : TransientRpcServer, IMqttServerManagerService
{
    private readonly IMqttConnectAuthFromDB m_authFromDB;
    private readonly ILogger<MqttServerManagerService> m_logger;
    private readonly IMqttServerHostService m_mqttHostService;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public MqttServerManagerService(ILogger<MqttServerManagerService> logger, IMqttServerHostService mqttHostService, IMqttConnectAuthFromDB authFromDB)
    {
        this.m_logger = logger;
        this.m_mqttHostService = mqttHostService;
        this.m_authFromDB = authFromDB;
    }

    /// <inheritdoc/>
    public Task<MqttServerListResponse> GetMqttServerList(LoginedRequest request)
    {
        var response = new MqttServerListResponse();
        var servers = this.m_mqttHostService.GetServers();
        response.Servers = [.. servers];
        return Task.FromResult(response.FromSuccess("获取成功"));
    }

    /// <inheritdoc/>
    public async Task<MqttServerResponse> AddMqttServer(AddMqttServerRequest request)
    {
        var response = new MqttServerResponse();

        if (request.Port <= 0 || request.Port > 65535)
        {
            return response.FromFail("端口号无效，需在 1~65535 范围内");
        }

        var host = string.IsNullOrWhiteSpace(request.Host) ? "0.0.0.0" : request.Host;

        MqttServerInfo? info;
        if (request.Type == MqttServerType.Tcp)
        {
            info = await this.m_mqttHostService.AddTcpServerAsync(host, request.Port, request.SslCertId);
            if (info == null)
            {
                return response.FromFail($"TCP 端口 {request.Port} 已被占用");
            }
        }
        else
        {
            var path = string.IsNullOrWhiteSpace(request.Path) ? "/mqtt" : request.Path;
            info = await this.m_mqttHostService.AddWebSocketServerAsync(host, request.Port, path, request.SslCertId);
            if (info == null)
            {
                return response.FromFail($"WebSocket 端口 {request.Port} 已被占用");
            }
        }

        response.Server = info;
        return response.FromSuccess($"添加 {request.Type} MQTT 服务器成功");
    }

    /// <inheritdoc/>
    public async Task<ResponseBase> StartMqttServer(MqttServerIdRequest request)
    {
        var response = new ResponseBase();

        if (string.IsNullOrWhiteSpace(request.ServerId))
        {
            return response.FromFail("服务器 ID 不能为空");
        }

        var success = await this.m_mqttHostService.StartServerAsync(request.ServerId);
        return success
            ? response.FromSuccess("启动成功")
            : response.FromFail("服务器不存在或启动失败");
    }

    /// <inheritdoc/>
    public async Task<ResponseBase> StopMqttServer(MqttServerIdRequest request)
    {
        var response = new ResponseBase();

        if (string.IsNullOrWhiteSpace(request.ServerId))
        {
            return response.FromFail("服务器 ID 不能为空");
        }

        var success = await this.m_mqttHostService.StopServerAsync(request.ServerId);
        return success
            ? response.FromSuccess("停止成功")
            : response.FromFail("服务器不存在或停止失败");
    }

    /// <inheritdoc/>
    public async Task<ResponseBase> RemoveMqttServer(MqttServerIdRequest request)
    {
        var response = new ResponseBase();

        if (string.IsNullOrWhiteSpace(request.ServerId))
        {
            return response.FromFail("服务器 ID 不能为空");
        }

        var success = await this.m_mqttHostService.RemoveServerAsync(request.ServerId);
        return success
            ? response.FromSuccess("移除成功")
            : response.FromFail("服务器不存在");
    }

    /// <inheritdoc/>
    public async Task<MqttAuthUserListResponse> GetMqttAuthUsers(LoginedRequest request)
    {
        var response = new MqttAuthUserListResponse();
        var users = await this.m_authFromDB.GetAllAsync();
        response.Users = users;
        return response.FromSuccess("获取成功");
    }

    /// <inheritdoc/>
    public async Task<ResponseBase> AddMqttAuthUser(MqttAuthUserRequest request)
    {
        var response = new ResponseBase();

        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            return response.FromFail("用户名不能为空");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return response.FromFail("密码不能为空");
        }

        var info = new MqttAuthUserInfo
        {
            UserName = request.UserName,
            Password = request.Password,
            Remark = request.Remark,
            CreatedAt = DateTime.UtcNow
        };

        var success = await this.m_authFromDB.AddAsync(info);
        return success
            ? response.FromSuccess("添加成功")
            : response.FromFail("用户名已存在");
    }

    /// <inheritdoc/>
    public async Task<ResponseBase> UpdateMqttAuthUser(MqttAuthUserRequest request)
    {
        var response = new ResponseBase();

        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            return response.FromFail("用户名不能为空");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return response.FromFail("密码不能为空");
        }

        var info = new MqttAuthUserInfo
        {
            UserName = request.UserName,
            Password = request.Password,
            Remark = request.Remark
        };

        var success = await this.m_authFromDB.UpdateAsync(info);
        return success
            ? response.FromSuccess("更新成功")
            : response.FromFail("用户不存在");
    }

    /// <inheritdoc/>
    public async Task<ResponseBase> DeleteMqttAuthUser(MqttAuthUserDeleteRequest request)
    {
        var response = new ResponseBase();

        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            return response.FromFail("用户名不能为空");
        }

        var success = await this.m_authFromDB.DeleteAsync(request.UserName);
        return success
            ? response.FromSuccess("删除成功")
            : response.FromFail("用户不存在");
    }

    /// <inheritdoc/>
    public Task<MqttBrokerSettingsResponse> GetMqttBrokerSettings(LoginedRequest request)
    {
        var (capacity, msgExpiry, sessionExpiry) = this.m_mqttHostService.GetBrokerSettings();
        var response = new MqttBrokerSettingsResponse
        {
            MessageCapacity = capacity,
            MessageExpirySeconds = msgExpiry,
            SessionExpirySeconds = sessionExpiry
        };
        return Task.FromResult(response.FromSuccess("获取成功"));
    }

    /// <inheritdoc/>
    public async Task<ResponseBase> SetMqttBrokerSettings(SetMqttBrokerSettingsRequest request)
    {
        var response = new ResponseBase();
        if (request.MessageCapacity < 0)
        {
            return response.FromFail("消息队列容量不能为负数");
        }
        if (request.MessageExpirySeconds < 0)
        {
            return response.FromFail("消息过期时间不能为负数");
        }
        if (request.SessionExpirySeconds < 0)
        {
            return response.FromFail("会话过期时间不能为负数");
        }
        await this.m_mqttHostService.SetBrokerSettings(request.MessageCapacity, request.MessageExpirySeconds, request.SessionExpirySeconds);
        return response.FromSuccess("设置成功");
    }

    /// <inheritdoc/>
    public Task<MqttDiscardedStatsResponse> GetMqttDiscardedStats(LoginedRequest request)
    {
        var (total, queueFull, expired, publishFailed, lastResetAt) = this.m_mqttHostService.GetDiscardedStats();
        var response = new MqttDiscardedStatsResponse
        {
            Total = total,
            QueueFull = queueFull,
            Expired = expired,
            PublishFailed = publishFailed,
            LastResetAt = lastResetAt
        };
        return Task.FromResult(response.FromSuccess("获取成功"));
    }

    /// <inheritdoc/>
    public Task<ResponseBase> ResetMqttDiscardedStats(LoginedRequest request)
    {
        this.m_mqttHostService.ResetDiscardedStats();
        return Task.FromResult(new ResponseBase().FromSuccess("重置成功"));
    }

    /// <inheritdoc/>
    public Task<MqttDashboardStatsResponse> GetMqttDashboardStats(LoginedRequest request)
    {
        var stats = this.m_mqttHostService.GetDashboardStats();
        return Task.FromResult(stats.FromSuccess("获取成功"));
    }

    /// <inheritdoc/>
    public async Task<MqttSslCertListResponse> GetMqttSslCerts(LoginedRequest request)
    {
        var certs = await this.m_mqttHostService.GetSslCertsAsync();
        var response = new MqttSslCertListResponse { Certs = certs };
        return response.FromSuccess("获取成功");
    }

    /// <inheritdoc/>
    public async Task<MqttSslCertResponse> AddMqttSslCert(AddMqttSslCertRequest request)
    {
        var response = new MqttSslCertResponse();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return response.FromFail("证书名称不能为空");
        }

        if (string.IsNullOrWhiteSpace(request.CertData))
        {
            return response.FromFail("证书数据不能为空");
        }

        if (request.Type == MqttSslCertType.Pem && string.IsNullOrWhiteSpace(request.KeyData))
        {
            return response.FromFail("PEM 类型证书必须提供私鑰数据");
        }

        System.Security.Cryptography.X509Certificates.X509Certificate2 cert;
        try
        {
            if (request.Type == MqttSslCertType.Pfx)
            {
                var certBytes = Convert.FromBase64String(request.CertData);
                var password = string.IsNullOrEmpty(request.Password) ? null : request.Password;
                cert = System.Security.Cryptography.X509Certificates.X509CertificateLoader.LoadPkcs12(certBytes, password);
            }
            else
            {
                cert = System.Security.Cryptography.X509Certificates.X509Certificate2.CreateFromPem(request.CertData, request.KeyData);
            }
        }
        catch (Exception ex)
        {
            return response.FromFail($"证书解析失败：{ex.Message}");
        }

        var info = new MqttSslCertInfo
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Type = request.Type,
            Subject = cert.Subject,
            Thumbprint = cert.Thumbprint,
            NotBefore = cert.NotBefore.ToUniversalTime(),
            NotAfter = cert.NotAfter.ToUniversalTime(),
            CreatedAt = DateTime.UtcNow
        };

        var data = new MqttSslCertData
        {
            Type = request.Type,
            CertData = request.CertData,
            KeyData = request.KeyData,
            Password = request.Password
        };

        var success = await this.m_mqttHostService.AddSslCertAsync(info, data);
        if (!success)
        {
            return response.FromFail("证书添加失败");
        }

        response.Cert = info;
        return response.FromSuccess("添加成功");
    }

    /// <inheritdoc/>
    public async Task<ResponseBase> DeleteMqttSslCert(MqttSslCertIdRequest request)
    {
        var response = new ResponseBase();

        if (string.IsNullOrWhiteSpace(request.CertId))
        {
            return response.FromFail("证书 ID 不能为空");
        }

        var success = await this.m_mqttHostService.DeleteSslCertAsync(request.CertId);
        return success
            ? response.FromSuccess("删除成功")
            : response.FromFail("证书不存在");
    }

    /// <inheritdoc/>
    public async Task<MqttDashboardStatsHistoryResponse> GetDashboardStatsHistory(GetDashboardStatsHistoryRequest request)
    {
        var response = new MqttDashboardStatsHistoryResponse();

        var now = DateTime.UtcNow;
        DateTime from;

        // 根据粒度确定查询起始时间
        switch (request.Granularity?.ToLowerInvariant())
        {
            case "hour":
                from = now.AddHours(-24);
                break;
            case "day":
                from = now.AddDays(-30);
                break;
            case "all":
                from = now.AddDays(-365);
                break;
            default: // minute
                from = now.AddHours(-1);
                break;
        }

        var raw = await this.m_mqttHostService.QueryStatsHistoryAsync(from, now);

        List<MqttStatsSnapshot> snapshots;
        switch (request.Granularity?.ToLowerInvariant())
        {
            case "hour":
                // 按小时分组，取每个小时最后一条
                snapshots = raw
                    .GroupBy(s => new DateTime(s.SnapshotAt.Year, s.SnapshotAt.Month, s.SnapshotAt.Day, s.SnapshotAt.Hour, 0, 0, DateTimeKind.Utc))
                    .Select(g => g.OrderBy(s => s.SnapshotAt).Last())
                    .OrderBy(s => s.SnapshotAt)
                    .ToList();
                break;
            case "day":
            case "all":
                // 按天分组，取每天最后一条
                snapshots = raw
                    .GroupBy(s => new DateTime(s.SnapshotAt.Year, s.SnapshotAt.Month, s.SnapshotAt.Day, 0, 0, 0, DateTimeKind.Utc))
                    .Select(g => g.OrderBy(s => s.SnapshotAt).Last())
                    .OrderBy(s => s.SnapshotAt)
                    .ToList();
                break;
            default: // minute - 原始数据，每分钟一条
                snapshots = raw;
                break;
        }

        response.Snapshots = snapshots;
        return response.FromSuccess("获取成功");
    }
}
