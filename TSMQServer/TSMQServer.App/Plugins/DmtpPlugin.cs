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
using TouchSocket.Dmtp;
using TouchSocket.Sockets;

namespace TSMQServer.App.Plugins;

internal class DmtpPlugin : PluginBase, IDmtpConnectedPlugin, IDmtpClosedPlugin
{
    private readonly ILogger<DmtpPlugin> m_logger;

    public DmtpPlugin(ILogger<DmtpPlugin> logger)
    {
        this.m_logger = logger;
    }
    public async Task OnDmtpClosed(IDmtpActorObject client, ClosedEventArgs e)
    {
        this.m_logger.LogInformation($"客户端{client.DmtpActor.Id}已断开，原因：{e.Message}");
        await e.InvokeNext();
    }

    public async Task OnDmtpConnected(IDmtpActorObject client, DmtpVerifyEventArgs e)
    {
        this.m_logger.LogInformation($"客户端{client.DmtpActor.Id}已连接。");
        await e.InvokeNext();
    }
}
