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

using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace TSMQServer.Core.Imp.Services;

/// <summary>
/// 读取 NLog 按天分文件存储的日志
/// </summary>
public sealed class NLogFileReader
{
    // 行头格式：2024-01-01 12:00:00.0000|LEVEL|Category|Message
    private static readonly Regex s_lineRegex = new(
        @"^(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{4})\|(\w+)\|([^|]*)\|(.*)$",
        RegexOptions.Compiled);

    private readonly LogFileOptions m_options;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public NLogFileReader(IOptions<LogFileOptions> options)
    {
        this.m_options = options.Value;
    }

    /// <summary>
    /// 查询日志条目
    /// </summary>
    public (List<LogEntry> Items, int Total) Query(
        string? level,
        string? category,
        string? keyword,
        DateTime? startDate,
        DateTime? endDate,
        int skip,
        int take)
    {
        var start = startDate?.Date ?? DateTime.Today;
        var end = endDate?.Date ?? DateTime.Today;

        if (end < start)
        {
            end = start;
        }

        var entries = new List<LogEntry>();
        long nextId = 1;

        for (var date = start; date <= end; date = date.AddDays(1))
        {
            var filePath = Path.Combine(this.m_options.LogDirectory, $"{date:yyyy-MM-dd}.log");
            if (!File.Exists(filePath))
            {
                continue;
            }

            ParseFile(filePath, entries, ref nextId);
        }

        // 按时间倒序
        entries.Sort((a, b) => b.Id.CompareTo(a.Id));

        // 筛选
        IEnumerable<LogEntry> query = entries;

        if (!string.IsNullOrEmpty(level))
        {
            query = query.Where(e => string.Equals(e.Level, level, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(e => e.Category != null && e.Category.Contains(category, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(e => e.Message != null && e.Message.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        var all = query.ToList();
        return (all.Skip(skip).Take(take > 0 ? take : 100).ToList(), all.Count);
    }

    /// <summary>
    /// 获取所有日志文件的日期列表
    /// </summary>
    public List<string> GetAvailableDates()
    {
        if (!Directory.Exists(this.m_options.LogDirectory))
        {
            return [];
        }

        return [.. Directory.GetFiles(this.m_options.LogDirectory, "*.log")
            .Select(f => Path.GetFileNameWithoutExtension(f))
            .Where(n => n.Length == 10)
            .OrderDescending()];
    }

    private static void ParseFile(string filePath, List<LogEntry> entries, ref long nextId)
    {
        string[] lines;
        try
        {
            // 用 FileShare.ReadWrite 允许 NLog 持续写入的同时读取
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var sr = new StreamReader(fs);
            lines = sr.ReadToEnd().Split('\n');
        }
        catch
        {
            return;
        }

        LogEntry? current = null;
        var exceptionBuilder = new System.Text.StringBuilder();

        foreach (var rawLine in lines)
        {
            var line = rawLine.TrimEnd('\r');
            if (line.Length == 0)
            {
                continue;
            }

            var match = s_lineRegex.Match(line);
            if (match.Success)
            {
                // 保存上一条
                if (current != null)
                {
                    if (exceptionBuilder.Length > 0)
                    {
                        current.Exception = exceptionBuilder.ToString().Trim();
                        exceptionBuilder.Clear();
                    }

                    entries.Add(current);
                }

                if (!DateTime.TryParse(match.Groups[1].Value, out var ts))
                {
                    current = null;
                    continue;
                }

                current = new LogEntry
                {
                    Id = nextId++,
                    Timestamp = new DateTimeOffset(ts),
                    Level = match.Groups[2].Value,
                    Category = match.Groups[3].Value,
                    Message = match.Groups[4].Value
                };
            }
            else if (current != null)
            {
                // 异常堆栈续行
                exceptionBuilder.AppendLine(line);
            }
        }

        // 最后一条
        if (current != null)
        {
            if (exceptionBuilder.Length > 0)
            {
                current.Exception = exceptionBuilder.ToString().Trim();
            }

            entries.Add(current);
        }
    }
}
