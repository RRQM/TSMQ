import request from '@/utils/request'

export interface LoginRequest {
  Account: string
  Password: string
}

export interface LoginResponse {
  Code: 'Success' | 'Fail' | 'Error' | 'Unknown' | 'Unauthorized' | 'Forbidden'
  Message: string
  LoginedToken?: string
}

export interface RegisterRequest {
  Account: string
  Password: string
  Email?: string
}

export interface ResponseBase<T = any> {
  Code: 'Success' | 'Fail' | 'Error' | 'Unknown' | 'Unauthorized' | 'Forbidden'
  Message: string
  Data?: T
}

export interface RoleRequest {
  RoleId?: number
  RoleName?: string
  RoleCode?: string
  Description?: string
}

export interface RpcMethodInfo {
  ServiceName: string
  MethodName: string
  ClassTargetKey: string
  MethodTargetKey: string
  Description?: string
}

export interface RpcMethodRoleInfo {
  TargetKey: string
  RoleCodes: string[]
}

export const userApi = {
  logout() {
    return request({ url: '/IUserManagerService/Logout', method: 'post', data: {} })
  },
  login(data: LoginRequest) {
    return request<LoginResponse>({
      url: '/IUserManagerService/Login',
      method: 'post',
      data
    })
  },

  register(data: RegisterRequest) {
    return request<ResponseBase>({
      url: '/IUserManagerService/Register',
      method: 'post',
      data
    })
  },

  getUserList(data: any = {}) {
    return request<ResponseBase>({
      url: '/IUserManagerService/GetUserList',
      method: 'post',
      data
    })
  },

  getUserInfo(data: any = {}) {
    return request<ResponseBase>({
      url: '/IUserManagerService/GetUserInfo',
      method: 'post',
      data
    })
  },

  changePassword(data: { OldPassword: string; NewPassword: string }) {
    return request<ResponseBase>({
      url: '/IUserManagerService/ChangePassword',
      method: 'post',
      data
    })
  },

  assignUserRoles(data: { UserId: number; RoleIds: number[] }) {
    return request<ResponseBase>({
      url: '/IUserManagerService/AssignUserRoles',
      method: 'post',
      data
    })
  },

  deleteUser(data: { UserId: number }) {
    return request<ResponseBase>({
      url: '/IUserManagerService/DeleteUser',
      method: 'post',
      data
    })
  }
}

export const roleApi = {
  createRole(data: RoleRequest) {
    return request<ResponseBase>({
      url: '/IRoleManagerService/CreateRole',
      method: 'post',
      data
    })
  },

  updateRole(data: RoleRequest) {
    return request<ResponseBase>({
      url: '/IRoleManagerService/UpdateRole',
      method: 'post',
      data
    })
  },

  deleteRole(data: RoleRequest) {
    return request<ResponseBase>({
      url: '/IRoleManagerService/DeleteRole',
      method: 'post',
      data
    })
  },

  getRoleList(data: any = {}) {
    return request<ResponseBase>({
      url: '/IRoleManagerService/GetRoleList',
      method: 'post',
      data
    })
  },

}

export const rpcAuthorizationApi = {
  getAllRpcMethods(data: any = {}) {
    return request<ResponseBase>({
      url: '/IRpcAuthorizationService/GetAllRpcMethods',
      method: 'post',
      data
    })
  },

  getAllRpcRoles(data: any = {}) {
    return request<ResponseBase>({
      url: '/IRpcAuthorizationService/GetAllRpcRoles',
      method: 'post',
      data
    })
  },

  getRpcRoles(data: { TargetKey: string }) {
    return request<ResponseBase>({
      url: '/IRpcAuthorizationService/GetRpcRoles',
      method: 'post',
      data
    })
  },

  setRpcRoles(data: { TargetKey: string; RoleCodes: string[]; Description?: string }) {
    return request<ResponseBase>({
      url: '/IRpcAuthorizationService/SetRpcRoles',
      method: 'post',
      data
    })
  },

  removeRpcRoles(data: { TargetKey: string }) {
    return request<ResponseBase>({
      url: '/IRpcAuthorizationService/RemoveRpcRoles',
      method: 'post',
      data
    })
  }
}

export interface LogEntry {
  Id: number
  Timestamp: string
  Level: string
  Category: string
  Message: string
  Exception?: string
}

export interface GetLogListRequest {
  LoginedToken?: string
  Level?: string
  Category?: string
  Keyword?: string
  StartDate?: string
  EndDate?: string
  Skip?: number
  Take?: number
}

