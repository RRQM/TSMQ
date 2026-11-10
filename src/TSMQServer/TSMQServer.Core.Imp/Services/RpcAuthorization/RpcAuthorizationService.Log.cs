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

namespace TSMQServer.Core.Imp.Services;

public partial class RpcAuthorizationService
{
    [LoggerMessage(Level = LogLevel.Information, Message = "开始获取所有RPC授权配置")]
    private static partial void LogGettingAllRpcRoles(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "获取所有RPC授权配置成功，共 {Count} 条")]
    private static partial void LogGetAllRpcRolesSuccess(ILogger logger, int count);

    [LoggerMessage(Level = LogLevel.Information, Message = "开始获取RPC目标角色，目标键：{TargetKey}")]
    private static partial void LogGettingRpcRoles(ILogger logger, string targetKey);

    [LoggerMessage(Level = LogLevel.Information, Message = "获取RPC目标角色成功，目标键：{TargetKey}，角色数量：{Count}")]
    private static partial void LogGetRpcRolesSuccess(ILogger logger, string targetKey, int count);

    [LoggerMessage(Level = LogLevel.Information, Message = "开始设置RPC角色授权，目标键：{TargetKey}，角色数量：{Count}")]
    private static partial void LogSettingRpcRoles(ILogger logger, string targetKey, int count);

    [LoggerMessage(Level = LogLevel.Information, Message = "设置RPC角色授权成功，目标键：{TargetKey}")]
    private static partial void LogSetRpcRolesSuccess(ILogger logger, string targetKey);

    [LoggerMessage(Level = LogLevel.Information, Message = "开始移除RPC角色授权，目标键：{TargetKey}")]
    private static partial void LogRemovingRpcRoles(ILogger logger, string targetKey);

    [LoggerMessage(Level = LogLevel.Warning, Message = "移除RPC角色授权失败，目标键不存在：{TargetKey}")]
    private static partial void LogRemoveRpcRolesNotFound(ILogger logger, string targetKey);

    [LoggerMessage(Level = LogLevel.Information, Message = "移除RPC角色授权成功，目标键：{TargetKey}")]
    private static partial void LogRemoveRpcRolesSuccess(ILogger logger, string targetKey);

    [LoggerMessage(Level = LogLevel.Information, Message = "获取已注册的RPC方法列表，共 {Count} 个方法")]
    private static partial void LogGettingAllRpcMethods(ILogger logger, int count);

    [LoggerMessage(Level = LogLevel.Information, Message = "获取RPC方法列表成功，返回 {Count} 个方法")]
    private static partial void LogGetAllRpcMethodsSuccess(ILogger logger, int count);
}
