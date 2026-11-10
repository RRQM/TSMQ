// ------------------------------------------------------------------------------
// 姝や唬鐮佺増鏉冿紙闄ょ壒鍒０鏄庢垨鍦╔REF缁撳熬鐨勫懡鍚嶇┖闂寸殑浠ｇ爜锛夊綊浣滆€呮湰浜鸿嫢姹濇鑼楁墍鏈?
// 婧愪唬鐮佷娇鐢ㄥ崗璁伒寰湰浠撳簱鐨勫紑婧愬崗璁強闄勫姞鍗忚锛岃嫢鏈粨搴撴病鏈夎缃紝鍒欐寜MIT寮€婧愬崗璁巿鏉?
// CSDN鍗氬锛歨ttps://blog.csdn.net/qq_40374647
// 鍝斿摡鍝斿摡瑙嗛锛歨ttps://space.bilibili.com/94253567
// Gitee婧愪唬鐮佷粨搴擄細https://gitee.com/RRQM_Home
// Github婧愪唬鐮佷粨搴擄細https://github.com/RRQM
// API棣栭〉锛歨ttps://touchsocket.net/
// 浜ゆ祦QQ缇わ細234762506
// 鎰熻阿鎮ㄧ殑涓嬭浇鍜屼娇鐢?
// ------------------------------------------------------------------------------

using TouchSocket.Core;
using TouchSocket.JsonRpc;
using TSMQServer.Core.Clients;

namespace TSMQClient.Remote.Module.Tests;

[Collection("RemoteServiceTests")]
public class JsonRpcRemoteServiceTests : RemoteServiceTestsBase
{
    private WebSocketJsonRpcClient m_client = null!;
    private JsonRpcRemoteService m_service = null!;

    protected override IRemoteService Service => this.m_service;

    protected override async ValueTask InitializeServiceAsync()
    {
        this.m_client = new WebSocketJsonRpcClient();
        await this.m_client.SetupAsync(new TouchSocketConfig()
            .SetRemoteIPHost("ws://127.0.0.1:7789/wsjsonrpc"));
        await this.m_client.ConnectAsync(default);
        this.m_service = new JsonRpcRemoteService(this.m_client);
    }

    protected override async ValueTask DisposeServiceAsync()
    {
        this.m_client.Dispose();
        await ValueTask.CompletedTask;
    }
}