export interface GetLogListResponse extends ResponseBase {
  Logs?: LogEntry[]
  Total?: number
}

export const logApi = {
  getLogList(data: GetLogListRequest) {
    return request<GetLogListResponse>({
      url: '/ILogManagerService/GetLogList',
      method: 'post',
      data
    })
  },

  clearLogs(data: { LoginedToken?: string } = {}) {
    return request<ResponseBase>({
      url: '/ILogManagerService/ClearLogs',
      method: 'post',
      data
    })
  }
}

export type MqttServerType = 'Tcp' | 'WebSocket'

export interface MqttServerInfo {
  Id: string
  Type: MqttServerType
  Host: string
  Port: number
  Path: string
  IsRunning: boolean
  CreatedAt: string
  SslCertId?: string
  SslCertName?: string
}

export interface MqttServerListResponse extends ResponseBase {
  Servers?: MqttServerInfo[]
}

export interface MqttServerResponse extends ResponseBase {
  Server?: MqttServerInfo
}

export interface MqttAuthUserInfo {
  UserName: string
  Password: string
  Remark?: string
  CreatedAt: string
}

export interface MqttAuthUserListResponse extends ResponseBase {
  Users?: MqttAuthUserInfo[]
}

export interface MqttBrokerSettingsResponse extends ResponseBase {
  MessageCapacity: number
  MessageExpirySeconds: number
  SessionExpirySeconds: number
}

export interface MqttDiscardedStatsResponse extends ResponseBase {
  Total: number
  QueueFull: number
  Expired: number
  PublishFailed: number
  LastResetAt: string
}

export interface MqttDashboardStatsResponse extends ResponseBase {
  OnlineClients: number
  ActiveTopics: number
  RunningServers: number
  TotalReceived: number
  TotalForwarded: number
  TotalBytesReceived: number
  TotalBytesForwarded: number
  DiscardedTotal: number
  DiscardedQueueFull: number
  DiscardedExpired: number
  DiscardedPublishFailed: number
  ServerStartedAt: string
  SnapshotAt: string
}

export interface MqttStatsSnapshot {
  Id: number
  SnapshotAt: string
  OnlineClients: number
  ActiveTopics: number
  RunningServers: number
  TotalReceived: number
  TotalForwarded: number
  TotalBytesReceived: number
  TotalBytesForwarded: number
  DiscardedTotal: number
  DiscardedQueueFull: number
  DiscardedExpired: number
  DiscardedPublishFailed: number
}

export interface MqttDashboardStatsHistoryResponse extends ResponseBase {
  Snapshots?: MqttStatsSnapshot[]
}

export const mqttServerApi = {
  getList(data: any = {}) {
    return request<MqttServerListResponse>({
      url: '/IMqttServerManagerService/GetMqttServerList',
      method: 'post',
      data
    })
  },

  addServer(data: { Type: MqttServerType; Host: string; Port: number; Path?: string; SslCertId?: string }) {
    return request<MqttServerResponse>({
      url: '/IMqttServerManagerService/AddMqttServer',
      method: 'post',
      data
    })
  },

  startServer(data: { ServerId: string }) {
    return request<ResponseBase>({
      url: '/IMqttServerManagerService/StartMqttServer',
      method: 'post',
      data
    })
  },

  stopServer(data: { ServerId: string }) {
    return request<ResponseBase>({
      url: '/IMqttServerManagerService/StopMqttServer',
      method: 'post',
      data
    })
  },

  removeServer(data: { ServerId: string }) {
    return request<ResponseBase>({
      url: '/IMqttServerManagerService/RemoveMqttServer',
      method: 'post',
      data
    })
  },

  getAuthUsers(data: any = {}) {
    return request<MqttAuthUserListResponse>({
      url: '/IMqttServerManagerService/GetMqttAuthUsers',
      method: 'post',
      data
    })
  },

  addAuthUser(data: { UserName: string; Password: string; Remark?: string }) {
    return request<ResponseBase>({
      url: '/IMqttServerManagerService/AddMqttAuthUser',
      method: 'post',
      data
    })
  },

  updateAuthUser(data: { UserName: string; Password: string; Remark?: string }) {
    return request<ResponseBase>({
      url: '/IMqttServerManagerService/UpdateMqttAuthUser',
      method: 'post',
      data
    })
  },

  deleteAuthUser(data: { UserName: string }) {
    return request<ResponseBase>({
      url: '/IMqttServerManagerService/DeleteMqttAuthUser',
      method: 'post',
      data
    })
  },

  getBrokerSettings(data: any = {}) {
    return request<MqttBrokerSettingsResponse>({
      url: '/IMqttServerManagerService/GetMqttBrokerSettings',
      method: 'post',
      data
    })
  },

  setBrokerSettings(data: { MessageCapacity: number; MessageExpirySeconds: number; SessionExpirySeconds: number }) {
    return request<ResponseBase>({
      url: '/IMqttServerManagerService/SetMqttBrokerSettings',
      method: 'post',
      data
    })
  },

  getDiscardedStats(data: any = {}) {
    return request<MqttDiscardedStatsResponse>({
      url: '/IMqttServerManagerService/GetMqttDiscardedStats',
      method: 'post',
      data
    })
  },

  resetDiscardedStats(data: any = {}) {
    return request<ResponseBase>({
      url: '/IMqttServerManagerService/ResetMqttDiscardedStats',
      method: 'post',
      data
    })
  },

  getDashboardStats(data: any = {}) {
    return request<MqttDashboardStatsResponse>({
      url: '/IMqttServerManagerService/GetMqttDashboardStats',
      method: 'post',
      data
    })
  },

  getDashboardStatsHistory(data: { Granularity: string }) {
    return request<MqttDashboardStatsHistoryResponse>({
      url: '/IMqttServerManagerService/GetDashboardStatsHistory',
      method: 'post',
      data
    })
  },

  getSslCerts(data: any = {}) {
    return request<MqttSslCertListResponse>({
      url: '/IMqttServerManagerService/GetMqttSslCerts',
      method: 'post',
      data
    })
  },

  addSslCert(data: { Name: string; Type: number; CertData: string; KeyData?: string; Password?: string }) {
    return request<MqttSslCertResponse>({
      url: '/IMqttServerManagerService/AddMqttSslCert',
      method: 'post',
      data
    })
  },

  deleteSslCert(data: { CertId: string }) {
    return request<ResponseBase>({
      url: '/IMqttServerManagerService/DeleteMqttSslCert',
      method: 'post',
      data
    })
  }
}

