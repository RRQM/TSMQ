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

using Microsoft.Extensions.DependencyInjection;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Sockets;
using TouchSocket.WebApi;
using TSMQServer.Core.Clients;
using TSMQServer.Core.Json;

namespace TSMQClient.Remote.Module;

internal static class IServiceCollectionExtension
{
    public static void UseDmtpRemote(this IServiceCollection services)
    {
        services.AddClientHostedService<IDmtpClient, HttpDmtpClient>(config =>
        {
            config.SetRemoteIPHost("tcp://127.0.0.1:7789");
            config.SetDmtpOption(options =>
            {
                options.VerifyToken = "Dmtp";
            });

            //配置容器
            config.ConfigureContainer(a =>
            {
                //注入日志组件
                a.AddAspNetCoreLogger();
            });

            //配置插件
            config.ConfigurePlugins(a =>
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

                a.UseReconnection<IDmtpClient>(options =>
                {
                    options.UseDmtpCheckAction();
                });
            });
        });

        services.AddSingleton<IRemoteService, DmtpRemoteService>();
    }

    public static void UseWebApiRemote(this IServiceCollection services)
    {
        services.AddClientHostedService<IWebApiClient, WebApiClient>(config =>
        {
            config.SetRemoteIPHost("http://127.0.0.1:7789");
            //配置容器
            config.ConfigureContainer(a =>
            {
                //注入日志组件
                a.AddAspNetCoreLogger();
            });

            //配置插件
            config.ConfigurePlugins(a =>
            {
                a.UseReconnection<IWebApiClient>();
            });
        });

        services.AddSingleton<IRemoteService, WebApiRemoteService>();
    }
}
