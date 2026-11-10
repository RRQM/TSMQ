import request from '@/utils/request'

export interface AclRule { UserName: string; ClientId: string; TopicFilter: string; Action: string; Allow: boolean }
export interface AccessPolicy { AuthenticationEnabled: boolean; AclEnabled: boolean; CapturePayload: boolean; Rules: AclRule[] }
export interface DiagnosticEvent {
  Id: number; Timestamp: string; Kind: string; ClientId: string; UserName: string; Topic: string; Detail: string
  Qos: number; Retain: boolean; PayloadLength: number; Truncated: boolean; PayloadBase64: string
}
async function post<T>(method: string, data: object = {}): Promise<T> {
  return await request({ url: `/IMqttAccessService/${method}`, method: 'post', data }) as unknown as T
}
export const mqttAccessApi = {
  getPolicy: () => post<{ Policy: AccessPolicy }>('GetMqttAccessPolicy'),
  savePolicy: (Policy: AccessPolicy) => post<{ Message: string }>('SetMqttAccessPolicy', { Policy }),
  test: (data: { UserName: string; ClientId: string; Topic: string; Action: string }) => post<{ Allowed: boolean }>('TestMqttAccess', data),
  diagnostics: (data: { AfterId: number; TopicFilter: string; ClientId: string; Limit: number }) =>
    post<{ Events: DiagnosticEvent[]; LastId: number }>('GetMqttDiagnostics', data),
  publish: (data: { Topic: string; Payload: string; IsBase64: boolean; Qos: number; Retain: boolean }) =>
    post<{ Message: string }>('PublishMqttMessage', data)
}
