using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;

namespace TSMQServer.Integration.Tests;

internal sealed class MqttWire : IAsyncDisposable
{
    private TcpClient? m_tcp;
    private ClientWebSocket? m_ws;

    public static async Task<MqttWire> Open(int port, bool webSocket = false)
    {
        var wire = new MqttWire();
        if (webSocket)
        {
            wire.m_ws = new ClientWebSocket();
            wire.m_ws.Options.AddSubProtocol("mqtt");
            await wire.m_ws.ConnectAsync(new Uri($"ws://127.0.0.1:{port}/mqtt"), CancellationToken.None);
        }
        else { wire.m_tcp = new TcpClient(); await wire.m_tcp.ConnectAsync("127.0.0.1", port); }
        return wire;
    }

    public async Task<byte[]> Connect(string id, string user = "device", string password = "secret", bool v5 = false, string? willTopic = null, bool splitHeader = false)
    {
        var body = new List<byte>();
        body.AddRange(Utf8("MQTT")); body.Add(v5 ? (byte)5 : (byte)4);
        body.Add((byte)(0xc2 | (willTopic == null ? 0 : 4))); body.AddRange([0, 60]);
        if (v5) body.Add(0);
        body.AddRange(Utf8(id));
        if (willTopic != null) { if (v5) body.Add(0); body.AddRange(Utf8(willTopic)); body.AddRange(Utf8("will")); }
        body.AddRange(Utf8(user)); body.AddRange(Utf8(password));
        var packet = Packet(0x10, body.ToArray());
        if (splitHeader)
        {
            await this.Send(packet[..2]);
            // 模拟多字节剩余长度字段跨 WebSocket 消息到达。
            await Task.Delay(50);
            await this.Send(packet[2..]);
        }
        else await this.Send(packet);
        return await this.Receive();
    }

    public async Task<byte[]> Subscribe(string topic, bool v5 = false)
    {
        var body = new List<byte> { 0, 1 };
        if (v5) body.Add(0);
        body.AddRange(Utf8(topic)); body.Add(0);
        await this.Send(Packet(0x82, body.ToArray()));
        return await this.Receive();
    }

    public Task Publish(string topic, byte[] payload) => this.Send(Packet(0x30, [.. Utf8(topic), .. payload]));

    public async Task Send(byte[] packet)
    {
        if (this.m_ws != null) await this.m_ws.SendAsync(packet, WebSocketMessageType.Binary, true, CancellationToken.None);
        else await this.m_tcp!.GetStream().WriteAsync(packet);
    }

    public async Task<byte[]> Receive(int timeout = 3000)
    {
        using var cts = new CancellationTokenSource(timeout);
        if (this.m_ws != null)
        {
            var bytes = new byte[70000];
            var result = await this.m_ws.ReceiveAsync(bytes, cts.Token);
            return bytes[..result.Count];
        }
        var stream = this.m_tcp!.GetStream();
        var first = new byte[2]; await stream.ReadExactlyAsync(first, cts.Token);
        var resultBytes = new List<byte>(first);
        var length = first[1] & 127; var multiplier = 128; var last = first[1];
        while ((last & 128) != 0) { var next = new byte[1]; await stream.ReadExactlyAsync(next, cts.Token); last = next[0]; resultBytes.Add(last); length += (last & 127) * multiplier; multiplier *= 128; }
        var body = new byte[length]; await stream.ReadExactlyAsync(body, cts.Token); resultBytes.AddRange(body);
        return resultBytes.ToArray();
    }

    public ValueTask DisposeAsync() { this.m_tcp?.Dispose(); this.m_ws?.Abort(); this.m_ws?.Dispose(); return ValueTask.CompletedTask; }
    private static byte[] Utf8(string value) { var bytes = Encoding.UTF8.GetBytes(value); return [(byte)(bytes.Length >> 8), (byte)bytes.Length, .. bytes]; }
    private static byte[] Packet(byte first, byte[] body)
    {
        var header = new List<byte> { first }; var remaining = body.Length;
        do { var b = (byte)(remaining % 128); remaining /= 128; if (remaining > 0) b |= 128; header.Add(b); } while (remaining > 0);
        return [.. header, .. body];
    }
}
