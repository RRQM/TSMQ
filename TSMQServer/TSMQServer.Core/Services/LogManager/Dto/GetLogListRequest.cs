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

namespace TSMQServer.Core;

/// <summary>
/// 获取日志列表请求
/// </summary>
public class GetLogListRequest : LoginedRequest
{
    /// <summary>
    /// 日志级别筛选，为 <see langword="null"/> 时不筛选
    /// </summary>
    public string? Level { get; set; }

    /// <summary>
    /// 分类（模块）关键字筛选，为 <see langword="null"/> 时不筛选
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// 消息内容关键字筛选，为 <see langword="null"/> 时不筛选
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 起始日期（含）
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 结束日期（含）
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 跳过条数
    /// </summary>
    public int Skip { get; set; }

    /// <summary>
    /// 获取条数，默认100
    /// </summary>
    public int Take { get; set; } = 100;
}
