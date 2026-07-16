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
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Mqtt;
using TouchSocket.Sockets;
using TSMQServer.Db;

namespace TSMQServer.Core.Imp.Plugins;

/// <summary>
/// MQTT 连接事件插件，负责处理客户端连接、断开及消息接收的业务逻辑。
/// </summary>
internal class MqttServerPlugin : PluginBase,
    IMqttConnectingPlugin,
    IMqttConnectedPlugin,
    IMqttClosedPlugin,
    IMqttReceivedPlugin
{
    private static readonly DependencyProperty<MqttClientSessionData?> s_sessionDataProperty =
        new("MqttClientSessionData", default);

    private readonly IMqttConnectAuthFromDB m_authFromDB;
    private readonly Action<string, long> m_trackInflow;
    private volatile bool m_authEnabled;

    /// <summary>
    /// <inheritdoc cref="MqttServerPlugin"/>
    /// </summary>
    public MqttServerPlugin(IMqttConnectAuthFromDB authFromDB, Action<string, long> trackInflow)
    {
        this.m_authFromDB = authFromDB;
        this.m_trackInflow = trackInflow;
    }

    /// <summary>
    /// 获取存储客户端会话数据的依赖属性，供外部读取已连接客户端信息。
    /// </summary>
    public static DependencyProperty<MqttClientSessionData?> SessionDataProperty => s_sessionDataProperty;

    /// <summary>
    /// 设置连接授权开关。
    /// </summary>
    public void SetAuthEnabled(bool enabled)
    {
        this.m_authEnabled = enabled;
    }

    /// <inheritdoc/>
    public async Task OnMqttConnecting(IMqttSession client, MqttConnectingEventArgs e)
    {
        if (this.m_authEnabled)
        {
            var msg = e.ConnectMessage;
            var userName = msg.UserName ?? string.Empty;
            var password = msg.Password ?? string.Empty;
            var valid = await this.m_authFromDB.ValidateAsync(userName, password).ConfigureAwait(false);
            if (!valid)
            {
                e.ConnAckMessage.ReturnCode = MqttReasonCode.NotAuthorized;
                return;
            }
        }

        e.ConnAckMessage.ReturnCode = MqttReasonCode.ConnectionAccepted;
        await e.InvokeNext();
    }

    /// <inheritdoc/>
    public async Task OnMqttConnected(IMqttSession client, MqttConnectedEventArgs e)
    {
        var msg = e.ConnectMessage;
        var ip = client is IMqttTcpSessionClient tcpClient
            ? $"{tcpClient.IP}:{tcpClient.Port}"
            : client is IMqttWebSocketSessionClient wsClient
                ? $"{wsClient.Client.IP}:{wsClient.Client.Port}"
                : string.Empty;

        client.SetValue(s_sessionDataProperty, new MqttClientSessionData
        {
            ClientId = msg.ClientId,
            UserName = msg.UserName,
            IpAddress = ip,
            KeepAlive = msg.KeepAlive,
            CleanStart = msg.CleanSession,
            SessionExpiryInterval = msg.SessionExpiryInterval,
            ConnectedAt = DateTime.UtcNow,
            Session = client
        });

        await e.InvokeNext();
    }

    /// <inheritdoc/>
    public async Task OnMqttClosed(IMqttSession client, MqttClosedEventArgs e)
    {
        client.RemoveValue(s_sessionDataProperty);
        await e.InvokeNext();
    }

    /// <inheritdoc/>
    public async Task OnMqttReceived(IMqttSession client, MqttReceivedEventArgs e)
    {
        if (e.MqttMessage is MqttArrivedMessage arrivedMsg)
        {
            this.m_trackInflow(arrivedMsg.TopicName, arrivedMsg.Payload.Length);
        }

        await e.InvokeNext();
    }
}

/// <summary>
/// MQTT 客户端会话数据。
/// </summary>
internal sealed class MqttClientSessionData
{
    public bool CleanStart { get; init; }
    public string ClientId { get; init; } = string.Empty;
    public DateTime ConnectedAt { get; init; }
    public string IpAddress { get; init; } = string.Empty;
    public ushort KeepAlive { get; init; }
    public IMqttSession Session { get; init; } = null!;
    public uint SessionExpiryInterval { get; init; }
    public string? UserName { get; init; }
}
