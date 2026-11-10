using BenchmarkDotNet.Attributes;
using BenchmarkServers;
using MQTTnet;
using MQTTnet.Protocol;
using TouchSocket.Mqtt;

namespace BenchmarkConsoleApp;

/// <summary>
/// 使用统一的 MQTTnet 客户端测量 Broker 的端到端消息转发性能。
/// </summary>
[MemoryDiagnoser]
public class BenchmarkBroker
{
    private const int MessageCount = 1000;
    private const string Topic = "benchmark/broker";
    private readonly BrokerHost m_broker = new();
    private MQTTnet.IMqttClient? m_publisher;
    private MQTTnet.IMqttClient? m_subscriber;
    private MqttApplicationMessage m_message = null!;
    private TaskCompletionSource m_received = null!;
    private byte[] m_payload = [];
    private int m_count;
    private bool m_invalidMessage;

    /// <summary>
    /// 被测 Broker。
    /// </summary>
    [Params(BrokerKind.TouchSocket, BrokerKind.Mqttnet)]
    public BrokerKind Broker;

    /// <summary>
    /// 消息负载的字节数。
    /// </summary>
    [Params(1024, 4096, 8192)]
    public int DataSize;

    /// <summary>
    /// 发布与订阅使用的服务质量等级。
    /// </summary>
    [Params(MqttQualityOfServiceLevel.AtMostOnce, MqttQualityOfServiceLevel.AtLeastOnce, MqttQualityOfServiceLevel.ExactlyOnce)]
    public MqttQualityOfServiceLevel Qos;

    /// <summary>
    /// MQTT 协议版本。
    /// </summary>
    [Params(MqttProtocolVersion.V311, MqttProtocolVersion.V500)]
    public MqttProtocolVersion Version;

    /// <summary>
    /// 启动 Broker 并建立发布与订阅连接。
    /// </summary>
    [GlobalSetup]
    public async Task SetupAsync()
    {
        try
        {
            await this.m_broker.StartAsync(this.Broker);
            this.m_payload = new byte[this.DataSize];
            Random.Shared.NextBytes(this.m_payload);
            this.m_message = new MqttApplicationMessageBuilder()
                .WithTopic(Topic)
                .WithPayload(this.m_payload)
                .WithQualityOfServiceLevel(this.Qos)
                .Build();
            var factory = new MqttClientFactory();
            this.m_subscriber = factory.CreateMqttClient();
            this.m_subscriber.ApplicationMessageReceivedAsync += this.OnMessageReceivedAsync;
            this.m_publisher = factory.CreateMqttClient();
            await this.m_subscriber.ConnectAsync(this.CreateOptions());
            var result = await this.m_subscriber.SubscribeAsync(new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter(filter => filter.WithTopic(Topic).WithQualityOfServiceLevel(this.Qos))
                .Build());
            if (result.Items.Any(item => (int)item.ResultCode != (int)this.Qos))
            {
                throw new InvalidOperationException("Broker 未授予请求的订阅 QoS。");
            }
            await this.m_publisher.ConnectAsync(this.CreateOptions());
        }
        catch
        {
            await this.CleanupAsync();
            throw;
        }
    }

    /// <summary>
    /// 发布并等待订阅者收到全部 1000 条消息。
    /// </summary>
    [Benchmark]
    public async Task ForwardMessages()
    {
        this.m_count = 0;
        this.m_invalidMessage = false;
        this.m_received = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        for (var i = 0; i < MessageCount; i++)
        {
            await this.m_publisher!.PublishAsync(this.m_message);
        }
        await this.m_received.Task.WaitAsync(TimeSpan.FromSeconds(30));
        if (this.m_invalidMessage || this.m_count != MessageCount)
        {
            throw new InvalidOperationException("Broker 转发消息的数量、主题、QoS 或负载校验失败。");
        }
    }

    /// <summary>
    /// 释放连接和 Broker。
    /// </summary>
    [GlobalCleanup]
    public async Task CleanupAsync()
    {
        try
        {
            await DisconnectAsync(this.m_publisher);
        }
        finally
        {
            this.m_publisher = null;
            try
            {
                await DisconnectAsync(this.m_subscriber);
            }
            finally
            {
                this.m_subscriber = null;
                await this.m_broker.DisposeAsync();
            }
        }
    }

    private static async Task DisconnectAsync(MQTTnet.IMqttClient? client)
    {
        if (client == null)
        {
            return;
        }
        try
        {
            if (client.IsConnected)
            {
                await client.DisconnectAsync();
            }
        }
        finally
        {
            client.Dispose();
        }
    }

    private MqttClientOptions CreateOptions()
    {
        return new MqttClientOptionsBuilder()
            .WithTcpServer("127.0.0.1", this.m_broker.Port)
            .WithProtocolVersion(this.Version == MqttProtocolVersion.V500
                ? MQTTnet.Formatter.MqttProtocolVersion.V500
                : MQTTnet.Formatter.MqttProtocolVersion.V311)
            .WithClientId($"BrokerBenchmark_{Guid.NewGuid():N}")
            .WithCleanSession()
            .Build();
    }

    private bool PayloadMatches(System.Buffers.ReadOnlySequence<byte> payload)
    {
        if (payload.Length != this.m_payload.Length)
        {
            return false;
        }
        var offset = 0;
        foreach (var segment in payload)
        {
            if (!segment.Span.SequenceEqual(this.m_payload.AsSpan(offset, segment.Length)))
            {
                return false;
            }
            offset += segment.Length;
        }
        return true;
    }

    private Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        var message = args.ApplicationMessage;
        if (message.Topic != Topic || message.QualityOfServiceLevel != this.Qos
            || !this.PayloadMatches(message.Payload))
        {
            this.m_invalidMessage = true;
        }
        if (Interlocked.Increment(ref this.m_count) == MessageCount)
        {
            this.m_received.TrySetResult();
        }
        return Task.CompletedTask;
    }
}
