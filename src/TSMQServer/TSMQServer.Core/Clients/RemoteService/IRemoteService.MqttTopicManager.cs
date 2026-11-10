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

namespace TSMQServer.Core.Clients;

public partial interface IRemoteService
{
    /// <summary>获取当前所有已订阅的 MQTT 主题列表</summary>
    Task<Result<List<MqttTopicInfo>>> GetMqttTopicListAsync(CancellationToken cancellationToken = default);

    /// <summary>获取指定主题的消息统计信息；topics 为空时返回所有主题的统计</summary>
    Task<Result<List<MqttTopicStats>>> GetMqttTopicStatsAsync(string[] topics, CancellationToken cancellationToken = default);

    /// <summary>重置指定主题的消息统计计数；topics 为空时重置所有主题</summary>
    Task<Result> ResetMqttTopicStatsAsync(string[] topics, CancellationToken cancellationToken = default);
}
