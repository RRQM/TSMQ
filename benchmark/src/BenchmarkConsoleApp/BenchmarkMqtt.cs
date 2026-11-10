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

using BenchmarkServers;
using BenchmarkDotNet.Attributes;
using MQTTnet;
using MQTTnet.Protocol;
using TouchSocket.Core;
using TouchSocket.Mqtt;
using TouchSocket.Sockets;

namespace BenchmarkConsoleApp;

/// <summary>
/// 对比 MQTTnet 与 TouchSocket 的 MQTT 消息发布性能。
/// </summary>
[MemoryDiagnoser]
public class BenchmarkMqtt
{
    private const int MessageCount = 1000;
    private const string Topic = "benchmark/test";

    private readonly BrokerHost m_broker = new();
    private MQTTnet.IMqttClient? m_mqttnetClient;
    private byte[] m_testData = [];
    private MqttTcpClient? m_touchSocketClient;

    /// <summary>
    /// 承载客户端发布测试的 Broker。
    /// </summary>
    [Params(BrokerKind.TouchSocket, BrokerKind.Mqttnet)]
    public BrokerKind Broker;

    /// <summary>
    /// 消息负载的字节数。
    /// </summary>
    [Params(1024, 4096, 8192)]
    public int DataSize;

    /// <summary>
    /// 消息服务质量等级。
    /// </summary>
    [Params(MqttQualityOfServiceLevel.AtMostOnce, MqttQualityOfServiceLevel.AtLeastOnce, MqttQualityOfServiceLevel.ExactlyOnce)]
    public MqttQualityOfServiceLevel Qos;

    /// <summary>
    /// MQTT 协议版本。
    /// </summary>
    [Params(MqttProtocolVersion.V311, MqttProtocolVersion.V500)]
    public MqttProtocolVersion Version;

    /// <summary>
    /// 初始化消息负载及两个 MQTT 客户端。
    /// </summary>
    [GlobalSetup]
    public async Task SetupAsync()
    {
        this.m_testData = new byte[this.DataSize];
        Random.Shared.NextBytes(this.m_testData);

        await this.m_broker.StartAsync(this.Broker);
        const string host = "127.0.0.1";
        var port = this.m_broker.Port;

        try
        {
            this.m_mqttnetClient = new MqttClientFactory().CreateMqttClient();
            var optionsBuilder = new MqttClientOptionsBuilder()
                .WithTcpServer(host, port)
                .WithProtocolVersion(this.Version == MqttProtocolVersion.V500
                    ? MQTTnet.Formatter.MqttProtocolVersion.V500
                    : MQTTnet.Formatter.MqttProtocolVersion.V311)
                .WithClientId($"BenchmarkClient_{Guid.NewGuid():N}")
                .WithCleanSession();

            await this.m_mqttnetClient.ConnectAsync(optionsBuilder.Build());

            this.m_touchSocketClient = new MqttTcpClient();
            await this.m_touchSocketClient.SetupAsync(new TouchSocketConfig()
                .SetRemoteIPHost(new UriBuilder("tcp", host, port).Uri.AbsoluteUri)
                .SetMqttConnectOptions(options =>
                {
                    options.KeepAlive = 60;
                    options.ClientId = $"TouchSocketClient_{Guid.NewGuid():N}";
                    options.Version = this.Version;
                    options.CleanSession = true;
                }));
            await this.m_touchSocketClient.ConnectAsync(10000);
        }
        catch
        {
            await this.CleanupAsync();
            throw;
        }
    }

    /// <summary>
    /// 使用 MQTTnet 连续发布 1000 条消息。
    /// </summary>
    [Benchmark]
    public async Task MqttnetPublishMessages()
    {
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(this.GetTopic())
            .WithPayload(this.m_testData)
            .WithQualityOfServiceLevel(this.Qos)
            .WithRetainFlag(false)
            .Build();

        for (var i = 0; i < MessageCount; i++)
        {
            await this.m_mqttnetClient!.PublishAsync(message);
        }
    }

    /// <summary>
    /// 使用 TouchSocket 连续发布 1000 条消息。
    /// </summary>
    [Benchmark]
    public async Task TouchSocketPublishMessages()
    {
        var message = new MqttPublishMessage(
            this.GetTopic(),
            false,
            (QosLevel)this.Qos,
            this.m_testData);

        for (var i = 0; i < MessageCount; i++)
        {
            await this.m_touchSocketClient!.PublishAsync(message);
        }
    }

    /// <summary>
    /// 断开连接并释放客户端资源。
    /// </summary>
    [GlobalCleanup]
    public async Task CleanupAsync()
    {
        try
        {
            if (this.m_mqttnetClient?.IsConnected == true)
            {
                await this.m_mqttnetClient.DisconnectAsync();
            }
        }
        finally
        {
            this.m_mqttnetClient?.Dispose();
            this.m_mqttnetClient = null;

            try
            {
                if (this.m_touchSocketClient != null)
                {
                    await this.m_touchSocketClient.CloseAsync();
                }
            }
            finally
            {
                this.m_touchSocketClient?.Dispose();
                this.m_touchSocketClient = null;
                await this.m_broker.DisposeAsync();
            }
        }
    }

    private string GetTopic()
    {
        return $"{Topic}/{this.DataSize}bytes/{this.Version}/mqtt";
    }
}
