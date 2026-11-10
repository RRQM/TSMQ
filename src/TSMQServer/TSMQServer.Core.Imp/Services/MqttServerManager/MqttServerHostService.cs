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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Security.Cryptography.X509Certificates;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Mqtt;
using TouchSocket.Sockets;
using TSMQServer.Core;
using TSMQServer.Core.Imp.Plugins;
using TSMQServer.Db;

namespace TSMQServer.Core.Imp.Services;

/// <summary>
/// MQTT 服务宿主，管理一个共享 <see cref="MqttBroker"/>，
/// 所有 TCP 和 WebSocket 类型的监听均通过该 Broker 路由消息。
/// </summary>
public sealed class MqttServerHostService : IHostedService, IMqttServerHostService
{
    private readonly MqttAccessManager m_access;
    private readonly ConcurrentDictionary<string, MqttServerConfig> m_configs = new();
    private readonly MqttMessageDiscardPlugin m_discardPlugin;
    private readonly HttpService m_httpService = new();
    private readonly SemaphoreSlim m_lock = new(1, 1);
    private readonly ILogger<MqttServerHostService> m_logger;
    private readonly IMqttServerFromDB m_mqttServerFromDB;
    private readonly MqttServerPlugin m_plugin;
    private readonly MqttBroker m_sharedBroker = new();
    private readonly IMqttSslCertFromDB m_sslCertFromDB;
    private readonly IServiceScopeFactory m_scopeFactory;
    private readonly DateTime m_startedAt = DateTime.UtcNow;
    private readonly TSMQService m_tcpService;
    private readonly ConcurrentDictionary<string, TopicCounter> m_topicCounters = new();
    private Timer? m_snapshotTimer;
    private long m_totalBytesForwarded;
    private long m_totalBytesReceived;
    private long m_totalForwarded;
    private long m_totalReceived;

    /// <summary>
    /// <inheritdoc cref="MqttServerHostService"/>
    /// </summary>
    public MqttServerHostService(ILogger<MqttServerHostService> logger, IMqttServerFromDB mqttServerFromDB, MqttAccessManager access, IMqttSslCertFromDB sslCertFromDB, IServiceScopeFactory scopeFactory, MqttDiagnostics diagnostics)
    {
        this.m_tcpService = new TSMQService(this.m_sharedBroker, access);
        this.m_discardPlugin = new MqttMessageDiscardPlugin(diagnostics);
        this.m_logger = logger;
        this.m_mqttServerFromDB = mqttServerFromDB;
        this.m_access = access;
        this.m_sslCertFromDB = sslCertFromDB;
        this.m_scopeFactory = scopeFactory;
        this.m_plugin = new MqttServerPlugin(access, this.TrackInflow);
    }

    /// <inheritdoc/>
    public async Task<MqttServerInfo?> AddTcpServerAsync(string host, int port, string? sslCertId = null)
    {
        await this.m_lock.WaitAsync();
        try
        {
            if (this.IsPortInUse(port, MqttServerType.Tcp))
            {
                return null;
            }

            var config = new MqttServerConfig
            {
                Id = Guid.NewGuid().ToString(),
                Type = MqttServerType.Tcp,
                Host = host,
                Port = port,
                Path = string.Empty,
                IsRunning = false,
                CreatedAt = DateTime.UtcNow,
                SslCertId = sslCertId
            };

            if (sslCertId != null)
            {
                config.SslCertName = await this.m_sslCertFromDB.GetNameAsync(sslCertId);
            }

            this.m_configs[config.Id] = config;
            await this.StartListenerAsync(config);
            await this.m_mqttServerFromDB.AddAsync(config.ToInfo());
            return config.ToInfo();
        }
        finally
        {
            this.m_lock.Release();
        }
    }

