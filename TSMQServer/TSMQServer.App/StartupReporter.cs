using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;
using TouchSocket.Dmtp;
using TSMQServer.Core;
using TSMQServer.Core.Imp.Services;

namespace TSMQServer.App;

internal sealed class StartupReporter(
    IHostApplicationLifetime lifetime,
    IHostEnvironment environment,
    IHttpDmtpService managementService,
    IMqttServerHostService mqttService,
    ILogger<StartupReporter> logger) : IHostedService, IDisposable
{
    private readonly Stopwatch m_stopwatch = Stopwatch.StartNew();
    private CancellationTokenRegistration m_started;
    private CancellationTokenRegistration m_stopping;
    private CancellationTokenRegistration m_stopped;

    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        this.m_started = lifetime.ApplicationStarted.Register(this.Report);
        this.m_stopping = lifetime.ApplicationStopping.Register(() =>
            logger.LogInformation("正在停止 TSMQ Server，关闭监听器并释放连接…"));
        this.m_stopped = lifetime.ApplicationStopped.Register(() =>
            logger.LogInformation("TSMQ Server 已停止。"));
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.m_started.Dispose();
        this.m_stopping.Dispose();
        this.m_stopped.Dispose();
    }

    private void Report()
    {
        this.m_stopwatch.Stop();
        logger.LogInformation("────────────────────────────────────────────────────────────");
        logger.LogInformation("TSMQ Server {Version} 已就绪，启动耗时 {Elapsed:F0} ms",
            typeof(Program).Assembly.GetName().Version?.ToString(3), this.m_stopwatch.Elapsed.TotalMilliseconds);

        var addresses = GetLocalAddresses();
        foreach (var monitor in managementService.Monitors)
        {
            if (monitor.Socket.LocalEndPoint is not IPEndPoint endpoint)
            {
                continue;
            }

            var scheme = monitor.Option.UseSsl ? "https" : "http";
            logger.LogInformation("管理监听      {Address}（Web / HTTP API / DMTP）", endpoint);
            if (endpoint.Address.Equals(IPAddress.Any) || endpoint.Address.Equals(IPAddress.IPv6Any))
            {
                var loopback = endpoint.AddressFamily == AddressFamily.InterNetwork ? IPAddress.Loopback : IPAddress.IPv6Loopback;
                logger.LogInformation("Web 本机      {Url}", FormatUrl(scheme, loopback.ToString(), endpoint.Port, "/"));
                foreach (var address in addresses.Where(address => address.AddressFamily == endpoint.AddressFamily
                    || endpoint.AddressFamily == AddressFamily.InterNetworkV6 && monitor.Socket.DualMode))
                {
                    logger.LogInformation("Web 网卡      {Url}", FormatUrl(scheme, address.ToString(), endpoint.Port, "/"));
                }
            }
            else
            {
                logger.LogInformation("Web 访问      {Url}", FormatUrl(scheme, endpoint.Address.ToString(), endpoint.Port, "/"));
            }
        }

        var servers = mqttService.GetServers().OrderBy(server => server.Type).ThenBy(server => server.Port).ToList();
        logger.LogInformation("MQTT 监听器   已配置 {Total} 个，运行中 {Running} 个", servers.Count, servers.Count(server => server.IsRunning));
        foreach (var server in servers)
        {
            var encrypted = !string.IsNullOrEmpty(server.SslCertId);
            var scheme = server.Type == MqttServerType.Tcp ? encrypted ? "mqtts" : "mqtt" : encrypted ? "wss" : "ws";
            var path = server.Type == MqttServerType.WebSocket ? "/" + server.Path.TrimStart('/') : "";
            logger.LogInformation("MQTT {Transport}  [{State}] {Endpoint}",
                server.Type == MqttServerType.Tcp ? "TCP" : "WebSocket",
                server.IsRunning ? "运行中" : "已停止",
                FormatUrl(scheme, server.Host, server.Port, path));
        }
        if (servers.Count == 0)
        {
            logger.LogInformation("尚未配置 MQTT 监听器，可在 Web 控制台的“MQTT服务器”页面添加。");
        }
        logger.LogInformation("0.0.0.0 / [::] 表示监听所有网卡，客户端请使用具体 IP 或域名连接。");
        logger.LogInformation("配置目录      {Directory}", environment.ContentRootPath);
        logger.LogInformation("数据库        {Path}", Path.GetFullPath("TSMQ.db"));
        logger.LogInformation("日志目录      {Directory}（控制台 Info+，文件 Debug+）", Path.Combine(AppContext.BaseDirectory, "logs"));
        if (!WindowsServiceHelpers.IsWindowsService())
        {
            logger.LogInformation("按 Ctrl+C 安全停止服务。");
        }
        logger.LogInformation("────────────────────────────────────────────────────────────");
    }

    private static string FormatUrl(string scheme, string host, int port, string path)
    {
        var authority = host.Contains(':') ? $"[{host.Trim('[', ']')}]" : host;
        return $"{scheme}://{authority}:{port}{path}";
    }

    private static IReadOnlyList<IPAddress> GetLocalAddresses()
    {
        try
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(network => network.OperationalStatus == OperationalStatus.Up
                    && network.NetworkInterfaceType is not NetworkInterfaceType.Loopback and not NetworkInterfaceType.Tunnel)
                .SelectMany(network => network.GetIPProperties().UnicastAddresses)
                .Select(address => address.Address)
                .Where(address => !IPAddress.IsLoopback(address) && !address.IsIPv6LinkLocal
                    && !address.Equals(IPAddress.Any) && !address.Equals(IPAddress.IPv6Any))
                .Distinct().OrderBy(address => address.AddressFamily).ThenBy(address => address.ToString()).ToList();
        }
        catch (NetworkInformationException)
        {
            return [];
        }
        catch (PlatformNotSupportedException)
        {
            return [];
        }
    }
}
