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

using System.ComponentModel;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.JsonRpc;
using TouchSocket.Rpc;
using TouchSocket.WebApi;

namespace TSMQServer.Core;

/// <summary>
/// MQTT 服务器管理服务接口。
/// </summary>
[GeneratorRpcProxy]
public interface IMqttServerManagerService : ITransientRpcServer
{
    /// <summary>
    /// 获取所有 MQTT 服务器列表。
    /// </summary>
    [Description("获取所有MQTT服务器列表")]
    [WebApi(Method = HttpMethodType.Post)]
    [DmtpRpc]
    [JsonRpc(MethodInvoke = true)]
    Task<MqttServerListResponse> GetMqttServerList(LoginedRequest request);

    /// <summary>
    /// 添加 MQTT 服务器监听。
    /// </summary>
    [Description("添加MQTT服务器")]
    [WebApi(Method = HttpMethodType.Post)]
    [DmtpRpc]
    [JsonRpc(MethodInvoke = true)]
    Task<MqttServerResponse> AddMqttServer(AddMqttServerRequest request);

    /// <summary>
    /// 启动指定 MQTT 服务器监听。
    /// </summary>
    [Description("启动MQTT服务器")]
    [WebApi(Method = HttpMethodType.Post)]
    [DmtpRpc]
    [JsonRpc(MethodInvoke = true)]
    Task<ResponseBase> StartMqttServer(MqttServerIdRequest request);

    /// <summary>
    /// 停止指定 MQTT 服务器监听。
    /// </summary>
    [Description("停止MQTT服务器")]
    [WebApi(Method = HttpMethodType.Post)]
    [DmtpRpc]
    [JsonRpc(MethodInvoke = true)]
    Task<ResponseBase> StopMqttServer(MqttServerIdRequest request);

    /// <summary>
    /// 移除指定 MQTT 服务器配置。
    /// </summary>
    [Description("移除MQTT服务器")]
    [WebApi(Method = HttpMethodType.Post)]
    [DmtpRpc]
    [JsonRpc(MethodInvoke = true)]
    Task<ResponseBase> RemoveMqttServer(MqttServerIdRequest request);

    /// <summary>
    /// 获取 MQTT 连接授权用户列表。
    /// </summary>
    [Description("获取MQTT连接授权用户列表")]
    [WebApi(Method = HttpMethodType.Post)]
    [DmtpRpc]
    [JsonRpc(MethodInvoke = true)]
    Task<MqttAuthUserListResponse> GetMqttAuthUsers(LoginedRequest request);

    /// <summary>
    /// 添加 MQTT 连接授权用户。
    /// </summary>
    [Description("添加MQTT连接授权用户")]
    [WebApi(Method = HttpMethodType.Post)]
    [DmtpRpc]
    [JsonRpc(MethodInvoke = true)]
    Task<ResponseBase> AddMqttAuthUser(MqttAuthUserRequest request);

    /// <summary>
    /// 更新 MQTT 连接授权用户密码或备注。
    /// </summary>
    [Description("更新MQTT连接授权用户")]
    [WebApi(Method = HttpMethodType.Post)]
    [DmtpRpc]
    [JsonRpc(MethodInvoke = true)]
    Task<ResponseBase> UpdateMqttAuthUser(MqttAuthUserRequest request);

    /// <summary>
    /// 删除 MQTT 连接授权用户。
    /// </summary>
    [Description("删除MQTT连接授权用户")]
    [WebApi(Method = HttpMethodType.Post)]
    [DmtpRpc]
    [JsonRpc(MethodInvoke = true)]
    Task<ResponseBase> DeleteMqttAuthUser(MqttAuthUserDeleteRequest request);

    /// <summary>
    /// 获取 MQTT Broker 当前设置。
    /// </summary>
    [Description("获取MQTT Broker设置")]
    [WebApi(Method = HttpMethodType.Post)]
    [DmtpRpc]
    [JsonRpc(MethodInvoke = true)]
    Task<MqttBrokerSettingsResponse> GetMqttBrokerSettings(LoginedRequest request);

    /// <summary>
    /// 设置 MQTT Broker 参数，立即生效。
    /// </summary>
    [Description("设置MQTT Broker参数")]
    [WebApi(Method = HttpMethodType.Post)]
    [DmtpRpc]
    [JsonRpc(MethodInvoke = true)]
    Task<ResponseBase> SetMqttBrokerSettings(SetMqttBrokerSettingsRequest request);

    /// <summary>
    /// 获取消息丢弃统计信息。
    /// </summary>
    [Description("获取消息丢弃统计")]
    [WebApi(Method = HttpMethodType.Post)]
    [DmtpRpc]
    [JsonRpc(MethodInvoke = true)]
    Task<MqttDiscardedStatsResponse> GetMqttDiscardedStats(LoginedRequest request);

    /// <summary>
    /// 重置消息丢弃统计计数。
    /// </summary>
    [Description("重置消息丢弃统计")]
    [WebApi(Method = HttpMethodType.Post)]
    [DmtpRpc]
    [JsonRpc(MethodInvoke = true)]
    Task<ResponseBase> ResetMqttDiscardedStats(LoginedRequest request);

    /// <summary>
    /// 获取仪表盘综合统计数据。
    /// </summary>
    [Description("获取仪表盘统计数据")]
    [WebApi(Method = HttpMethodType.Post)]
    [DmtpRpc]
    [JsonRpc(MethodInvoke = true)]
    Task<MqttDashboardStatsResponse> GetMqttDashboardStats(LoginedRequest request);

    /// <summary>
    /// 获取仪表盘统计历史数据（支持按分钟/小时/天/全部粒度查询）。
    /// </summary>
    [Description("获取仪表盘统计历史数据")]
    [WebApi(Method = HttpMethodType.Post)]
    [DmtpRpc]
    [JsonRpc(MethodInvoke = true)]
    Task<MqttDashboardStatsHistoryResponse> GetDashboardStatsHistory(GetDashboardStatsHistoryRequest request);

    /// <summary>
    /// 获取所有 SSL 证书列表。
    /// </summary>
    [Description("获取SSL证书列表")]
    [WebApi(Method = HttpMethodType.Post)]
    [DmtpRpc]
    [JsonRpc(MethodInvoke = true)]
    Task<MqttSslCertListResponse> GetMqttSslCerts(LoginedRequest request);

    /// <summary>
    /// 添加 SSL 证书。
    /// </summary>
    [Description("添加SSL证书")]
    [WebApi(Method = HttpMethodType.Post)]
    [DmtpRpc]
    [JsonRpc(MethodInvoke = true)]
    Task<MqttSslCertResponse> AddMqttSslCert(AddMqttSslCertRequest request);

    /// <summary>
    /// 删除 SSL 证书。
    /// </summary>
    [Description("删除SSL证书")]
    [WebApi(Method = HttpMethodType.Post)]
    [DmtpRpc]
    [JsonRpc(MethodInvoke = true)]
    Task<ResponseBase> DeleteMqttSslCert(MqttSslCertIdRequest request);
}
