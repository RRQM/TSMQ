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
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Sockets;
using TSMQServer.Core.Clients;
using TSMQServer.Core.Json;

namespace TSMQClient.Remote.Module.Tests;

[Collection("RemoteServiceTests")]
public class DmtpRemoteServiceTests : RemoteServiceTestsBase
{
    private HttpDmtpClient m_client = null!;
    private DmtpRemoteService m_service = null!;

    protected override IRemoteService Service => this.m_service;

    protected override async ValueTask InitializeServiceAsync()
    {
        this.m_client = new HttpDmtpClient();
        await this.m_client.SetupAsync(new TouchSocketConfig()
            .SetRemoteIPHost("tcp://127.0.0.1:7789")
            .SetDmtpOption(options =>
            {
                options.VerifyToken = "Dmtp";
            })
            .ConfigurePlugins(a =>
            {
                a.UseDmtpRpc(options =>
                {
                    options.ConfigureDefaultSerializationSelector(selector =>
                    {
                        selector.UseSystemTextJson(options =>
                        {
                            options.TypeInfoResolverChain.Insert(0, TSMQServerCoreJsonSerializerContext.Default);
                        });
                    });
                });
            }));
        await this.m_client.ConnectAsync();
        this.m_service = new DmtpRemoteService(this.m_client);
    }

    protected override async ValueTask DisposeServiceAsync()
    {
        await this.m_client.CloseAsync();
        this.m_client.Dispose();
    }
}
