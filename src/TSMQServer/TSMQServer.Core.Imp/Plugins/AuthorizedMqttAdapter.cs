//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using TouchSocket.Core;
using TouchSocket.Mqtt;

namespace TSMQServer.Core.Imp.Plugins;

internal class AuthorizedMqttAdapter : CustomDataHandlingAdapter<MqttMessage>
{
    public AuthorizedMqttAdapter()
    {
    }

    public MqttProtocolVersion Version { get; set; } = MqttProtocolVersion.Unknown;

    protected override FilterResult Filter<TReader>(ref TReader reader, bool beCached, ref MqttMessage request)
    {
        if (reader.BytesRemaining < 2)
        {
            return FilterResult.Cache;
        }
        var position = reader.BytesRead;
        var firstByte = ReaderExtension.ReadValue<TReader, byte>(ref reader);
        var mqttControlPacketType = (MqttMessageType)firstByte.GetHeight4();

        var remainingLength = 0;
        var multiplier = 1;
        for (var i = 0; ; i++)
        {
            if (reader.BytesRemaining == 0)
            {
                reader.BytesRead = position;
                return FilterResult.Cache;
            }
            var value = ReaderExtension.ReadValue<TReader, byte>(ref reader);
            remainingLength += (value & 127) * multiplier;
            if ((value & 128) == 0) break;
            if (i == 3) throw new InvalidDataException("MQTT 剩余长度编码无效");
            multiplier *= 128;
        }
        if (reader.BytesRemaining < remainingLength)
        {
            reader.BytesRead = position;
            return FilterResult.Cache;
        }

        var mqttMessage = MqttMessage.CreateMqttMessage(mqttControlPacketType);

        //Console.WriteLine("Rev:"+ mqttMessage.MessageType);

        if (mqttMessage.MessageType != MqttMessageType.Connect)
        {
            MqttProtocolAccess.SetVersion(mqttMessage, this.Version);
        }

        reader.BytesRead = position;
        mqttMessage.Unpack(ref reader);
        if (reader.BytesRead != position + remainingLength + 1 + MqttExtension.GetVariableByteIntegerCount((int)remainingLength))
        {
            throw new Exception("存在没有读取的数据");
        }

        if (mqttMessage.MessageType == MqttMessageType.Connect)
        {
            this.Version = mqttMessage.Version;
        }

        request = mqttMessage;
        return FilterResult.Success;
    }
}
