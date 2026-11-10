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

using System.Text.Json.Serialization;

namespace TSMQServer.Core.Json;

// 基础类型
[JsonSerializable(typeof(RequestBase))]
[JsonSerializable(typeof(LoginedRequest))]
[JsonSerializable(typeof(ResponseBase))]
[JsonSerializable(typeof(ResponseCode))]

// 用户管理
[JsonSerializable(typeof(LoginRequest))]
[JsonSerializable(typeof(LoginResponse))]
[JsonSerializable(typeof(RegisterRequest))]
[JsonSerializable(typeof(RegisterResponse))]
[JsonSerializable(typeof(UserInfo))]
[JsonSerializable(typeof(List<UserInfo>))]
[JsonSerializable(typeof(GetUserListResponse))]
[JsonSerializable(typeof(GetUserInfoResponse))]
[JsonSerializable(typeof(ChangePasswordRequest))]
[JsonSerializable(typeof(DeleteUserRequest))]

// 角色管理
[JsonSerializable(typeof(CreateRoleRequest))]
[JsonSerializable(typeof(UpdateRoleRequest))]
[JsonSerializable(typeof(DeleteRoleRequest))]
[JsonSerializable(typeof(GetRoleListRequest))]
[JsonSerializable(typeof(AssignUserRolesRequest))]
[JsonSerializable(typeof(RoleResponse))]
[JsonSerializable(typeof(RoleListResponse))]
[JsonSerializable(typeof(RoleInfo))]
[JsonSerializable(typeof(List<RoleInfo>))]

// 基元数组类型
[JsonSerializable(typeof(long[]))]
[JsonSerializable(typeof(string[]))]

// 日志管理
[JsonSerializable(typeof(LogEntry))]
[JsonSerializable(typeof(List<LogEntry>))]
[JsonSerializable(typeof(GetLogListRequest))]
[JsonSerializable(typeof(GetLogListResponse))]
[JsonSerializable(typeof(DateTime))]
[JsonSerializable(typeof(DateTime?))]
[JsonSerializable(typeof(DateTimeOffset))]

// RPC动态角色授权
[JsonSerializable(typeof(GetRpcRolesRequest))]
[JsonSerializable(typeof(RemoveRpcRolesRequest))]
[JsonSerializable(typeof(SetRpcRolesRequest))]
[JsonSerializable(typeof(RpcMethodRoleInfo))]
[JsonSerializable(typeof(List<RpcMethodRoleInfo>))]
[JsonSerializable(typeof(RpcRoleResponse))]
[JsonSerializable(typeof(RpcRoleListResponse))]
[JsonSerializable(typeof(RpcMethodInfo))]
[JsonSerializable(typeof(List<RpcMethodInfo>))]
[JsonSerializable(typeof(RpcMethodListResponse))]

// MQTT 服务器管理
[JsonSerializable(typeof(MqttServerType))]
[JsonSerializable(typeof(MqttServerInfo))]
[JsonSerializable(typeof(List<MqttServerInfo>))]
[JsonSerializable(typeof(AddMqttServerRequest))]
[JsonSerializable(typeof(MqttServerIdRequest))]
[JsonSerializable(typeof(MqttServerListResponse))]
[JsonSerializable(typeof(MqttServerResponse))]
[JsonSerializable(typeof(MqttAuthUserInfo))]
[JsonSerializable(typeof(List<MqttAuthUserInfo>))]
[JsonSerializable(typeof(MqttAuthUserRequest))]
[JsonSerializable(typeof(MqttAuthUserDeleteRequest))]
[JsonSerializable(typeof(MqttAuthUserListResponse))]
[JsonSerializable(typeof(SetMqttBrokerSettingsRequest))]
[JsonSerializable(typeof(MqttBrokerSettingsResponse))]
[JsonSerializable(typeof(MqttDiscardedStatsResponse))]
[JsonSerializable(typeof(MqttDashboardStatsResponse))]
[JsonSerializable(typeof(MqttStatsSnapshot))]
[JsonSerializable(typeof(List<MqttStatsSnapshot>))]
[JsonSerializable(typeof(GetDashboardStatsHistoryRequest))]
[JsonSerializable(typeof(MqttDashboardStatsHistoryResponse))]

// MQTT SSL 证书管理
[JsonSerializable(typeof(MqttSslCertType))]
[JsonSerializable(typeof(MqttSslCertInfo))]
[JsonSerializable(typeof(List<MqttSslCertInfo>))]
[JsonSerializable(typeof(AddMqttSslCertRequest))]
[JsonSerializable(typeof(MqttSslCertIdRequest))]
[JsonSerializable(typeof(MqttSslCertListResponse))]
[JsonSerializable(typeof(MqttSslCertResponse))]

// MQTT 客户端管理
[JsonSerializable(typeof(MqttClientInfo))]
[JsonSerializable(typeof(List<MqttClientInfo>))]
[JsonSerializable(typeof(KickMqttClientsRequest))]
[JsonSerializable(typeof(MqttClientListResponse))]

// MQTT 主题管理
[JsonSerializable(typeof(MqttTopicInfo))]
[JsonSerializable(typeof(List<MqttTopicInfo>))]
[JsonSerializable(typeof(MqttTopicStats))]
[JsonSerializable(typeof(List<MqttTopicStats>))]
[JsonSerializable(typeof(GetMqttTopicStatsRequest))]
[JsonSerializable(typeof(ResetMqttTopicStatsRequest))]
[JsonSerializable(typeof(MqttTopicListResponse))]
[JsonSerializable(typeof(MqttTopicStatsResponse))]
[JsonSerializable(typeof(MqttAccessPolicy))]
[JsonSerializable(typeof(MqttAccessPolicyResponse))]
[JsonSerializable(typeof(SetMqttAccessPolicyRequest))]
[JsonSerializable(typeof(TestMqttAccessRequest))]
[JsonSerializable(typeof(TestMqttAccessResponse))]
[JsonSerializable(typeof(GetMqttDiagnosticsRequest))]
[JsonSerializable(typeof(MqttDiagnosticsResponse))]
[JsonSerializable(typeof(PublishMqttMessageRequest))]
public partial class TSMQServerCoreJsonSerializerContext : JsonSerializerContext
{
}
