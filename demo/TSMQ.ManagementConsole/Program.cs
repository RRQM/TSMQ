using System.Text;
using TSMQ.ManagementConsole.Commands;
using TSMQ.ManagementConsole.Connection;

namespace TSMQ.ManagementConsole;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        if (args.Length > 0)
        {
            Console.WriteLine("用法：TSMQ.ManagementConsole");
            Console.WriteLine("管理服务地址、账号和密码均在启动后交互输入，密码隐藏输入。");
            return args.Length == 1 && args[0] is "--help" or "-h" ? 0 : 1;
        }

        const string defaultAddress = "http://127.0.0.1:7789";
        Console.Write($"管理服务地址（默认：{defaultAddress}，直接回车使用默认地址）：");
        var input = Console.ReadLine();
        if (input is null)
        {
            return 1;
        }

        var address = string.IsNullOrWhiteSpace(input) ? defaultAddress : input.Trim();
        if (!Uri.TryCreate(address, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttp)
        {
            Console.Error.WriteLine("请输入 http:// 开头的管理服务地址；不要填写 MQTT 监听端口。");
            return 1;
        }

        Console.Write("管理账号：");
        var account = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(account))
        {
            Console.Error.WriteLine("管理账号不能为空。");
            return 1;
        }

        if (Console.IsInputRedirected)
        {
            Console.Error.WriteLine("请在交互式终端运行，以便隐藏输入密码。");
            return 1;
        }

        var password = ReadPassword();
        try
        {
            await using var connection = new ManagementConnection();
            if (!await connection.ConnectAsync(address, account, password.ToString()))
            {
                return 1;
            }

            password.Clear();
            await new ManagementCommands(connection.Service).RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"操作失败：{ex.Message}");
            return 1;
        }
        finally
        {
            password.Clear();
        }
    }

    private static StringBuilder ReadPassword()
    {
        Console.Write("管理密码：");
        var password = new StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return password;
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0)
                {
                    password.Length--;
                }
            }
            else if (!char.IsControl(key.KeyChar))
            {
                password.Append(key.KeyChar);
            }
        }
    }
}