    /// <inheritdoc/>
    public async Task<MqttServerInfo?> AddWebSocketServerAsync(string host, int port, string path, string? sslCertId = null)
    {
        await this.m_lock.WaitAsync();
        try
        {
            if (this.IsPortInUse(port, MqttServerType.WebSocket))
            {
                return null;
            }

            var config = new MqttServerConfig
            {
                Id = Guid.NewGuid().ToString(),
                Type = MqttServerType.WebSocket,
                Host = host,
                Port = port,
                Path = string.IsNullOrWhiteSpace(path) ? "/mqtt" : path,
                IsRunning = false,
                CreatedAt = DateTime.UtcNow,
                SslCertId = sslCertId
            };

            if (sslCertId != null)
            {
                config.SslCertName = await this.m_sslCertFromDB.GetNameAsync(sslCertId);
            }

            this.m_configs[config.Id] = config;
            await this.StartListenerAsync(config);
            await this.m_mqttServerFromDB.AddAsync(config.ToInfo());
            return config.ToInfo();
        }
        finally
        {
            this.m_lock.Release();
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<MqttClientInfo> GetConnectedClients()
    {
        List<MqttClientInfo> clients = new();
        foreach (var item in this.m_tcpService.Clients)
        {
            //tcp都是mqtt
            if (item.TryGetValue(MqttServerPlugin.SessionDataProperty, out var s))
            {
                if (s is null)
                {
                    continue;
                }
                var mqttClientInfo = new MqttClientInfo
                {
                    ClientId = s.ClientId,
                    UserName = s.UserName,
                    Status = "在线",
                    IpAddress = s.IpAddress,
                    KeepAlive = s.KeepAlive,
                    CleanStart = s.CleanStart,
                    SessionExpiryInterval = s.SessionExpiryInterval,
                    ConnectedAt = s.ConnectedAt
                };
                clients.Add(mqttClientInfo);
            }
        }
        var mqttProtocol = new Protocol(MqttUtility.MqttProtocolName);
        foreach (var item in m_httpService.Clients)
        {
            if (item.Protocol != mqttProtocol)
            {
                continue;
            }

            if (item.TryGetValue(MqttServerPlugin.SessionDataProperty, out var s))
            {
                if (s is null)
                {
                    continue;
                }
                var mqttClientInfo = new MqttClientInfo
                {
                    ClientId = s.ClientId,
                    UserName = s.UserName,
                    Status = "在线",
                    IpAddress = s.IpAddress,
                    KeepAlive = s.KeepAlive,
                    CleanStart = s.CleanStart,
                    SessionExpiryInterval = s.SessionExpiryInterval,
                    ConnectedAt = s.ConnectedAt
                };
                clients.Add(mqttClientInfo);
            }
        }

        return clients;
    }

    /// <inheritdoc/>
    public IReadOnlyList<MqttServerInfo> GetServers()
    {
        return this.m_configs.Values.Select(c => c.ToInfo()).ToList();
    }

    /// <inheritdoc/>
    public IReadOnlyList<MqttTopicInfo> GetTopicList()
    {
        var topics = this.m_sharedBroker.GetAllTopics();

        var topicInfos = new List<MqttTopicInfo>();

        foreach (var item in topics)
        {
            var mqttTopicInfo = new MqttTopicInfo();
            var subscriptions = this.m_sharedBroker.GetSubscribers(item);
            mqttTopicInfo.Topic = item;
            mqttTopicInfo.SubscriberCount = subscriptions.Count;
            topicInfos.Add(mqttTopicInfo);
        }

        return topicInfos;
    }

    /// <inheritdoc/>
    public IReadOnlyList<MqttTopicStats> GetTopicStats(IEnumerable<string> topics)
    {
        var topicSet = new HashSet<string>(topics ?? []);
        var result = new List<MqttTopicStats>();
        var entries = topicSet.Count == 0
            ? this.m_topicCounters
            : this.m_topicCounters.Where(kv => topicSet.Contains(kv.Key));
        foreach (var (topic, counter) in entries)
        {
            var subscriberCount = this.m_sharedBroker.GetSubscribers(topic).Count;
            result.Add(new MqttTopicStats
            {
                Topic = topic,
                SubscriberCount = subscriberCount,
                InflowCount = Interlocked.Read(ref counter.Inflow),
                OutflowCount = Interlocked.Read(ref counter.Outflow),
                SnapshotAt = DateTime.UtcNow
            });
        }
        return result;
    }

    /// <inheritdoc/>
    public async Task<int> KickClientsAsync(IEnumerable<string> clientIds)
    {
        var count = 0;
        foreach (var clientId in clientIds)
        {
            try
            {
                var kicked = false;
                if (this.m_tcpService.TryGetClient(clientId, out var tcpClient))
                {
                    await tcpClient.CloseAsync("管理员踢出");
                    kicked = true;
                }

                if (this.m_httpService.TryGetClient(clientId, out var wsClient))
                {
                    await wsClient.CloseAsync("管理员踢出");
                    kicked = true;
                }
                if (kicked) count++;
            }
            catch
            {
                // 忽略踢出时的异常
            }
        }
        return count;
    }

    /// <inheritdoc/>
    public async Task<bool> RemoveServerAsync(string serverId)
    {
        await this.m_lock.WaitAsync();
        try
        {
            if (!this.m_configs.TryRemove(serverId, out var config))
            {
                return false;
            }

            if (config.IsRunning)
            {
                this.StopListener(config);
            }

            await this.m_mqttServerFromDB.DeleteAsync(config.Id);
            return true;
        }
        finally
        {
            this.m_lock.Release();
        }
    }

    /// <inheritdoc/>
    public void ResetTopicStats(IEnumerable<string> topics)
    {
        var topicSet = new HashSet<string>(topics ?? []);
        if (topicSet.Count == 0)
        {
            foreach (var counter in this.m_topicCounters.Values)
            {
                Interlocked.Exchange(ref counter.Inflow, 0);
                Interlocked.Exchange(ref counter.Outflow, 0);
            }
        }
        else
        {
            foreach (var topic in topicSet)
            {
                if (this.m_topicCounters.TryGetValue(topic, out var counter))
                {
                    Interlocked.Exchange(ref counter.Inflow, 0);
                    Interlocked.Exchange(ref counter.Outflow, 0);
                }
            }
        }
    }

    /// <inheritdoc/>
    public Task<int> PublishMessageAsync(string topic, ReadOnlyMemory<byte> payload, int qos, bool retain) =>
        this.m_sharedBroker.ForwardMessageAsync(new MqttArrivedMessage(topic, (QosLevel)qos, retain, payload)).AsTask();

    /// <inheritdoc/>
    public (int MessageCapacity, int MessageExpirySeconds, int SessionExpirySeconds) GetBrokerSettings()
    {
        return (
            this.m_sharedBroker.MessageCapacity,
            (int)this.m_sharedBroker.MessageExpiry.TotalSeconds,
            (int)this.m_sharedBroker.SessionExpiry.TotalSeconds
        );
    }

    /// <inheritdoc/>
    public async Task SetBrokerSettings(int messageCapacity, int messageExpirySeconds, int sessionExpirySeconds)
    {
        this.m_sharedBroker.MessageCapacity = messageCapacity;
        this.m_sharedBroker.MessageExpiry = TimeSpan.FromSeconds(messageExpirySeconds);
        this.m_sharedBroker.SessionExpiry = TimeSpan.FromSeconds(sessionExpirySeconds);

        using var scope = this.m_scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IBrokerSettingsFromDB>();
        await db.SaveAsync(messageCapacity, messageExpirySeconds, sessionExpirySeconds).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public (long Total, long QueueFull, long Expired, long PublishFailed, DateTime LastResetAt) GetDiscardedStats()
    {
        return this.m_discardPlugin.GetStats();
    }

    /// <inheritdoc/>
    public void ResetDiscardedStats()
    {
        this.m_discardPlugin.Reset();
    }

    /// <inheritdoc/>
    public MqttDashboardStatsResponse GetDashboardStats()
    {
        var (discardTotal, discardQueueFull, discardExpired, discardPublishFailed, _) = this.m_discardPlugin.GetStats();
        return new MqttDashboardStatsResponse
        {
            OnlineClients = this.GetConnectedClients().Count,
            ActiveTopics = this.m_sharedBroker.GetAllTopics().Count,
            RunningServers = this.m_configs.Values.Count(c => c.IsRunning),
            TotalReceived = Interlocked.Read(ref this.m_totalReceived),
            TotalForwarded = Interlocked.Read(ref this.m_totalForwarded),
            TotalBytesReceived = Interlocked.Read(ref this.m_totalBytesReceived),
            TotalBytesForwarded = Interlocked.Read(ref this.m_totalBytesForwarded),
            DiscardedTotal = discardTotal,
            DiscardedQueueFull = discardQueueFull,
            DiscardedExpired = discardExpired,
            DiscardedPublishFailed = discardPublishFailed,
            ServerStartedAt = this.m_startedAt,
            SnapshotAt = DateTime.UtcNow
        };
    }

    /// <inheritdoc/>
    public async Task<List<MqttStatsSnapshot>> QueryStatsHistoryAsync(DateTime from, DateTime to)
    {
        using var scope = this.m_scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IMqttStatsSnapshotFromDB>();
        return await db.QueryRangeAsync(from, to).ConfigureAwait(false);
    }

    private async void OnSnapshotTimerCallback(object? state)
    {
        try
        {
            var snapshot = this.BuildSnapshot();
            using var scope = this.m_scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IMqttStatsSnapshotFromDB>();
            await db.InsertAsync(snapshot).ConfigureAwait(false);
            // 清理 365 天前的数据
            await db.DeleteBeforeAsync(DateTime.UtcNow.AddDays(-365)).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            this.m_logger.LogWarning(ex, "写入统计快照失败");
        }
    }

    private MqttStatsSnapshot BuildSnapshot()
    {
        var (discardTotal, discardQueueFull, discardExpired, discardPublishFailed, _) = this.m_discardPlugin.GetStats();
        return new MqttStatsSnapshot
        {
            SnapshotAt = DateTime.UtcNow,
            OnlineClients = this.GetConnectedClients().Count,
            ActiveTopics = this.m_sharedBroker.GetAllTopics().Count,
            RunningServers = this.m_configs.Values.Count(c => c.IsRunning),
            TotalReceived = Interlocked.Read(ref this.m_totalReceived),
            TotalForwarded = Interlocked.Read(ref this.m_totalForwarded),
            TotalBytesReceived = Interlocked.Read(ref this.m_totalBytesReceived),
            TotalBytesForwarded = Interlocked.Read(ref this.m_totalBytesForwarded),
            DiscardedTotal = discardTotal,
            DiscardedQueueFull = discardQueueFull,
            DiscardedExpired = discardExpired,
            DiscardedPublishFailed = discardPublishFailed
        };
    }

    /// <inheritdoc/>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await this.m_access.InitializeAsync(this.m_sharedBroker);

        await this.m_tcpService.SetupAsync(new TouchSocketConfig()
            .ConfigurePlugins(a =>
            {
                a.Add(this.m_plugin);
                a.Add(this.m_discardPlugin);
            }));

        await this.m_tcpService.StartAsync();

        await this.m_httpService.SetupAsync(new TouchSocketConfig()
            .ConfigureContainer(a =>
            {
                a.RegisterSingleton<IMqttWebSocketService>(new SharedBrokerMqttWebSocketService(this.m_sharedBroker));
            })
            .ConfigurePlugins(a =>
            {
                a.Add(new AuthorizedMqttWebSocketFeature(this.m_sharedBroker, this.m_access,
                    (client, context) => this.m_configs.Values.Any(c => c.IsRunning && c.Type == MqttServerType.WebSocket
                        && c.Port == client.ServicePort && context.Request.UrlEquals(c.Path))));
                a.Add(this.m_plugin);
                a.Add(this.m_discardPlugin);
            }));

        await this.m_httpService.StartAsync();

        var persistedServers = await this.m_mqttServerFromDB.GetAllAsync();
        foreach (var server in persistedServers)
        {
            var config = MqttServerConfig.FromInfo(server);
            if (config.SslCertId != null)
            {
                config.SslCertName = await this.m_sslCertFromDB.GetNameAsync(config.SslCertId);
            }
            this.m_configs[config.Id] = config;

            if (config.IsRunning)
            {
                try
                {
                    await this.StartListenerAsync(config);
                }
                catch (Exception ex)
                {
                    config.IsRunning = false;
                    await this.m_mqttServerFromDB.UpdateAsync(config.ToInfo());
                    this.m_logger.LogError(ex, "恢复 MQTT 服务器失败，ID：{Id}", config.Id);
                }
            }
        }

        // 从数据库恢复 Broker 参数设置
        {
            using var scope = this.m_scopeFactory.CreateScope();
            var brokerDb = scope.ServiceProvider.GetRequiredService<IBrokerSettingsFromDB>();
            var (capacity, msgExpiry, sessionExpiry) = await brokerDb.GetAsync().ConfigureAwait(false);
            this.m_sharedBroker.MessageCapacity = capacity;
            this.m_sharedBroker.MessageExpiry = TimeSpan.FromSeconds(msgExpiry);
            this.m_sharedBroker.SessionExpiry = TimeSpan.FromSeconds(sessionExpiry);
            this.m_logger.LogInformation(
                "已从数据库恢复 Broker 参数：MessageCapacity={C}, MessageExpiry={M}s, SessionExpiry={S}s",
                capacity, msgExpiry, sessionExpiry);
        }

        this.m_logger.LogInformation("MQTT 服务宿主已启动，共享 Broker 已就绪。");

        // 每分钟写入一次统计快照，并清理 365 天前的旧数据
        var interval = TimeSpan.FromMinutes(1);
        this.m_snapshotTimer = new Timer(this.OnSnapshotTimerCallback, null, interval, interval);
    }

    /// <inheritdoc/>
    public async Task<bool> StartServerAsync(string serverId)
    {
        await this.m_lock.WaitAsync();
        try
        {
            if (!this.m_configs.TryGetValue(serverId, out var config))
            {
                return false;
            }

            if (config.IsRunning)
            {
                return true;
            }

            await this.StartListenerAsync(config);
            await this.m_mqttServerFromDB.UpdateAsync(config.ToInfo());
            return true;
        }
        finally
        {
            this.m_lock.Release();
        }
    }

    /// <inheritdoc/>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (this.m_snapshotTimer != null)
        {
            await this.m_snapshotTimer.DisposeAsync();
            this.m_snapshotTimer = null;
        }
        await this.m_tcpService.StopAsync(cancellationToken);
        await this.m_httpService.StopAsync(cancellationToken);
        this.m_logger.LogInformation("MQTT 服务宿主已停止。");
    }

    /// <inheritdoc/>
    public async Task<bool> StopServerAsync(string serverId)
    {
        await this.m_lock.WaitAsync();
        try
        {
            if (!this.m_configs.TryGetValue(serverId, out var config))
            {
                return false;
            }

            if (!config.IsRunning)
            {
                return true;
            }

            this.StopListener(config);
            await this.m_mqttServerFromDB.UpdateAsync(config.ToInfo());
            return true;
        }
        finally
        {
            this.m_lock.Release();
        }
    }

    /// <inheritdoc/>
    public Task<List<MqttSslCertInfo>> GetSslCertsAsync()
    {
        return this.m_sslCertFromDB.GetAllAsync();
    }

    /// <inheritdoc/>
    public Task<bool> AddSslCertAsync(MqttSslCertInfo info, MqttSslCertData data)
    {
        return this.m_sslCertFromDB.AddAsync(info, data);
    }

    /// <inheritdoc/>
    public Task<bool> DeleteSslCertAsync(string certId)
    {
        return this.m_sslCertFromDB.DeleteAsync(certId);
    }

    private bool IsPortInUse(int port, MqttServerType type)
    {
        return this.m_configs.Values.Any(c => c.Type == type && c.Port == port);
    }

    private Task StartListenerAsync(MqttServerConfig config)
    {
        ServiceSslOption? sslOption = null;
        if (!string.IsNullOrEmpty(config.SslCertId))
        {
            var certData = this.m_sslCertFromDB.GetCertDataAsync(config.SslCertId).GetAwaiter().GetResult()
                ?? throw new InvalidOperationException("监听引用的 SSL 证书不存在，拒绝启动未加密监听");
            if (certData != null)
            {
                var certificate = LoadX509Certificate(certData);
                sslOption = new ServiceSslOption { Certificate = certificate };
            }
        }

        var ipHost = new IPHost($"{config.Host}:{config.Port}");
        var option = new TcpListenOption
        {
            IpHost = ipHost,
            Name = config.Id,
            ServiceSslOption = sslOption
        };

        if (config.Type == MqttServerType.Tcp)
        {
            this.m_tcpService.AddListen(option);
        }
        else
        {
            this.m_httpService.AddListen(option);
        }

        config.IsRunning = true;
        this.m_logger.LogInformation(
            "MQTT {Type} 服务器已启动，地址：{Host}:{Port}，ID：{Id}",
            config.Type, config.Host, config.Port, config.Id);

        return Task.CompletedTask;
    }

    private static X509Certificate LoadX509Certificate(MqttSslCertData data)
    {
        if (data.Type == MqttSslCertType.Pfx)
        {
            var certBytes = Convert.FromBase64String(data.CertData);
            var password = string.IsNullOrEmpty(data.Password) ? null : data.Password;
            return X509CertificateLoader.LoadPkcs12(certBytes, password);
        }
        else
        {
            return X509Certificate2.CreateFromPem(data.CertData, data.KeyData);
        }
    }

    private void StopListener(MqttServerConfig config)
    {
        var monitors = config.Type == MqttServerType.Tcp
            ? this.m_tcpService.Monitors
            : this.m_httpService.Monitors;

        var monitor = monitors.FirstOrDefault(m => m.Option.Name == config.Id);
        if (monitor != null)
        {
            if (config.Type == MqttServerType.Tcp)
            {
                this.m_tcpService.RemoveListen(monitor);
            }
            else
            {
                this.m_httpService.RemoveListen(monitor);
            }
        }

        config.IsRunning = false;
        this.m_logger.LogInformation(
            "MQTT {Type} 服务器已停止，ID：{Id}",
            config.Type, config.Id);
    }

    private void TrackInflow(string topic, long payloadBytes)
    {
        var counter = this.m_topicCounters.GetOrAdd(topic, _ => new TopicCounter());
        Interlocked.Increment(ref counter.Inflow);
        Interlocked.Increment(ref this.m_totalReceived);
        Interlocked.Add(ref this.m_totalBytesReceived, payloadBytes);
        var subscriberCount = this.m_sharedBroker.GetSubscribers(topic).Count;
        if (subscriberCount > 0)
        {
            Interlocked.Add(ref counter.Outflow, subscriberCount);
            Interlocked.Add(ref this.m_totalForwarded, subscriberCount);
            Interlocked.Add(ref this.m_totalBytesForwarded, payloadBytes * subscriberCount);
        }
    }
    internal sealed class SharedBrokerMqttWebSocketService : IMqttWebSocketService
    {
        /// <summary>
        /// 使用指定的 <see cref="MqttBroker"/> 初始化实例。
        /// </summary>
        /// <param name="mqttBroker">共享的 MQTT Broker。</param>
        public SharedBrokerMqttWebSocketService(MqttBroker mqttBroker)
        {
            this.MqttBroker = mqttBroker;
        }

        /// <inheritdoc/>
        public MqttBroker MqttBroker { get; }
    }

    private sealed class MqttServerConfig
    {
        public DateTime CreatedAt { get; set; }
        public string Host { get; set; } = "0.0.0.0";
        public string Id { get; set; } = string.Empty;
        public bool IsRunning { get; set; }
        public string Path { get; set; } = "/mqtt";
        public int Port { get; set; }
        public string? SslCertId { get; set; }
        public string? SslCertName { get; set; }
        public MqttServerType Type { get; set; }

        public static MqttServerConfig FromInfo(MqttServerInfo info)
        {
            return new MqttServerConfig
            {
                Id = info.Id,
                Type = info.Type,
                Host = info.Host,
                Port = info.Port,
                Path = info.Path,
                IsRunning = info.IsRunning,
                CreatedAt = info.CreatedAt,
                SslCertId = info.SslCertId
            };
        }

        public MqttServerInfo ToInfo() => new()
        {
            Id = this.Id,
            Type = this.Type,
            Host = this.Host,
            Port = this.Port,
            Path = this.Path,
            IsRunning = this.IsRunning,
            CreatedAt = this.CreatedAt,
            SslCertId = this.SslCertId,
            SslCertName = this.SslCertName
        };
    }

    private sealed class TopicCounter
    {
        public long Inflow;
        public long Outflow;
    }

    private class TSMQService : MqttTcpServiceBase
    {
        private readonly MqttAccessManager m_access;
        public TSMQService(MqttBroker mqttBroker, MqttAccessManager access)
        {
            this.MqttBroker = mqttBroker;
            this.m_access = access;
        }

        protected override MqttTcpSessionClient NewClient() => new AuthorizedMqttTcpSessionClient(this.MqttBroker, this.m_access);

        public override MqttBroker MqttBroker { get; }
    }
}
