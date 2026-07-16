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
using TSMQServer.Core.Imp.Services;
using TSMQServer.Core.Imp.Services.Jwt;
namespace TSMQServer.Core.Imp.Extensions;

public static class TSMQServerCoreImpExtension
{
    /// <summary>
    /// 注册 TSMQServer Core 服务，JWT 和日志等配置从 <see cref="IConfiguration"/> 读取。
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">应用配置</param>
    /// <param name="logDirectory">日志目录，为 <see langword="null"/> 时使用默认路径</param>
    public static void AddTSMQServerCoreImp(this IServiceCollection services, IConfiguration configuration, string? logDirectory = null)
    {
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        services.AddSingleton<IJwtService, JwtService>();

        services.Configure<LogFileOptions>(options =>
        {
            options.LogDirectory = logDirectory ?? Path.Combine(AppContext.BaseDirectory, "logs");
        });
        services.AddSingleton<NLogFileReader>();

        services.AddSingleton<MqttServerHostService>();
        services.AddSingleton<IMqttServerHostService>(sp => sp.GetRequiredService<MqttServerHostService>());
        services.AddHostedService(sp => sp.GetRequiredService<MqttServerHostService>());
    }
}
