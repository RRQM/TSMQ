using System.Runtime.CompilerServices;
using TouchSocket.Mqtt;

namespace TSMQServer.Core.Imp.Plugins;

/// <summary>适配 TouchSocket 4.3 的受保护协议版本设置器，兼容 Native AOT。</summary>
internal static class MqttProtocolAccess
{
    /// <summary>为自定义传输设置协议版本。</summary>
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_Version")]
    internal static extern void SetVersion(MqttMessage message, MqttProtocolVersion version);

    // 管理端强制撤销时不再发送旧策略下的遗嘱；普通掉线仍由 Broker 处理遗嘱。
    internal static void ClearWill(IMqttSession session)
    {
        var actor = session switch
        {
            MqttTcpSessionClient tcp => tcp.MqttActor as MqttSessionActor,
            AuthorizedMqttWebSocketSessionClient ws => ws.Actor,
            _ => null
        };
        if (actor != null) WillMessage(actor) = null;
    }

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "m_mqttWillMessage")]
    private static extern ref MqttPublishMessage? WillMessage(MqttSessionActor actor);
}
