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
using TSMQServer.Core.Imp.Attributes;
using TouchSocket.Rpc;
using TSMQServer.Core;
using TSMQServer.Core.Imp.Services;

namespace TSMQServer.Core.Imp.Services;

/// <summary>
/// MQTT 主题管理服务实现。
/// </summary>
[Authorize]
[ExceptionFilter]
[LoggerFilter]
public partial class MqttTopicManagerService : TransientRpcServer, IMqttTopicManagerService
{
    private readonly ILogger<MqttTopicManagerService> m_logger;
    private readonly IMqttServerHostService m_mqttHostService;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public MqttTopicManagerService(ILogger<MqttTopicManagerService> logger, IMqttServerHostService mqttHostService)
    {
        this.m_logger = logger;
        this.m_mqttHostService = mqttHostService;
    }

    /// <inheritdoc/>
    public Task<MqttTopicListResponse> GetMqttTopicList(LoginedRequest request)
    {
        var response = new MqttTopicListResponse();
        response.Topics = [.. this.m_mqttHostService.GetTopicList()];
        return Task.FromResult(response.FromSuccess("获取成功"));
    }

    /// <inheritdoc/>
    public Task<MqttTopicStatsResponse> GetMqttTopicStats(GetMqttTopicStatsRequest request)
    {
        var response = new MqttTopicStatsResponse();
        response.Stats = [.. this.m_mqttHostService.GetTopicStats(request.Topics ?? [])];
        return Task.FromResult(response.FromSuccess("获取成功"));
    }

    /// <inheritdoc/>
    public Task<ResponseBase> ResetMqttTopicStats(ResetMqttTopicStatsRequest request)
    {
        this.m_mqttHostService.ResetTopicStats(request.Topics ?? []);
        return Task.FromResult(new ResponseBase().FromSuccess("重置成功"));
    }
}
