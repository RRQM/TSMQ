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
using TouchSocket.Http;
using TouchSocket.Sockets;

namespace TSMQServer.Web.Plugins;

/// <summary>
/// 将 TSMQWeb 构建产物作为嵌入资源提供的 HTTP 静态页面插件，支持 SPA 路由回退。
/// 同时在 /tsmq-runtime-config.json 暴露当前托管地址供前端校验。
/// </summary>
internal sealed class TsmqWebPlugin : PluginBase, IServerStartedPlugin, IHttpPlugin
{
    private const string c_resourcePrefix = "TSMQWeb";

    // 缓存所有嵌入的静态文件：URL 路径 -> 内容
    private readonly Dictionary<string, ReadOnlyMemory<byte>> m_files
        = new(StringComparer.OrdinalIgnoreCase);

    // 运行时配置 JSON，在 OnServerStarted 后填充
    private ReadOnlyMemory<byte> m_runtimeConfig;

    /// <inheritdoc/>
    public async Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
    {
        var request = e.Context.Request;
        var response = e.Context.Response;
        var url = request.RelativeURL;

        // 1. 运行时配置接口
        if (url.Equals("/tsmq-runtime-config.json", StringComparison.OrdinalIgnoreCase))
        {
            e.Handled = true;
            response
                .SetStatusWithSuccess()
                .SetContentTypeByExtension(".json")
                .SetContent(this.m_runtimeConfig);
            await response.AnswerAsync().ConfigureDefaultAwait();
            return;
        }

        // 2. 精确匹配静态文件
        if (this.m_files.TryGetValue(url, out var entry))
        {
            e.Handled = true;
            response
                .SetStatusWithSuccess()
                .SetContentTypeByExtension(Path.GetExtension(url))
                .SetContent(entry);
            await response.AnswerAsync().ConfigureDefaultAwait();
            return;
        }

        // 3. SPA 路由回退：无扩展名的路径（如 /dashboard、/login）均返回 index.html
        if (url == "/" || !Path.HasExtension(url))
        {
            if (this.m_files.TryGetValue("/index.html", out var indexEntry))
            {
                e.Handled = true;
                response
                    .SetStatusWithSuccess()
                    .SetContentTypeByExtension(".html")
                    .SetContent(indexEntry);
                await response.AnswerAsync().ConfigureDefaultAwait();
                return;
            }
        }

        await e.InvokeNext().ConfigureDefaultAwait();
    }

    /// <inheritdoc/>
    public async Task OnServerStarted(IServiceBase sender, ServiceStateEventArgs e)
    {
        if (e.ServerState == ServerState.Running)
        {
            this.LoadEmbeddedResources();

            // 构建运行时配置：绑定 0.0.0.0/loopback 时用相对路径 "/"，让浏览器根据当前
            // origin 自动定位；否则使用服务器绑定的绝对地址。
            var monitor = ((ITcpServiceBase)sender).Monitors.FirstOrDefault();
            string runtimeConfigJson;
            if (monitor != null)
            {
                var iphost = monitor.Option.IpHost;
                var scheme = monitor.Option.UseSsl ? "https" : "http";
                string apiBaseUrl;
                if (iphost.IsLoopback
                    || iphost.DnsSafeHost == "0.0.0.0"
                    || iphost.DnsSafeHost == "::")
                {
                    apiBaseUrl = "/";
                }
                else
                {
                    apiBaseUrl = $"{scheme}://{iphost.DnsSafeHost}:{iphost.Port}";
                }

                runtimeConfigJson = $"{{\"apiBaseUrl\":\"{apiBaseUrl}\"}}";
            }
            else
            {
                runtimeConfigJson = "{\"apiBaseUrl\":\"/\"}";
            }

            this.m_runtimeConfig = runtimeConfigJson.ToUtf8Bytes();
        }

        await e.InvokeNext().ConfigureDefaultAwait();
    }

    private void LoadEmbeddedResources()
    {
        var assembly = typeof(TsmqWebPlugin).Assembly;

        // LogicalName 格式（Windows）：TSMQWeb/assets\chunk-abc.js（RecursiveDir 含反斜杠）
        // 统一归一化为正斜杠后再去前缀得到 URL 路径。
        foreach (var name in assembly.GetManifestResourceNames())
        {
            var normalized = name.Replace('\\', '/');
            if (!normalized.StartsWith(c_resourcePrefix + "/", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var urlPath = normalized.Substring(c_resourcePrefix.Length); // 已以 "/" 开头

            using var stream = assembly.GetManifestResourceStream(name)!;
            ReadOnlyMemory<byte> bytes = stream.ReadAllToByteArray();
            this.m_files[urlPath] = bytes;
        }
    }
}
