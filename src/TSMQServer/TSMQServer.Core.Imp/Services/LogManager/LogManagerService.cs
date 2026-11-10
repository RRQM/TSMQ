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

using Microsoft.Extensions.Logging;
using TSMQServer.Core.Imp.Attributes;
using TouchSocket.Rpc;

namespace TSMQServer.Core.Imp.Services;

/// <summary>
/// 日志管理服务实现，基于 NLog 文件读取
/// </summary>
[ExceptionFilter]
[LoggerFilter]
[Authorize]
public partial class LogManagerService : TransientRpcServer, ILogManagerService
{
    private readonly ILogger<LogManagerService> m_logger;
    private readonly NLogFileReader m_fileReader;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public LogManagerService(ILogger<LogManagerService> logger, NLogFileReader fileReader)
    {
        this.m_logger = logger;
        this.m_fileReader = fileReader;
    }

    /// <inheritdoc/>
    public Task<GetLogListResponse> GetLogList(GetLogListRequest request)
    {
        LogGettingLogList(this.m_logger);

        var take = request.Take <= 0 ? 100 : request.Take;
        var (items, total) = this.m_fileReader.Query(
            request.Level,
            request.Category,
            request.Keyword,
            request.StartDate,
            request.EndDate,
            request.Skip,
            take);

        LogGetLogListSuccess(this.m_logger, total);

        return Task.FromResult(new GetLogListResponse
        {
            Code = ResponseCode.Success,
            Logs = items,
            Total = total
        });
    }
}
