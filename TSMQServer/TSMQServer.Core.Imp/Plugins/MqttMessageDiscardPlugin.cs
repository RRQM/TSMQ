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

using TouchSocket.Core;
using TouchSocket.Mqtt;

namespace TSMQServer.Core.Imp.Plugins;

/// <summary>
/// MQTT 消息丢弃统计插件，实现 <see cref="IMqttMessageDiscardedPlugin"/>，统计各类丢弃原因的消息数量。
/// </summary>
internal class MqttMessageDiscardPlugin : PluginBase, IMqttMessageDiscardedPlugin
{
    private long m_expired;
    private DateTime m_lastResetAt = DateTime.UtcNow;
    private long m_publishFailed;
    private long m_queueFull;
    private long m_total;

    /// <summary>
    /// 获取当前丢弃统计快照。
    /// </summary>
    public (long Total, long QueueFull, long Expired, long PublishFailed, DateTime LastResetAt) GetStats()
    {
        return (
            Interlocked.Read(ref this.m_total),
            Interlocked.Read(ref this.m_queueFull),
            Interlocked.Read(ref this.m_expired),
            Interlocked.Read(ref this.m_publishFailed),
            this.m_lastResetAt
        );
    }

    /// <inheritdoc/>
    public Task OnMqttMessageDiscarded(IMqttSession client, MqttMessageDiscardedEventArgs e)
    {
        Interlocked.Increment(ref this.m_total);
        switch (e.Reason)
        {
            case DiscardReason.QueueFull:
                Interlocked.Increment(ref this.m_queueFull);
                break;
            case DiscardReason.Expired:
                Interlocked.Increment(ref this.m_expired);
                break;
            case DiscardReason.PublishFailed:
                Interlocked.Increment(ref this.m_publishFailed);
                break;
        }
        return e.InvokeNext();
    }

    /// <summary>
    /// 重置所有丢弃计数。
    /// </summary>
    public void Reset()
    {
        Interlocked.Exchange(ref this.m_total, 0);
        Interlocked.Exchange(ref this.m_queueFull, 0);
        Interlocked.Exchange(ref this.m_expired, 0);
        Interlocked.Exchange(ref this.m_publishFailed, 0);
        this.m_lastResetAt = DateTime.UtcNow;
    }
}
