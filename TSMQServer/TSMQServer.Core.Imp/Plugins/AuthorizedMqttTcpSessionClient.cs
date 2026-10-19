using TouchSocket.Core;
using TouchSocket.Mqtt;
using TouchSocket.Sockets;
using TSMQServer.Core.Imp.Services;

namespace TSMQServer.Core.Imp.Plugins;

/// <summary>在 Broker 处理前执行访问控制的 TCP 会话。</summary>
internal sealed class AuthorizedMqttTcpSessionClient(MqttBroker broker, MqttAccessManager access) : MqttTcpSessionClient(broker)
{
    /// <inheritdoc/>
    protected override async Task OnTcpReceived(ReceivedDataEventArgs e)
    {
        if (e.RequestInfo is MqttMessage message)
            await access.ProcessAsync(this, message, this.SendMqttAsync, () => base.OnTcpReceived(e));
        else await base.OnTcpReceived(e);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpClosed(ClosedEventArgs e)
    {
        try { await base.OnTcpClosed(e); }
        finally { access.Closed(this, e.Message); }
    }

    private async Task SendMqttAsync(MqttMessage message)
    {
        var locker = this.Transport.WriteLocker;
        await locker.WaitAsync();
        try
        {
            var writer = new PipeBytesWriter(this.Transport.Writer);
            message.Build(ref writer);
            await writer.FlushAsync(CancellationToken.None);
        }
        finally { locker.Release(); }
    }
}
