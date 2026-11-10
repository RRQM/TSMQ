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

using System.ComponentModel;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.JsonRpc;
using TouchSocket.Rpc;
using TouchSocket.WebApi;

namespace TSMQServer.Core;

/// <summary>
/// MQTT 主题管理服务接口。
/// </summary>
[GeneratorRpcProxy]
public interface IMqttTopicManagerService : ITransientRpcServer
{
    /// <summary>
    /// 获取当前所有已订阅的 MQTT 主题列表。
    /// </summary>
    [Description("获取MQTT主题列表")]
    [WebApi(Method = HttpMethodType.Post)]
    [DmtpRpc]
    [JsonRpc(MethodInvoke = true)]
    Task<MqttTopicListResponse> GetMqttTopicList(LoginedRequest request);

    /// <summary>
    /// 获取指定主题的消息统计信息；<paramref name="request"/> 的 <see cref="GetMqttTopicStatsRequest.Topics"/> 为空时返回所有主题的统计。
    /// </summary>
    [Description("获取MQTT主题统计")]
    [WebApi(Method = HttpMethodType.Post)]
    [DmtpRpc]
    [JsonRpc(MethodInvoke = true)]
    Task<MqttTopicStatsResponse> GetMqttTopicStats(GetMqttTopicStatsRequest request);

    /// <summary>
    /// 重置指定主题的消息统计计数；<paramref name="request"/> 的 <see cref="ResetMqttTopicStatsRequest.Topics"/> 为空时重置所有主题。
    /// </summary>
    [Description("重置MQTT主题统计")]
    [WebApi(Method = HttpMethodType.Post)]
    [DmtpRpc]
    [JsonRpc(MethodInvoke = true)]
    Task<ResponseBase> ResetMqttTopicStats(ResetMqttTopicStatsRequest request);
}
