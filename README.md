# TSMQ

基于 [TouchSocket](https://touchsocket.net/) 框架构建的权限管理系统模板项目，演示了如何使用 TouchSocket 的 RPC 能力（WebApi / DMTP / WebSocket JsonRpc）构建完整的前后端分离应用。

## 功能特性

- **用户管理**：注册、登录、用户信息查询、修改密码、删除用户、分配角色
- **角色管理**：创建、更新、删除、查询角色
- **动态 RPC 授权**：按方法或按类动态配置角色访问权限，运行时即时生效
- **日志管理**：基于 NLog 的服务端日志查询与筛选
- **多协议支持**：同一服务接口同时支持 WebApi（HTTP）、DMTP RPC、WebSocket JsonRpc 三种调用协议
- **多客户端演示**：WPF、Avalonia（跨平台）、WinForms 三种桌面客户端，以及 Vue 3 Web 前端

## 技术栈

### 服务端（TSMQServer）

| 技术 | 说明 |
|------|------|
| .NET 10.0 | 目标框架，支持 AOT 发布 |
| TouchSocket 4.1.9 | 通信框架（HttpDmtpService / WebApi / DMTP RPC / WebSocket JsonRpc） |
| SQLite + Dapper | 轻量级数据库与 ORM |
| JWT (System.IdentityModel) | 基于 Token 的身份认证 |
| NLog | 日志框架 |
| System.Text.Json (AOT) | 高性能序列化 |

### 桌面客户端（TSMQClient）

| 技术 | 说明 |
|------|------|
| .NET 10.0 | 目标框架 |
| WPF / Avalonia / WinForms | 三种 UI 框架 |
| CommunityToolkit.Mvvm | MVVM 框架 |
| Baboon | 桌面应用宿主框架 |

### Web 前端（TSMQWeb）

| 技术 | 说明 |
|------|------|
| Vue 3 + TypeScript | 前端框架 |
| Vite | 构建工具 |
| Element Plus | UI 组件库 |
| Pinia | 状态管理 |
| Vue Router | 路由管理 |
| Axios | HTTP 请求 |

## 项目结构

```
TSMQ/
├── TSMQServer/              # 服务端解决方案
│   ├── TSMQServer.App/      #   启动入口（HttpDmtpService，监听端口 7789）
│   ├── TSMQServer.Core/     #   RPC 服务接口定义（契约层）
│   │   └── Services/
│   │       ├── IUserManagerService.cs
│   │       ├── IRoleManagerService.cs
│   │       ├── IRpcAuthorizationService.cs
│   │       └── ILogManagerService.cs
│   ├── TSMQServer.Core.Imp/ #   服务实现层（JWT、鉴权、日志等）
│   │   ├── Attributes/            #     Authorize / AllowAnonymous / ExceptionFilter / LoggerFilter
│   │   └── Services/
│   │       ├── Jwt/               #     JWT 令牌生成与验证
│   │       ├── UserManager/       #     用户管理服务实现
│   │       ├── RoleManager/       #     角色管理服务实现
│   │       ├── RpcAuthorization/  #     RPC 动态角色授权服务实现
│   │       └── LogManager/        #     日志管理服务实现（NLog 文件读取）
│   ├── TSMQServer.Db/       #   数据库抽象接口层
│   └── TSMQServer.Db.Sqlite/#   SQLite 数据库实现（Dapper + 自动建表）
│
├── TSMQClient/              # 桌面客户端解决方案
│   ├── Core/
│   │   ├── TSMQClient.Core/ #   核心抽象（IRemoteService 接口）
│   │   └── TSMQClient.Mvvm/ #   共享 ViewModel（MVVM 层，UI 无关）
│   ├── Modules/
│   │   └── TSMQClient.Remote.Module/  # 远程服务模块（支持 DMTP / WebApi 切换）
│   ├── UI/
│   │   ├── TSMQClient.Wpf/      # WPF 客户端
│   │   ├── TSMQClient.Avalonia/ # Avalonia 客户端（跨平台）
│   │   └── TSMQClient.Winform/  # WinForms 客户端
│   └── Tests/                          # 单元测试
│
└── TSMQWeb/                 # Web 前端
    └── src/
        ├── api/index.ts           #   API 接口封装
        ├── views/                 #   页面组件
        │   ├── Login.vue          #     登录页
        │   ├── Dashboard.vue      #     首页仪表盘
        │   ├── user/UserList.vue  #     用户管理
        │   ├── role/RoleList.vue  #     角色管理
        │   ├── rpc-authorization/ #     RPC 角色授权管理
        │   └── LogViewer.vue      #     日志查看器
        ├── stores/                #   Pinia 状态管理
        ├── router/index.ts        #   路由配置（含鉴权守卫）
        ├── layout/MainLayout.vue  #   主布局
        └── utils/                 #   工具函数（请求封装、日志）
```

## 架构说明

### 单端口多协议

服务端使用 TouchSocket 的 `HttpDmtpService` 在 **7789** 端口同时提供：

- **HTTP WebApi**：标准 REST 风格接口，Web 前端通过 Vite 代理访问
- **DMTP RPC**：高性能双向 RPC 通信，适用于桌面客户端
- **WebSocket JsonRpc**：基于 WebSocket 的 JSON-RPC，路径为 `/wsjsonrpc`

### 鉴权机制

采用 **JWT + 动态角色授权** 双重鉴权：

1. **JWT 验证**：通过 `AuthorizeAttribute` 过滤器自动从请求参数中提取 `LoginedToken`，验证 JWT 有效性
2. **动态角色授权**：查询数据库中配置的 RPC 方法角色映射，优先匹配方法级键（`ClassName.MethodName`），无配置则回退至类级键（`ClassName`），数据库未配置时默认放行
3. `AllowAnonymous` 标记的方法可跳过鉴权（如登录、注册）

### 客户端抽象

桌面客户端通过 `IRemoteService` 接口抽象远程调用，可在 `TSMQClientRemoteModule` 中一行代码切换传输协议：

```csharp
// 使用 DMTP RPC
e.Services.UseDmtpRemote();

// 或使用 WebApi
e.Services.UseWebApiRemote();
```

## 快速开始

### 环境要求

- .NET 10.0 SDK
- Node.js 18+（仅 Web 前端需要）
- Visual Studio 2022 / Rider（可选）

### 启动服务端

```bash
cd TSMQServer
dotnet run --project TSMQServer.App
```

服务启动后将自动创建 SQLite 数据库并初始化默认数据。

### 启动 Web 前端

```bash
cd TSMQWeb
npm install
npm run dev
```

浏览器访问 http://localhost:3000 ，开发服务器会自动将 `/api` 请求代理到后端 7789 端口。

### 启动桌面客户端

```bash
cd TSMQClient

# WPF 客户端
dotnet run --project UI/TSMQClient.Wpf

# Avalonia 客户端
dotnet run --project UI/TSMQClient.Avalonia

# WinForms 客户端
dotnet run --project UI/TSMQClient.Winform
```

### 默认账号

| 账号 | 密码 | 角色 |
|------|------|------|
| admin | admin123 | 管理员（admin） |

## API 接口

所有接口均通过 WebApi 暴露，基础路径为 `/I{ServiceName}/{MethodName}`。

### 用户管理（IUserManagerService）

| 接口 | 方法 | 说明 |
|------|------|------|
| `/IUserManagerService/Login` | POST | 用户登录，返回 JWT Token |
| `/IUserManagerService/Register` | POST | 用户注册 |
| `/IUserManagerService/GetUserList` | POST | 获取用户列表 |
| `/IUserManagerService/GetUserInfo` | POST | 获取当前用户信息 |
| `/IUserManagerService/ChangePassword` | POST | 修改密码 |
| `/IUserManagerService/AssignUserRoles` | POST | 分配用户角色 |
| `/IUserManagerService/DeleteUser` | POST | 删除用户 |

### 角色管理（IRoleManagerService）

| 接口 | 方法 | 说明 |
|------|------|------|
| `/IRoleManagerService/CreateRole` | POST | 创建角色 |
| `/IRoleManagerService/UpdateRole` | POST | 更新角色 |
| `/IRoleManagerService/DeleteRole` | POST | 删除角色 |
| `/IRoleManagerService/GetRoleList` | POST | 获取角色列表 |

### RPC 授权（IRpcAuthorizationService）

| 接口 | 方法 | 说明 |
|------|------|------|
| `/IRpcAuthorizationService/GetAllRpcMethods` | POST | 获取所有已注册的 RPC 方法 |
| `/IRpcAuthorizationService/GetAllRpcRoles` | POST | 获取所有 RPC 授权配置 |
| `/IRpcAuthorizationService/GetRpcRoles` | POST | 获取指定目标的授权配置 |
| `/IRpcAuthorizationService/SetRpcRoles` | POST | 设置指定目标的角色授权 |
| `/IRpcAuthorizationService/RemoveRpcRoles` | POST | 移除指定目标的授权配置 |

### 日志管理（ILogManagerService）

| 接口 | 方法 | 说明 |
|------|------|------|
| `/ILogManagerService/GetLogList` | POST | 按条件查询服务端日志 |

> 需鉴权的接口请在请求体中携带 `LoginedToken` 字段。

## Swagger 文档

Debug 模式下启动服务端后，浏览器会自动打开 Swagger 页面（`http://localhost:7789/swagger`），可直接在线测试所有 API 接口。

## 数据库

项目使用 SQLite，首次启动时自动创建数据库文件 `touchadmin.db` 并初始化以下表：

- **Users**：用户表（账号、密码、昵称、邮箱、手机、角色ID列表等）
- **Roles**：角色表（角色名称、角色编码、描述、启用状态）
- **LoginSessions**：登录会话表
- **RpcMethodRoles**：RPC 方法角色授权映射表（目标键 -> 角色编码列表）

## 开源协议

本项目采用 [Apache License 2.0](LICENSE) 开源协议。


## 为什么选择 TSMQ？

市面上已经有大量成熟的后台管理系统和权限框架，如若依、ABP 等。它们功能全面、生态完善，但往往也带来了 **学习成本高、项目臃肿、部署复杂** 等问题。TSMQ 的定位不是"功能最多"，而是 **"最容易上手、最容易改造"**。

### 核心优势

**1. 一套接口，三种协议，一行切换**

大多数权限系统只提供 HTTP REST API。TSMQ 基于 TouchSocket 的 RPC 能力，同一套服务接口 **同时支持 WebApi（HTTP）、DMTP RPC、WebSocket JsonRpc** 三种调用方式。在客户端中只需一行代码即可切换传输协议：

```csharp
e.Services.UseDmtpRemote();  // 高性能长连接
e.Services.UseWebApiRemote(); // 标准 HTTP
```

这意味着你可以根据场景自由选择：Web 用 HTTP、桌面端用 DMTP 长连接、需要双向推送用 WebSocket —— 业务代码不需要任何改动。

**2. 极致轻量，零外部依赖**

- 服务端仅依赖 **TouchSocket + SQLite + Dapper + NLog**，无需 Redis、无需 EF Core、无需消息队列
- 整个项目 **单文件数据库**，拷贝即可迁移，无需安装数据库服务，当然也可以替换为 MySQL、SQL Server 等其他数据库
- 支持 **AOT 发布**，编译产物体积极小，适合嵌入式和边缘部署场景

**3. 真正的项目模版，不是脚手架**

- 内置 **RenameTools.exe**，一条命令批量重命名项目名称，10秒钟从模版变成你的项目
- 代码结构清晰，Core（接口定义）→ Core.Imp（实现）→ Db（数据访问）**三层分离**，新增接口只需 4 个文件

**4. 动态 RPC 授权，运行时即时生效**

TSMQ 将每个 RPC 方法与角色映射存储在数据库中，通过管理界面修改后 **立即生效**，无需重启服务。

**5. 桌面 + Web 双端演示**

同时提供 **WPF、Avalonia（跨平台）、WinForms** 三种桌面客户端和 **Vue 3** Web 前端。不仅是演示，更是可直接作为你项目的 UI 起点：

- 客户端通过 `IRemoteService` 接口抽象远程调用，UI 层与通信层完全解耦
- 切换 UI 框架时只需新建 UI 项目并引用共享的 Core 和 Mvvm 模块

**6. 学习 TouchSocket RPC 的最佳入口**

如果你正在学习 TouchSocket 框架，或者想在项目中使用 TouchSocket 的 RPC 能力，TSMQ 是一个 **完整的、可运行的生产级示例**。它覆盖了 WebApi、DMTP、JsonRpc 三种 RPC 的定义、实现、鉴权、序列化等全部最佳实践。

### 适用场景

| 场景 | 说明 |
|------|------|
| 小型后台管理系统 | 功能够用，不过度设计，开箱即用 |
| 桌面应用后端 | 需要长连接 RPC 而非纯 HTTP 的桌面应用 |
| IoT / 工控上位机 | 轻量级、支持 AOT、单文件部署 |
| TouchSocket 学习 | 完整的 RPC 实战参考项目 |
| 二次开发基础 | 10秒钟改项目名，快速搭建自己的系统 |


## 相关链接

- [TouchSocket 官网](https://touchsocket.net/)
- [TouchSocket GitHub](https://github.com/RRQM/TouchSocket)
- [作者 CSDN 博客](https://blog.csdn.net/qq_40374647)
- [作者哔哩哔哩](https://space.bilibili.com/94253567)
- [交流 QQ 群：234762506](https://jq.qq.com/?_wv=1027&k=5bgUO6cI)
