# TSMQServer.Core

[![NuGet](https://img.shields.io/nuget/v/TSMQServer.Core.svg)](https://www.nuget.org/packages/TSMQServer.Core/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/TSMQServer.Core.svg)](https://www.nuget.org/packages/TSMQServer.Core/)

TSMQServer.Core 是 [TSMQ](https://touchsocket.net/) 服务端的契约层 NuGet 包，基于 [TouchSocket](https://touchsocket.net/) 框架构建。  
它包含所有 RPC 服务接口定义、DTO 模型以及远程调用客户端接口，供客户端项目引用，以实现强类型的远程调用（WebApi / DMTP RPC / WebSocket JsonRpc）。

---

## 安装

```bash
dotnet add package TSMQServer.Core
```

---

## 包含内容

### RPC 服务接口（Services）

| 接口 | 说明 |
|------|------|
| `IUserManagerService` | 用户注册、登录、查询、修改密码、删除、角色分配 |
| `IRoleManagerService` | 角色创建、更新、删除、查询 |
| `IRpcAuthorizationService` | 动态 RPC 方法 / 类级别角色授权管理 |
| `ILogManagerService` | 服务端日志查询与筛选 |
| `IMqttServerManagerService` | MQTT 服务器实例管理（增删启停）及授权用户管理 |
| `IMqttClientManagerService` | 已连接 MQTT 客户端查询、踢出、订阅主题查看 |
| `IMqttTopicManagerService` | MQTT 主题浏览与消息推送 |

### 远程调用客户端接口（Clients）

`IRemoteService` 是由源代码生成器自动生成的客户端调用接口，集成了上述全部服务方法，可直接注入使用：

```csharp
// 以 DMTP 调用为例
var result = await remoteService.GetMqttServerListAsync();
```

### 数据模型（Models）

- `LoginedRequest` — 已登录请求基类，携带 Token
- `ResponseBase<T>` — 统一响应包装
- `ResponseCode` — 响应状态码枚举
- `MqttStatsSnapshot` — MQTT 统计快照
- `RpcMethodInfo` / `RpcMethodRoleInfo` — RPC 方法权限信息

---

## 使用示例

### 1. 注册客户端服务

```csharp
// 使用 DMTP 客户端
services.AddSingleton<IRemoteService>(sp =>
{
    var client = new HttpDmtpClient();
    client.Setup(new TouchSocketConfig()
        .SetRemoteIPHost("127.0.0.1:7789")
        .ConfigurePlugins(a =>
        {
            a.UseDmtpRpc();
        }));
    client.Connect();
    return client.CreateDmtpRpcRemoteService<IRemoteService>();
});
```

### 2. 调用 RPC 方法

```csharp
// 获取 MQTT 服务器列表
var result = await remoteService.GetMqttServerListAsync(cancellationToken);
if (result.IsSuccess)
{
    foreach (var server in result.Value)
    {
        Console.WriteLine($"{server.ServerId} - {server.Host}:{server.Port}");
    }
}

// 添加 MQTT 服务器
var addResult = await remoteService.AddMqttServerAsync(
    MqttServerType.Tcp, "0.0.0.0", 1883, "/", null, cancellationToken);
```

---

## 依赖项

| 包 | 说明 |
|----|------|
| `TouchSocket.Dmtp` | DMTP RPC 支持 |
| `TouchSocket.WebApi` | WebApi HTTP 支持 |
| `TouchSocket.JsonRpc` | WebSocket JsonRpc 支持 |

---

## 相关链接

- 官方文档：[https://touchsocket.net/](https://touchsocket.net/)
- GitHub：[https://github.com/RRQM/TouchSocket](https://github.com/RRQM/TouchSocket)
- 问题反馈 QQ 群：234762506

---

## 许可证

本项目遵循仓库根目录的开源许可协议。
