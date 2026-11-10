using System.Text.Json;
using TouchSocket.Core;
using TSMQServer.Core;
using TSMQServer.Core.Clients;

namespace TSMQ.ManagementConsole.Commands;

internal sealed class ManagementCommands(IRemoteService service)
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() { WriteIndented = true };

    /// <summary>运行交互式管理菜单。</summary>
    public async Task RunAsync()
    {
        while (true)
        {
            Console.WriteLine("\n1 统计概览  2 监听器列表  3 在线客户端  4 主题监控");
            Console.WriteLine("5 查看访问策略  6 最近诊断事件  7 发布消息  0 退出");
            Console.Write("请选择：");
            var choice = Console.ReadLine();
            if (choice is null or "0")
            {
                return;
            }

            switch (choice)
            {
                case "1":
                    WriteResult(await service.GetMqttDashboardStatsAsync());
                    break;
                case "2":
                    WriteResult(await service.GetMqttServerListAsync());
                    break;
                case "3":
                    WriteResult(await service.GetMqttClientListAsync());
                    break;
                case "4":
                    WriteResult(await service.GetMqttTopicListAsync());
                    break;
                case "5":
                    WriteResult(await service.GetMqttAccessPolicyAsync(new LoginedRequest()));
                    break;
                case "6":
                    WriteResult(await service.GetMqttDiagnosticsAsync(new GetMqttDiagnosticsRequest { Limit = 20 }));
                    break;
                case "7":
                    await this.PublishAsync();
                    break;
                default:
                    Console.WriteLine("请输入有效选项。");
                    break;
            }
        }
    }

    private async Task PublishAsync()
    {
        Console.Write("目标主题：");
        var topic = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(topic) || topic.Contains('+') || topic.Contains('#'))
        {
            Console.WriteLine("发布主题不能为空，也不能包含通配符。");
            return;
        }

        Console.Write("文本负载：");
        var payload = Console.ReadLine();
        if (payload is null)
        {
            return;
        }

        Console.Write("此操作使用管理权限向 Broker 发布消息（QoS 0，不保留），输入 y 确认：");
        if (!string.Equals(Console.ReadLine(), "y", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var result = await service.PublishMqttMessageAsync(new PublishMqttMessageRequest
        {
            Topic = topic,
            Payload = payload,
            Qos = 0,
            Retain = false
        });
        Console.WriteLine(result.IsSuccess ? "发布成功。" : $"发布失败：{result.Message}");
    }

    private static void WriteResult<T>(Result<T> result)
    {
        if (!result.IsSuccess)
        {
            Console.Error.WriteLine($"调用失败：{result.Message}");
            return;
        }

        Console.WriteLine(JsonSerializer.Serialize(result.Value, s_jsonOptions));
    }
}
