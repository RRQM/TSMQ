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
using TouchSocket.Rpc;
using TSMQServer.Core.Imp.Attributes;

namespace TSMQServer.Core.Imp.Services;

/// <summary>
/// MQTT 客户端管理服务实现。
/// </summary>
[Authorize]
[ExceptionFilter]
[LoggerFilter]
public partial class MqttClientManagerService : TransientRpcServer, IMqttClientManagerService
{
    private readonly ILogger<MqttClientManagerService> m_logger;
    private readonly IMqttServerHostService m_mqttHostService;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public MqttClientManagerService(ILogger<MqttClientManagerService> logger, IMqttServerHostService mqttHostService)
    {
        this.m_logger = logger;
        this.m_mqttHostService = mqttHostService;
    }

    /// <inheritdoc/>
    public Task<MqttClientListResponse> GetMqttClientList(LoginedRequest request)
    {
        var response = new MqttClientListResponse();
        var clients = this.m_mqttHostService.GetConnectedClients();
        response.Clients = [.. clients];
        return Task.FromResult(response.FromSuccess("获取成功"));
    }

    /// <inheritdoc/>
    public async Task<ResponseBase> KickMqttClients(KickMqttClientsRequest request)
    {
        var response = new ResponseBase();
        var count = await this.m_mqttHostService.KickClientsAsync(request.ClientIds);
        return response.FromSuccess($"已踢除 {count} 个客户端");
    }
}
