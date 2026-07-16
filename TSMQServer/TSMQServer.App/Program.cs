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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using TSMQServer.App.Plugins;
using TSMQServer.Core.Imp;
using TSMQServer.Core.Imp.Extensions;
using TSMQServer.Web.Extensions;
using TSMQServer.Core.Json;
using TSMQServer.Db.Sqlite.Extensions;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Http;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace TSMQServer.App;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Environment.ContentRootPath = AppContext.BaseDirectory;
        builder.Services.UseSqliteDb();
        builder.Services.AddTSMQServerCoreImp(builder.Configuration);

        builder.Services.AddWindowsService(options =>
        {
            options.ServiceName = "TSMQService";
        });

        builder.Logging.ClearProviders();
        builder.Logging.AddNLog();

        var serverSettings = builder.Configuration.GetSection("Server").Get<ServerSettings>() ?? new ServerSettings();

        builder.Services.ConfigureContainer(a =>
        {
            a.AddAspNetCoreLogger();

            a.AddRpcStore(store =>
            {
                store.RegisterAllFromTSMQServer_Core_Imp();
            });
        });

        builder.Services.AddServiceHostedService<IHttpDmtpService, HttpDmtpService>(config =>
        {
            config.SetListenIPHosts(serverSettings.Port)
               .ConfigurePlugins(a =>
               {
                   a.UseTcpSessionCheckClear();

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

                   a.UseWebApi(options =>
                       {
                           options.ConfigureConverter(converter =>
                           {
                               converter.Clear();
                               converter.AddSystemTextJsonSerializerFormatter(options =>
                               {
                                   options.TypeInfoResolverChain.Insert(0, TSMQServerCoreJsonSerializerContext.Default);
                               });
                           });
                       });

                   //添加WebSocket功能
                   a.UseWebSocket(options =>
                   {
                       options.SetVerifyConnection((sessionClient, context) =>
                       {
                           var request = context.Request;

                           if (request.UrlEquals("/wsjsonrpc"))
                           {
                               return true;
                           }
                           return false;
                       });

                       options.SetAutoPong(true);//当收到ping报文时自动回应pong
                   });

                   a.UseWebSocketJsonRpc(options =>
                   {
                       options.SerializerOptions.TypeInfoResolverChain.Insert(0, TSMQServerCoreJsonSerializerContext.Default);

                       options.SetAllowJsonRpc((socketClient, context) =>
                       {
                           var request = context.Request;

                           if (request.UrlEquals("/wsjsonrpc"))
                           {
                               return true;
                           }
                           return false;
                       });
                   });

                   a.Add<DmtpPlugin>();
                   a.UseWebUI();
               })
               .SetDmtpOption(options =>
               {
                   options.VerifyToken = serverSettings.DmtpVerifyToken;
               });
        });

        var host = builder.Build();
        await host.RunAsync();
    }
}
