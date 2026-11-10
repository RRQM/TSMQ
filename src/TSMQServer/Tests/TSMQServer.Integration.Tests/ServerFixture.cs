using System.Net;
using System.Net.Sockets;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Rpc;
using TouchSocket.Sockets;
using TSMQServer.Core.Imp;
using TSMQServer.Core.Imp.Extensions;
using TSMQServer.Core.Imp.Services;
using TSMQServer.Core.Json;
using TSMQServer.Db.Sqlite.Extensions;
using HttpClient = System.Net.Http.HttpClient;

namespace TSMQServer.Integration.Tests;

public sealed class ServerFixture : IAsyncLifetime
{
    private readonly string m_directory = Path.Combine(Path.GetTempPath(), "tsmq-tests-" + Guid.NewGuid().ToString("N"));
    private IHost m_host = null!;
    public HttpClient Client { get; private set; } = null!;
    public string AdminToken { get; private set; } = "";
    public int MqttPort { get; private set; }
    public int WsPort { get; private set; }
    public IServiceProvider Services => this.m_host.Services;

    public async ValueTask InitializeAsync()
    {
        Directory.CreateDirectory(this.m_directory);
        var builder = Host.CreateApplicationBuilder();
        builder.Logging.ClearProviders();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:SecretKey"] = "TSMQ-integration-test-signing-key-12345678901234567890",
            ["Jwt:Issuer"] = "TSMQ-tests", ["Jwt:Audience"] = "TSMQ-tests", ["Jwt:ExpirationMinutes"] = "60"
        });
        builder.Services.UseSqliteDb(Path.Combine(this.m_directory, "test.db"));
        builder.Services.AddTSMQServerCoreImp(builder.Configuration, this.m_directory);
        builder.Services.ConfigureContainer(c =>
        {
            c.AddAspNetCoreLogger();
            c.AddRpcStore(store => store.RegisterAllFromTSMQServer_Core_Imp());
        });
        var port = FreePort();
        builder.Services.AddServiceHostedService<IHttpService, HttpService>(config => config.SetListenIPHosts(port).ConfigurePlugins(p =>
        {
            p.UseWebApi(options => options.ConfigureConverter(converter =>
            {
                converter.Clear();
                converter.AddSystemTextJsonSerializerFormatter(json => json.TypeInfoResolverChain.Insert(0, TSMQServerCoreJsonSerializerContext.Default));
            }));
        }));
        this.m_host = builder.Build();
        await this.m_host.StartAsync();
        this.Client = new HttpClient { BaseAddress = new Uri($"http://127.0.0.1:{port}") };
        var login = await this.Post("IUserManagerService/Login", new { Account = "admin", Password = "admin123" });
        Assert.Equal("Success", login["Code"]?.GetValue<string>());
        this.AdminToken = login["LoginedToken"]!.GetValue<string>();
        this.MqttPort = FreePort();
        this.WsPort = FreePort();
        await this.Post("IMqttServerManagerService/AddMqttAuthUser", new { LoginedToken = this.AdminToken, UserName = "device", Password = "secret" });
        foreach (var listener in new[] { ("Tcp", this.MqttPort), ("WebSocket", this.WsPort) })
        {
            var result = await this.Post("IMqttServerManagerService/AddMqttServer", new { LoginedToken = this.AdminToken, Type = listener.Item1, Host = "127.0.0.1", Port = listener.Item2, Path = "/mqtt" });
            Assert.Equal("Success", result["Code"]!.GetValue<string>());
            var started = await this.Post("IMqttServerManagerService/StartMqttServer", new { LoginedToken = this.AdminToken, ServerId = result["Server"]!["Id"]!.GetValue<string>() });
            Assert.Equal("Success", started["Code"]!.GetValue<string>());
        }
    }

    public async Task<JsonObject> Post(string path, object body)
    {
        var response = await this.Client.PostAsJsonAsync(path, body, new System.Text.Json.JsonSerializerOptions());
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonObject>())!;
    }

    public async ValueTask DisposeAsync()
    {
        this.Client?.Dispose();
        if (this.m_host != null) { await this.m_host.StopAsync(); this.m_host.Dispose(); }
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        try { Directory.Delete(this.m_directory, true); }
        catch (IOException) { /* TORM 可能仍持有 SQLite 连接，保留独立临时目录供系统清理。 */ }
    }

    private static int FreePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }
}
