using System.Net;
using System.Net.Sockets;
using MQTTnet.Server;
using TouchSocket.Core;
using TouchSocket.Mqtt;
using TouchSocket.Sockets;

namespace BenchmarkServers;

/// <summary>
/// 被测 Broker 实现。
/// </summary>
public enum BrokerKind
{
    /// <summary>
    /// TouchSocket Broker。
    /// </summary>
    TouchSocket,

    /// <summary>
    /// MQTTnet Broker。
    /// </summary>
    Mqttnet
}

/// <summary>
/// 为每个基准用例启动独立的回环 TCP Broker。
/// </summary>
public sealed class BrokerHost : IAsyncDisposable
{
    private MqttServer? m_mqttnet;
    private MqttTcpService? m_touchSocket;

    /// <summary>
    /// 实际使用的 TCP 端口。
    /// </summary>
    public int Port { get; private set; }

    /// <summary>
    /// 启动指定的 Broker。
    /// </summary>
    public async Task StartAsync(BrokerKind kind)
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        this.Port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        try
        {
            if (kind == BrokerKind.Mqttnet)
            {
                this.m_mqttnet = new MqttServerFactory().CreateMqttServer(new MqttServerOptionsBuilder()
                    .WithDefaultEndpoint()
                    .WithDefaultEndpointBoundIPAddress(IPAddress.Loopback)
                    .WithDefaultEndpointPort(this.Port)
                    .Build());
                await this.m_mqttnet.StartAsync();
            }
            else
            {
                this.m_touchSocket = new MqttTcpService();
                await this.m_touchSocket.SetupAsync(new TouchSocketConfig()
                    .SetListenIPHosts($"127.0.0.1:{this.Port}"));
                await this.m_touchSocket.StartAsync();
            }
        }
        catch
        {
            await this.DisposeAsync();
            throw;
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        try
        {
            if (this.m_mqttnet != null)
            {
                try
                {
                    await this.m_mqttnet.StopAsync();
                }
                finally
                {
                    this.m_mqttnet.Dispose();
                    this.m_mqttnet = null;
                }
            }
        }
        finally
        {
            if (this.m_touchSocket != null)
            {
                try
                {
                    await this.m_touchSocket.StopAsync();
                }
                finally
                {
                    this.m_touchSocket.Dispose();
                    this.m_touchSocket = null;
                }
            }
        }
    }
}