export const MqttSslCertTypeEnum = {
  Pfx: 0,
  Pem: 1
} as const

export type MqttSslCertTypeValue = typeof MqttSslCertTypeEnum[keyof typeof MqttSslCertTypeEnum]

export interface MqttSslCertInfo {
  Id: string
  Name: string
  Type: MqttSslCertTypeValue
  Subject: string
  Thumbprint: string
  NotBefore: string
  NotAfter: string
  CreatedAt: string
}

export interface MqttSslCertListResponse extends ResponseBase {
  Certs?: MqttSslCertInfo[]
}

export interface MqttSslCertResponse extends ResponseBase {
  Cert?: MqttSslCertInfo
}

export interface MqttClientInfo {
  ClientId: string
  UserName?: string
  Status: string
  IpAddress: string
  KeepAlive: number
  CleanStart: boolean
  SessionExpiryInterval: number
  ConnectedAt: string
}

export interface MqttClientListResponse extends ResponseBase {
  Clients?: MqttClientInfo[]
}

export const mqttClientApi = {
  getClientList(data: any = {}) {
    return request<MqttClientListResponse>({
      url: '/IMqttClientManagerService/GetMqttClientList',
      method: 'post',
      data
    })
  },

  kickClients(data: { ClientIds: string[] }) {
    return request<ResponseBase>({
      url: '/IMqttClientManagerService/KickMqttClients',
      method: 'post',
      data
    })
  }
}

export interface MqttTopicInfo {
  Topic: string
  SubscriberCount: number
  Subscribers: string[]
}

export interface MqttTopicStats {
  Topic: string
  SubscriberCount: number
  InflowCount: number
  OutflowCount: number
  SnapshotAt: string
}

export interface MqttTopicListResponse extends ResponseBase {
  Topics?: MqttTopicInfo[]
}

export interface MqttTopicStatsResponse extends ResponseBase {
  Stats?: MqttTopicStats[]
}

export const mqttTopicApi = {
  getTopicList(data: any = {}) {
    return request<MqttTopicListResponse>({
      url: '/IMqttTopicManagerService/GetMqttTopicList',
      method: 'post',
      data
    })
  },

  getTopicStats(data: { Topics: string[] }) {
    return request<MqttTopicStatsResponse>({
      url: '/IMqttTopicManagerService/GetMqttTopicStats',
      method: 'post',
      data
    })
  },

  resetTopicStats(data: { Topics?: string[] } = {}) {
    return request<ResponseBase>({
      url: '/IMqttTopicManagerService/ResetMqttTopicStats',
      method: 'post',
      data
    })
  }
}
