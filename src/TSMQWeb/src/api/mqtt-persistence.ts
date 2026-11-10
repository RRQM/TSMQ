import request from '@/utils/request'

export interface PersistencePolicy {
  Enabled: boolean
  TopicFilters: string[]
  RetentionDays: number
}

export interface PersistedMessage {
  Id: number
  Timestamp: string
  ClientId: string
  UserName: string
  Topic: string
  Qos: number
  Retain: boolean
  PayloadBase64: string
  PayloadLength: number
}

export interface MessageQuery {
  TopicFilter: string
  ClientId: string
  StartTime?: string
  EndTime?: string
  BeforeId: number
  Limit: number
}

async function post<T>(method: string, data: object = {}): Promise<T> {
  return await request({ url: `/IMqttPersistenceService/${method}`, method: 'post', data }) as unknown as T
}

export const mqttPersistenceApi = {
  getPolicy: () => post<{ Policy: PersistencePolicy }>('GetPolicy'),
  savePolicy: (Policy: PersistencePolicy) => post<{ Message?: string }>('SavePolicy', { Policy }),
  queryMessages: (data: MessageQuery) => post<{ Messages: PersistedMessage[]; HasMore: boolean }>('QueryMessages', data)
}
