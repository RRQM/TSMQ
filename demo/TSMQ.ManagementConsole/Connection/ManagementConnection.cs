using TouchSocket.Core;
using TouchSocket.WebApi;
using TSMQServer.Core;
using TSMQServer.Core.Clients;

namespace TSMQ.ManagementConsole.Connection;

internal sealed class ManagementConnection : IAsyncDisposable
{
    private readonly WebApiClient m_client = new();

    /// <summary>创建基于 WebApi 的管理连接。</summary>
    public ManagementConnection()
    {
        this.Service = new WebApiRemoteService(this.m_client);
    }

    /// <summary>获取统一管理接口。</summary>
    public IRemoteService Service { get; }

    /// <summary>连接管理服务并登录，返回是否登录成功。</summary>
    public async Task<bool> ConnectAsync(string address, string account, string password)
    {
        await this.m_client.SetupAsync(new TouchSocketConfig().SetRemoteIPHost(address));
        await this.m_client.ConnectAsync(default);
        var result = await this.Service.LoginAsync(account, password);
        if (result.IsSuccess)
        {
            return true;
        }

        Console.Error.WriteLine($"登录失败：{result.Message}");
        return false;
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        try
        {
            if (!string.IsNullOrEmpty(this.Service.LoginedToken))
            {
                var result = await this.Service.LogoutAsync(new LoginedRequest());
                if (!result.IsSuccess)
                {
                    Console.Error.WriteLine($"注销失败：{result.Message}");
                }
            }
        }
        finally
        {
            this.m_client.Dispose();
        }
    }
}
