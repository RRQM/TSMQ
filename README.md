# TSMQ

基于 [TouchSocket](https://touchsocket.net/) 通信框架构建的 **MQTT Broker + 后台权限管理系统** 模板项目。不仅是一个完整的 MQTT 服务器管理平台，同时也是一个可直接用于二次开发的企业级权限管理系统。

> **版本**: 0.0.29 | **作者**: 若汝棋茗 | **协议**: [Apache License 2.0](LICENSE)
>
> **仓库地址**: [https://github.com/RRQM/TouchSocket](https://github.com/RRQM/TouchSocket)

---

## 目录

- [功能特性](#功能特性)
- [架构设计](#架构设计)
- [技术栈](#技术栈)
- [项目结构](#项目结构)
- [快速开始](#快速开始)
- [API 接口文档](#api-接口文档)
- [Swagger 文档](#swagger-文档)
- [数据库设计](#数据库设计)
- [配置说明](#配置说明)
- [构建与部署](#构建与部署)
- [客户端开发](#客户端开发)
- [如何新增一个接口](#如何新增一个接口)
- [常见问题](#常见问题)
- [为什么选择 TSMQ？](#为什么选择-tsmq)
- [相关链接](#相关链接)

---

## 功能特性

### MQTT Broker 管理

| 功能模块 | 说明 |
|----------|------|
| **多服务器监听** | 支持动态添加、启动、停止、移除 TCP 和 WebSocket 类型的 MQTT 监听端口 |
| **共享 Broker** | 所有 TCP 和 WebSocket 监听共享同一个 `MqttBroker` 实例，消息互通 |
| **连接认证** | 可开关的 MQTT 连接授权，支持用户名/密码验证 |
| **SSL/TLS 证书管理** | 支持 PFX 和 PEM 格式的 SSL 证书上传、查看和删除 |
| **客户端管理** | 查看所有在线 MQTT 客户端，支持强制踢除指定客户端 |
| **主题监控** | 主题列表查看、每个主题的订阅者数量、消息流入/流出统计 |
| **Broker 参数配置** | 消息容量、消息过期时间、会话过期时间等参数，运行时即时生效 |
| **消息丢弃统计** | 按 QueueFull / Expired / PublishFailed 分类统计消息丢弃情况 |
| **仪表盘统计** | 在线客户端数、活跃主题数、运行服务器数、消息收发总量统计 |
| **历史快照** | 每分钟自动写入统计快照，支持按时间范围查询历史数据 |

### 权限管理系统

| 功能模块 | 说明 |
|----------|------|
| **用户管理** | 注册、登录（JWT Token）、用户列表查询、当前用户信息、修改密码、删除用户、分配角色 |
| **角色管理** | 创建角色、更新角色、删除角色、查询角色列表 |
| **动态 RPC 授权** | 按方法级（`ClassName.MethodName`）或类级（`ClassName`）配置角色访问权限，数据库修改后运行时即时生效，无需重启 |
| **系统日志** | 基于 NLog 按天滚动的服务端日志查询，支持按级别、分类、关键词、时间范围筛选 |

### 多协议通信

| 协议 | 用途 | 路径 / 端口 |
|------|------|-------------|
| HTTP WebApi | Web 前端、Swagger、第三方 HTTP 客户端 | `http://localhost:7789` |
| DMTP RPC | 桌面客户端高性能双向长连接通信 | `tcp://127.0.0.1:7789` |
| WebSocket JsonRpc | WebSocket 双向通信 | `ws://localhost:7789/wsjsonrpc` |

三种协议共用一个端口（`7789`），同一套服务接口无需修改业务代码即可通过任意协议调用。

### 多客户端支持

| 客户端 | 技术框架 | 平台 |
|--------|----------|------|
| Web 前端 | Vue 3 + TypeScript + Element Plus | 浏览器（跨平台） |
| WPF 桌面客户端 | WPF + CommunityToolkit.Mvvm | Windows |
| Avalonia 桌面客户端 | Avalonia 12.0 + CommunityToolkit.Mvvm | Windows / macOS / Linux |
| WinForms 桌面客户端 | WinForms + CommunityToolkit.Mvvm | Windows |

所有桌面客户端共享同一套 ViewModel 和 `IRemoteService` 通信抽象层，切换 UI 框架只需新建 UI 项目并引用共享模块。

---

## 架构设计

### 分层架构

```
┌─────────────────────────────────────────────────────────────┐
│                      表示层 (Presentation)                    │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌────────────┐  │
│  │   WPF    │  │ Avalonia │  │ WinForms │  │  Vue 3 Web │  │
│  └────┬─────┘  └────┬─────┘  └────┬─────┘  └──────┬─────┘  │
│       │              │              │              │         │
│       └──────────────┴──────┬───────┴──────────────┘         │
│                             │                                │
│               ┌─────────────┴─────────────┐                  │
│               │   IRemoteService (抽象)    │                  │
│               │  WebApi / DMTP / JsonRpc  │                  │
│               └─────────────┬─────────────┘                  │
└─────────────────────────────┼───────────────────────────────┘
                              │
┌─────────────────────────────┼───────────────────────────────┐
│                    服务层 (Application)                       │
│               ┌─────────────┴─────────────┐                  │
│               │   HttpDmtpService :7789   │                  │
│               │  (单端口多协议)            │                  │
│               └─────────────┬─────────────┘                  │
│                             │                                │
│     ┌───────────────────────┼───────────────────────┐        │
│     │                       │                       │        │
│  ┌──▼──────────┐  ┌─────────▼─────┐  ┌──────────────▼───┐   │
│  │ WebApi 路由  │  │  DMTP RPC    │  │ WebSocket JsonRpc │   │
│  └──┬──────────┘  └─────────┬─────┘  └──────────────┬───┘   │
│     │                       │                       │        │
│     └───────────────────────┼───────────────────────┘        │
│                             │                                │
│               ┌─────────────┴─────────────┐                  │
│               │       RPC Store           │                  │
│               └─────────────┬─────────────┘                  │
└─────────────────────────────┼───────────────────────────────┘
                              │
┌─────────────────────────────┼───────────────────────────────┐
│                     领域层 (Domain)                           │
│  ┌──────────────────────────┴───────────────────────────┐    │
│  │              TSMQServer.Core (契约层)                  │    │
│  │     服务接口定义 / DTO 模型 / JSON 序列化上下文        │    │
│  └──────────────────────────┬───────────────────────────┘    │
│  ┌──────────────────────────┴───────────────────────────┐    │
│  │            TSMQServer.Core.Imp (实现层)                │    │
│  │   AuthorizeAttribute / ExceptionFilter / LoggerFilter │    │
│  │   UserManager / RoleManager / RpcAuthorization        │    │
│  │   MqttServerManager / MqttClientManager               │    │
│  │   MqttTopicManager / LogManager / JwtService          │    │
│  └──────────────────────────┬───────────────────────────┘    │
│  ┌──────────────────────────┴───────────────────────────┐    │
│  │              TSMQServer.Db (数据库抽象层)              │    │
│  │        IUserManagerFromDB / IRoleManagerFromDB 等     │    │
│  └──────────────────────────┬───────────────────────────┘    │
└─────────────────────────────┼───────────────────────────────┘
                              │
┌─────────────────────────────┼───────────────────────────────┐
│                  基础设施层 (Infrastructure)                  │
│  ┌──────────────────────────┴───────────────────────────┐    │
│  │         TSMQServer.Db.Sqlite (数据库实现)              │    │
│  │        Dapper + TORM + 自动建表 + 种子数据            │    │
│  └──────────────────────────────────────────────────────┘    │
│  ┌──────────────────────────────────────────────────────┐    │
│  │       SQLite 数据库文件 (TSMQ.db)                      │    │
│  └──────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

### 鉴权流程

```
客户端发起 RPC 调用
        │
        ▼
┌───────────────────┐
│  LoggerFilter     │  ← 记录调用日志（请求参数、耗时）
└────────┬──────────┘
         │
         ▼
┌───────────────────┐
│  Authorize Filter │
│                   │
│  是否是            │
│  [AllowAnonymous]? ├─── 是 ──→ 跳过鉴权，直接进入业务逻辑
│                   │
│  否               │
│         │         │
│         ▼         │
│  提取 LoginedToken│
│         │         │
│         ▼         │
│  JWT 验证 Token   ├─── 非法 ──→ 返回 401 Unauthorized
│  有效性           │
│         │ 有效    │
│         ▼         │
│  查询 RPC 方法    │
│  角色映射          │
│    ├─ 方法级键     │
│    │  (Class.Method)│
│    ├─ 类级键      │
│    │  (Class)     │
│    └─ 数据库无配置──┼─── 默认放行
│         │         │
│         ▼         │
│  当前用户角色     │
│  是否匹配？       ├─── 不匹配 ──→ 返回 403 Forbidden
│                   │
│  匹配             │
└────────┬──────────┘
         │
         ▼
┌───────────────────┐
│  业务逻辑执行     │
└────────┬──────────┘
         │
         ▼
┌───────────────────┐
│  ExceptionFilter  │  ← 捕获未处理异常，统一返回错误响应
└───────────────────┘
```

### MQTT Broker 架构

```
                           MqttServerHostService (IHostedService)
                                       │
           ┌───────────────────────────┼───────────────────────────┐
           │                           │                           │
           ▼                           ▼                           ▼
   ┌───────────────┐          ┌───────────────┐          ┌───────────────┐
   │  TSMQService  │          │  HttpService   │          │  TSMQService  │
   │  (TCP MQTT)   │          │  (WS MQTT)    │          │  (TCP MQTT)   │
   │  port: 1883   │          │  port: 1884   │          │  port: 1885   │
   └───────┬───────┘          └───────┬───────┘          └───────┬───────┘
           │                           │                           │
           └───────────────────────────┼───────────────────────────┘
                                       │
                              ┌────────┴────────┐
                              │   MqttBroker    │  ← 共享 Broker 实例
                              └────────┬────────┘
                                       │
                    ┌──────────────────┼──────────────────┐
                    │                  │                  │
                    ▼                  ▼                  ▼
        ┌─────────────────┐ ┌───────────────┐ ┌─────────────────┐
        │  MqttServerPlugin│ │ MqttMessage   │ │  Stats Snapshot │
        │  (连接认证/跟踪) │ │ DiscardPlugin │ │  (每分钟写入)   │
        └─────────────────┘ └───────────────┘ └─────────────────┘
```

---

## 技术栈

### 服务端 (TSMQServer)

| 技术 | 版本 | 说明 |
|------|------|------|
| .NET | 10.0 | 目标框架，支持 AOT 编译发布 |
| TouchSocket | 4.x | 核心通信框架，提供 HttpDmtpService / WebApi / DMTP RPC / WebSocket JsonRpc |
| TouchSocket.TORM | 2.x | 轻量级 ORM 框架，自动建表 |
| Microsoft.Data.Sqlite | - | SQLite 数据库驱动 |
| System.IdentityModel.Tokens.Jwt | 8.x | JWT 令牌生成与验证 |
| NLog | 6.x | 高性能日志框架，按天滚动 |
| System.Text.Json | - | JSON 序列化（源码生成器模式，AOT 兼容） |
| Microsoft.Extensions.Hosting | - | 通用主机，支持 Windows 服务部署 |

### 桌面客户端 (TSMQClient)

| 技术 | 说明 |
|------|------|
| .NET 10.0 | 目标框架 |
| WPF | Windows 原生桌面框架 |
| Avalonia 12.0 | 跨平台桌面框架（Windows / macOS / Linux） |
| WinForms | Windows 经典桌面框架 |
| CommunityToolkit.Mvvm | MVVM 框架（源生成器模式） |
| Baboon | 桌面应用宿主框架 |

### Web 前端 (TSMQWeb)

| 技术 | 版本 | 说明 |
|------|------|------|
| Vue | 3.4 | 渐进式 JavaScript 框架 |
| TypeScript | 6.0 | 类型安全的 JavaScript 超集 |
| Vite | 7.3 | 下一代前端构建工具 |
| Element Plus | 2.5 | Vue 3 UI 组件库 |
| Pinia | - | Vue 状态管理 |
| Vue Router | - | SPA 路由管理 |
| Axios | - | HTTP 请求客户端 |
| ECharts | 6 | 数据可视化图表库 |

---

## 项目结构

```
TSMQ/
├── Directory.Build.props                    # 全局 MSBuild 配置（版本号、目标框架、AOT 等）
├── LICENSE
├── README.md
├── 快速使用指南.md                          # 手把手从零搭建指南
├── 如何新增一个接口.md                      # 新增接口的标准流程文档
├── Clean-BuildArtifacts.cmd                # 清理构建产物脚本
│
├── TSMQServer/                             # 服务端解决方案
│   ├── TSMQServer.App/                     # 启动入口
│   │   ├── Program.cs                      #   主程序：配置主机、注册服务、启动 HttpDmtpService
│   │   ├── appsettings.json                #   配置文件（端口、JWT 密钥等）
│   │   ├── Plugins/                        #   插件（TsmqWebPlugin、SwaggerPlugin）
│   │   ├── TSMQServer.App.csproj
│   │   └── nlog.config                     #   NLog 配置
│   │
│   ├── TSMQServer.Core/                    # 契约层（接口定义 + DTO 模型）
│   │   ├── Services/                       #   RPC 服务接口定义
│   │   │   ├── IUserManagerService.cs      #     用户管理服务接口
│   │   │   ├── IRoleManagerService.cs      #     角色管理服务接口
│   │   │   ├── IRpcAuthorizationService.cs #     RPC 动态授权服务接口
│   │   │   ├── ILogManagerService.cs       #     日志管理服务接口
│   │   │   ├── IMqttServerManagerService.cs#     MQTT 服务器管理接口
│   │   │   ├── IMqttClientManagerService.cs#     MQTT 客户端管理接口
│   │   │   └── IMqttTopicManagerService.cs #     MQTT 主题管理接口
│   │   ├── Models/                         #   DTO 数据传输对象
│   │   │   ├── RequestBase.cs              #     请求基类
│   │   │   ├── LoginedRequest.cs           #     需登录的请求基类（带 LoginedToken）
│   │   │   ├── ResponseBase.cs             #     响应基类
│   │   │   ├── UserInfo.cs                 #     用户信息模型
│   │   │   ├── RoleInfo.cs                 #     角色信息模型
│   │   │   └── ...                         #     其他 DTO 模型
│   │   ├── Json/                           #   JSON 序列化上下文（AOT 兼容）
│   │   └── TSMQServer.Core.csproj
│   │
│   ├── TSMQServer.Core.Imp/                # 服务实现层
│   │   ├── Services/                       #   所有服务实现
│   │   │   ├── Jwt/                        #     JWT 令牌生成与验证
│   │   │   ├── UserManager/                #     用户管理实现
│   │   │   ├── RoleManager/                #     角色管理实现
│   │   │   ├── RpcAuthorization/           #     RPC 动态授权实现
│   │   │   ├── LogManager/                 #     日志管理实现（NLog 文件读取器）
│   │   │   ├── Mqtt/                       #     MQTT 服务实现
│   │   │   │   ├── MqttServerHostService.cs#       MQTT 宿主服务（IHostedService）
│   │   │   │   ├── MqttServerManager/      #       服务器管理
│   │   │   │   ├── MqttClientManager/      #       客户端管理
│   │   │   │   └── MqttTopicManager/       #       主题管理
│   │   │   └── ...
│   │   ├── Attributes/                     #   过滤器特性
│   │   │   ├── AuthorizeAttribute.cs       #     鉴权过滤器（JWT + 角色）
│   │   │   ├── AllowAnonymousAttribute.cs  #     匿名访问标记
│   │   │   ├── ExceptionFilterAttribute.cs #     异常过滤器
│   │   │   └── LoggerFilterAttribute.cs    #     日志过滤器
│   │   └── Extensions/                     #   依赖注入扩展方法
│   │
│   ├── TSMQServer.Db/                      # 数据库抽象接口层
│   │   ├── IUserManagerFromDB.cs           #   用户数据访问接口
│   │   ├── IRoleManagerFromDB.cs           #   角色数据访问接口
│   │   ├── ILoginSessionFromDB.cs          #   登录会话数据访问接口
│   │   ├── IRpcMethodRoleFromDB.cs         #   RPC 授权数据访问接口
│   │   └── ...                             #   其他数据库接口
│   │
│   ├── TSMQServer.Db.Sqlite/               # SQLite 数据库实现
│   │   ├── SqliteDbContext.cs              #   数据库上下文（连接管理）
│   │   ├── SqliteDbInitializer.cs          #   自动建表 + 种子数据 + 默认授权配置
│   │   ├── Extensions/                     #   DI 扩展方法
│   │   └── Implementations/                #   各数据库接口的 SQLite 实现
│   │
│   └── TSMQServer.Web/                     # Web 前端嵌入层
│       ├── build/                          #   Vue 构建产物（嵌入资源）
│       ├── Plugins/TsmqWebPlugin.cs         #   静态文件服务 + SPA 路由回退
│       └── Extensions/                     #   DI 扩展
│
├── TSMQClient/                             # 桌面客户端解决方案
│   ├── Core/
│   │   ├── TSMQClient.Core/                #   核心抽象层
│   │   │   └── Services/Remote/
│   │   │       ├── IRemoteService.cs       #     远程服务主接口
│   │   │       ├── IRemoteService.UserManager.cs
│   │   │       ├── IRemoteService.RoleManager.cs
│   │   │       └── ...                     #     各模块接口扩展文件
│   │   │
│   │   └── TSMQClient.Mvvm/                #   共享 ViewModel（UI 无关）
│   │       └── ViewModels/                 #     各页面对应的 ViewModel
│   │
│   ├── Modules/
│   │   └── TSMQClient.Remote.Module/       #   远程服务模块
│   │       └── Services/Remote/
│   │           ├── WebApi/                 #     WebApi 远程服务实现
│   │           │   ├── WebApiRemoteService.cs
│   │           │   └── WebApiRemoteService.*.cs
│   │           ├── Dmtp/                   #     DMTP RPC 远程服务实现
│   │           │   ├── DmtpRemoteService.cs
│   │           │   └── DmtpRemoteService.*.cs
│   │           └── JsonRpc/                #     WebSocket JsonRpc 远程服务实现
│   │               ├── JsonRpcRemoteService.cs
│   │               └── JsonRpcRemoteService.*.cs
│   │
│   ├── UI/
│   │   ├── TSMQClient.Wpf/                 #   WPF 桌面客户端
│   │   ├── TSMQClient.Avalonia/            #   Avalonia 跨平台桌面客户端
│   │   └── TSMQClient.Winform/              #   WinForms 桌面客户端
│   │
│   └── Tests/                              #   单元测试项目
│
└── TSMQWeb/                                # Web 前端项目
    ├── package.json                        #   项目配置与依赖
    ├── vite.config.ts                      #   Vite 构建配置
    ├── tsconfig.json                       #   TypeScript 配置
    ├── index.html                          #   HTML 入口
    └── src/
        ├── App.vue                         #   根组件
        ├── main.ts                         #   入口文件
        ├── api/
        │   └── index.ts                    #   所有 API 接口封装（统一管理）
        ├── router/
        │   └── index.ts                    #   路由配置（含鉴权守卫）
        ├── stores/
        │   ├── user.ts                     #   用户状态管理
        │   └── app.ts                      #   应用全局状态
        ├── layout/
        │   └── MainLayout.vue              #   主布局（侧边栏 + 顶栏 + 内容区）
        ├── utils/
        │   ├── request.ts                  #   Axios 封装（Token 注入、错误处理）
        │   └── logger.ts                   #   前端日志工具
        └── views/
            ├── Login.vue                   #   登录页面
            ├── Dashboard.vue               #   仪表盘（ECharts 统计图表）
            ├── user/
            │   └── UserList.vue            #   用户管理
            ├── role/
            │   └── RoleList.vue            #   角色管理
            ├── rpc-authorization/          #   RPC 动态角色授权管理
            ├── LogViewer.vue               #   系统日志查看器
            └── mqtt/                       #   MQTT 管理
                ├── MqttServerList.vue      #     服务器列表
                ├── MqttClientList.vue      #     在线客户端
                ├── MqttTopicList.vue       #     主题监控
                └── MqttSettings.vue        #     Broker 参数配置
```

---

## 快速开始

### 环境要求

| 软件 | 版本要求 | 说明 |
|------|----------|------|
| .NET SDK | 10.0+ | [下载](https://dotnet.microsoft.com/download/dotnet/10.0) |
| Node.js | 18+ | [下载](https://nodejs.org/)（仅 Web 前端需要） |
| 操作系统 | Windows / macOS / Linux | |
| IDE | Visual Studio 2022+ / JetBrains Rider（可选） | |

### 第一步：启动服务端

打开终端，进入服务端目录并启动：

```bash
cd TSMQServer
dotnet run --project TSMQServer.App
```

启动成功后，终端将显示：

```
info: TouchSocket.Sockets.HttpDmtpService[0]
      The server is listening on port: 7789
```

> 服务端默认监听 **7789** 端口。首次启动时会自动创建 SQLite 数据库文件并初始化默认数据。
>
> **启动后不要关闭此终端窗口**，后续客户端和前端都需要连接它。

### 第二步：启动 Web 前端

打开一个新的终端窗口：

```bash
cd TSMQWeb

# 首次运行需安装依赖（后续无需重复执行）
npm install

# 启动开发服务器
npm run dev
```

浏览器访问 **http://localhost:3000** 即可看到登录页面。

> Vite 开发服务器会自动将 `/api` 请求代理到后端 `7789` 端口，无需额外配置。

### 第三步：启动桌面客户端（可选，三选一）

打开一个新终端窗口：

```bash
cd TSMQClient

# 方式 A：WPF 客户端（推荐 Windows 用户）
dotnet run --project UI/TSMQClient.Wpf

# 方式 B：Avalonia 客户端（跨平台）
dotnet run --project UI/TSMQClient.Avalonia

# 方式 C：WinForms 客户端
dotnet run --project UI/TSMQClient.Winform
```

### 默认账号

| 账号 | 密码 | 角色 |
|------|------|------|
| `admin` | `admin123` | 管理员 (admin) |

### 启动顺序速查

```
终端 1 → 启动服务端
   cd TSMQServer && dotnet run --project TSMQServer.App

终端 2 → 启动桌面客户端（可选）
   cd TSMQClient && dotnet run --project UI/TSMQClient.Wpf

终端 3 → 启动 Web 前端
   cd TSMQWeb && npm run dev
```

三个终端窗口保持打开即可正常使用。

---

## API 接口文档

所有接口均通过 HTTP POST 方法暴露，URL 路径格式为：`/I{ServiceName}Service/{MethodName}`。

> 需鉴权的接口请在请求体中携带 `LoginedToken` 字段（登录成功后返回的 JWT Token）。

### 用户管理 (`IUserManagerService`)

| 接口路径 | 方法 | 鉴权 | 说明 |
|----------|------|------|------|
| `/IUserManagerService/Login` | POST | 否 | 用户登录，返回 JWT Token |
| `/IUserManagerService/Register` | POST | 否 | 用户注册 |
| `/IUserManagerService/GetUserList` | POST | 是 | 获取用户列表 |
| `/IUserManagerService/GetUserInfo` | POST | 是 | 获取当前登录用户信息 |
| `/IUserManagerService/ChangePassword` | POST | 是 | 修改密码 |
| `/IUserManagerService/AssignUserRoles` | POST | 是 | 分配用户角色 |
| `/IUserManagerService/DeleteUser` | POST | 是 | 删除用户 |

### 角色管理 (`IRoleManagerService`)

| 接口路径 | 方法 | 鉴权 | 说明 |
|----------|------|------|------|
| `/IRoleManagerService/CreateRole` | POST | 是 | 创建角色 |
| `/IRoleManagerService/UpdateRole` | POST | 是 | 更新角色信息 |
| `/IRoleManagerService/DeleteRole` | POST | 是 | 删除角色 |
| `/IRoleManagerService/GetRoleList` | POST | 是 | 获取角色列表 |

### RPC 动态授权 (`IRpcAuthorizationService`)

| 接口路径 | 方法 | 鉴权 | 说明 |
|----------|------|------|------|
| `/IRpcAuthorizationService/GetAllRpcMethods` | POST | 是 | 获取所有已注册的 RPC 方法列表 |
| `/IRpcAuthorizationService/GetAllRpcRoles` | POST | 是 | 获取所有 RPC 授权配置 |
| `/IRpcAuthorizationService/GetRpcRoles` | POST | 是 | 获取指定目标（方法/类）的授权配置 |
| `/IRpcAuthorizationService/SetRpcRoles` | POST | 是 | 设置指定目标的角色授权 |
| `/IRpcAuthorizationService/RemoveRpcRoles` | POST | 是 | 移除指定目标的授权配置 |

### 日志管理 (`ILogManagerService`)

| 接口路径 | 方法 | 鉴权 | 说明 |
|----------|------|------|------|
| `/ILogManagerService/GetLogList` | POST | 是 | 按日志级别、分类、关键词、时间范围查询服务端日志 |

### MQTT 服务器管理 (`IMqttServerManagerService`)

| 接口路径 | 方法 | 鉴权 | 说明 |
|----------|------|------|------|
| `/IMqttServerManagerService/GetMqttServerList` | POST | 是 | 获取 MQTT 服务器列表 |
| `/IMqttServerManagerService/AddMqttServer` | POST | 是 | 添加 MQTT 监听服务器 |
| `/IMqttServerManagerService/StartMqttServer` | POST | 是 | 启动指定 MQTT 服务器 |
| `/IMqttServerManagerService/StopMqttServer` | POST | 是 | 停止指定 MQTT 服务器 |
| `/IMqttServerManagerService/RemoveMqttServer` | POST | 是 | 移除 MQTT 服务器 |
| `/IMqttServerManagerService/GetMqttAuthUsers` | POST | 是 | 获取 MQTT 连接授权用户列表 |
| `/IMqttServerManagerService/AddMqttAuthUser` | POST | 是 | 添加 MQTT 连接授权用户 |
| `/IMqttServerManagerService/UpdateMqttAuthUser` | POST | 是 | 更新 MQTT 授权用户信息 |
| `/IMqttServerManagerService/DeleteMqttAuthUser` | POST | 是 | 删除 MQTT 授权用户 |
| `/IMqttServerManagerService/GetMqttBrokerSettings` | POST | 是 | 获取 Broker 参数配置 |
| `/IMqttServerManagerService/SetMqttBrokerSettings` | POST | 是 | 设置 Broker 参数配置 |
| `/IMqttServerManagerService/GetMqttDiscardedStats` | POST | 是 | 获取消息丢弃统计 |
| `/IMqttServerManagerService/ResetMqttDiscardedStats` | POST | 是 | 重置消息丢弃统计 |
| `/IMqttServerManagerService/GetMqttDashboardStats` | POST | 是 | 获取仪表盘实时统计数据 |
| `/IMqttServerManagerService/GetDashboardStatsHistory` | POST | 是 | 获取统计快照历史（按时间范围查询） |
| `/IMqttServerManagerService/GetMqttSslCerts` | POST | 是 | 获取 SSL 证书列表 |
| `/IMqttServerManagerService/AddMqttSslCert` | POST | 是 | 添加 SSL 证书（PFX/PEM） |
| `/IMqttServerManagerService/DeleteMqttSslCert` | POST | 是 | 删除 SSL 证书 |

### MQTT 客户端管理 (`IMqttClientManagerService`)

| 接口路径 | 方法 | 鉴权 | 说明 |
|----------|------|------|------|
| `/IMqttClientManagerService/GetMqttClientList` | POST | 是 | 获取所有在线 MQTT 客户端列表 |
| `/IMqttClientManagerService/KickMqttClients` | POST | 是 | 强制踢除指定 MQTT 客户端 |

### MQTT 主题管理 (`IMqttTopicManagerService`)

| 接口路径 | 方法 | 鉴权 | 说明 |
|----------|------|------|------|
| `/IMqttTopicManagerService/GetMqttTopicList` | POST | 是 | 获取 MQTT 主题列表 |
| `/IMqttTopicManagerService/GetMqttTopicStats` | POST | 是 | 获取主题消息收发统计 |
| `/IMqttTopicManagerService/ResetMqttTopicStats` | POST | 是 | 重置主题统计 |

---

## Swagger 文档

Debug 模式下启动服务端后，浏览器会自动打开 Swagger 页面：

```
http://localhost:7789/swagger
```

在 Swagger 页面中可以浏览所有接口的完整文档，并直接在线调试。

> Swagger 仅 **Debug** 模式编译时可用。使用 `dotnet run` 默认为 Debug 模式。

---

## 数据库设计

项目使用 **SQLite** 单文件数据库（`TSMQ.db`），首次启动时自动创建并初始化。数据库文件位于应用程序运行目录下，拷贝即可迁移。

### 数据表清单

| 表名 | 说明 | 主要字段 |
|------|------|----------|
| `Users` | 用户表 | Id, Username, Password(哈希), Nickname, Email, Phone, RoleId, IsEnabled |
| `Roles` | 角色表 | Id, RoleName, RoleCode, Description, IsEnabled |
| `LoginSessions` | 登录会话表 | Id, UserId, Token, LoginTime, ExpireTime |
| `RpcMethodRoles` | RPC 方法角色授权映射表 | Id, TargetKey(方法/类), RoleCode, Description |
| `MqttServers` | MQTT 服务器配置表 | Id, Name, Type(TCP/WS), Port, IsEnabled, SslCertId |
| `MqttAuthUsers` | MQTT 连接授权用户表 | Id, Username, Password, IsEnabled, ServerId |
| `MqttSslCerts` | SSL 证书表 | Id, Name, CertType(PFX/PEM), CertData, Password |
| `MqttStatsSnapshots` | 统计快照表 | Id, SnapshotTime, OnlineClients, ActiveTopics, RunningServers, MessagesIn, MessagesOut |
| `BrokerSettings` | Broker 参数配置表 | Id, MaxMessageCapacity, MessageExpireMinutes, SessionExpireMinutes |

### 种子数据

- **管理员用户**：用户名 `admin`，密码 `admin123`，角色 `admin`
- **管理员角色**：角色编码 `admin`，角色名 "管理员"
- **默认 RPC 授权**：所有管理接口默认授权给 `admin` 角色

---

## 配置说明

### `appsettings.json` 完整配置

文件位置：`TSMQServer/TSMQServer.App/appsettings.json`

```json
{
  "Server": {
    "Port": 7789,
    "DmtpVerifyToken": "Dmtp"
  },
  "Jwt": {
    "SecretKey": "TSMQServerSecretKey12345678901234567890",
    "Issuer": "TSMQServer",
    "Audience": "TSMQClient",
    "ExpirationMinutes": 480
  }
}
```

### 配置项说明

| 配置项 | 类型 | 默认值 | 说明 |
|--------|------|--------|------|
| `Server.Port` | int | `7789` | HTTP/DMTP/WebSocket 统一监听端口 |
| `Server.DmtpVerifyToken` | string | `"Dmtp"` | DMTP RPC 连接验证令牌 |
| `Jwt.SecretKey` | string | 随机字符串 | JWT 签名密钥（生产环境请更换） |
| `Jwt.Issuer` | string | `"TSMQServer"` | JWT 签发者 |
| `Jwt.Audience` | string | `"TSMQClient"` | JWT 受众 |
| `Jwt.ExpirationMinutes` | int | `480` | JWT 过期时间（分钟），默认 8 小时 |

### 修改端口

如果 7789 端口被占用，可修改 `appsettings.json` 中的 `Server.Port` 值。同时需要修改前端代理配置：
- **Web 前端**：修改 `TSMQWeb/vite.config.ts` 中 `proxy` 目标的端口号
- **桌面客户端**：修改对应客户端项目中的连接地址

---

## 构建与部署

### 开发环境构建

```bash
# 服务端编译
cd TSMQServer
dotnet build

# 桌面客户端编译
cd TSMQClient
dotnet build

# Web 前端开发
cd TSMQWeb
npm run dev
```

### 生产发布

#### 服务端发布（单文件可执行文件）

```bash
cd TSMQServer
dotnet publish TSMQServer.App -c Release -o ../publish/server
```

发布产物位于 `publish/server/` 目录，运行 `TSMQServer.App.exe` 即可启动。支持 AOT 编译发布（已配置 `IsAotCompatible=true`）。

#### Web 前端构建（静态文件）

```bash
cd TSMQWeb
npm run build
```

构建产物位于 `TSMQWeb/dist/` 目录，可部署到 Nginx、IIS 等任意静态文件服务器。

如果要将 Web 前端嵌入到服务端中发布，将 `dist/` 目录内容复制到 `TSMQServer/TSMQServer.Web/build/` 目录，然后发布服务端即可。服务端启动后会通过 `TsmqWebPlugin` 自动提供静态文件服务。

#### 桌面客户端发布

```bash
# WPF 客户端
dotnet publish TSMQClient/UI/TSMQClient.Wpf -c Release -o ../publish/client-wpf

# Avalonia 客户端
dotnet publish TSMQClient/UI/TSMQClient.Avalonia -c Release -o ../publish/client-avalonia

# WinForms 客户端
dotnet publish TSMQClient/UI/TSMQClient.Winform -c Release -o ../publish/client-winform
```

### Docker 部署

项目支持 Docker 多阶段构建：

```dockerfile
# Stage 1: 构建前端
FROM node:22 AS web-builder
WORKDIR /src
COPY TSMQWeb/ .
RUN npm install && npm run build

# Stage 2: 构建并发布 .NET 服务端
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder
WORKDIR /src
COPY . .
COPY --from=web-builder /src/dist ./TSMQServer/TSMQServer.Web/build
RUN dotnet publish TSMQServer/TSMQServer.App -c Release -o /app

# Stage 3: 运行镜像
FROM mcr.microsoft.com/dotnet/runtime-deps:10.0
WORKDIR /app
COPY --from=builder /app .
ENTRYPOINT ["./TSMQServer.App"]
```

### 作为 Windows 服务运行

```bash
# 创建服务
sc create TSMQService binPath="完整路径/TSMQServer.App.exe"

# 启动服务
sc start TSMQService

# 停止服务
sc stop TSMQService

# 删除服务
sc delete TSMQService
```

---

## 客户端开发

### Web 前端开发

Web 前端使用 Vue 3 + TypeScript + Vite 构建，开发时通过 Vite 代理将 API 请求转发到服务端。

关键目录说明：

| 目录/文件 | 说明 |
|-----------|------|
| `src/api/index.ts` | 所有 API 接口定义，统一管理 |
| `src/utils/request.ts` | Axios 封装，自动注入 `LoginedToken`，统一错误处理 |
| `src/router/index.ts` | 路由配置，含鉴权守卫（登录状态检查） |
| `src/stores/` | Pinia 状态管理（用户状态、应用状态） |
| `src/layout/MainLayout.vue` | 主布局（侧边栏菜单 + 顶部导航 + 内容区域） |
| `src/views/` | 各功能页面组件 |

### 桌面客户端开发

桌面客户端基于 MVVM 架构，核心分层如下：

```
┌──────────────────────────────┐
│  UI 层 (WPF / Avalonia / WinForms)  │  ← 视图（XAML / AXAML / Designer）
├──────────────────────────────┤
│  ViewModel 层 (TSMQClient.Mvvm)     │  ← 业务逻辑与状态管理（UI 无关）
├──────────────────────────────┤
│  IRemoteService 抽象层 (TSMQClient.Core) │  ← 远程调用接口定义
├──────────────────────────────┤
│  通信实现层 (TSMQClient.Remote.Module) │  ← WebApi / DMTP / JsonRpc 三套实现
└──────────────────────────────┘
```

**协议一行切换**：

```csharp
// 使用 DMTP RPC（高性能长连接）
e.Services.UseDmtpRemote();

// 使用 WebApi（标准 HTTP）
e.Services.UseWebApiRemote();

// 使用 WebSocket JsonRpc
e.Services.UseJsonRpcRemote();
```

切换协议后，所有 ViewModel 的业务代码无需任何改动。

---

## 如何新增一个接口

完整的数据流向和新增接口涉及的文件如下：

### 涉及文件速查表

```
新增一个完整接口需要改动以下文件：

服务端（5 步）：
  1. 定义 DTO 模型（请求 + 响应）         → TSMQServer.Core/Models/
  2. 注册 JSON 序列化                     → TSMQServer.Core/Json/...JsonSerializerContext.cs
  3. 定义 RPC 服务接口                    → TSMQServer.Core/Services/I{Name}Service.cs
  4. 实现服务逻辑 + 日志定义               → TSMQServer.Core.Imp/Services/{Name}/
  5. 配置默认授权（可选）                  → TSMQServer.Db.Sqlite/SqliteDbInitializer.cs

桌面客户端（2 步）：
  6. 扩展 IRemoteService 接口            → TSMQClient.Core/Services/Remote/IRemoteService.{Name}.cs
  7. 实现三种协议远程服务                 → TSMQClient.Remote.Module/Services/Remote/{Protocol}/

Web 前端（2 步）：
  8. 添加 API 调用                       → TSMQWeb/src/api/index.ts
  9. 创建页面 + 注册路由 + 添加菜单       → TSMQWeb/src/views/ + router + layout
```

详细的步骤说明和代码示例请参考：[如何新增一个接口.md](如何新增一个接口.md)

---

## 常见问题

### Q1：启动服务端报错 "端口 7789 已被占用"

7789 端口被其他程序占用。解决方法：
- 关闭占用该端口的程序
- 或修改端口：打开 `TSMQServer/TSMQServer.App/appsettings.json`，将 `Server.Port` 改为其他端口号

### Q2：桌面客户端连不上服务端

请依次检查：
- 服务端是否已启动且正常监听
- 客户端连接地址是否为 `127.0.0.1:7789`（远程部署需修改为实际 IP）
- 防火墙是否阻止了 7789 端口的连接

### Q3：Web 前端登录报错

请依次检查：
- 服务端是否已启动且正常监听
- 浏览器控制台（F12）中是否有红色错误信息
- Vite 开发服务器是否启动成功

### Q4：忘记管理员密码怎么办？

删除项目目录下的 `TSMQ.db` 数据库文件，重启服务端将自动重新创建数据库并初始化默认账号（`admin` / `admin123`）。

### Q5：如何迁移到其他数据库（MySQL / SQL Server 等）？

参照 `TSMQServer.Db.Sqlite` 项目的实现，新建一个数据库实现项目，实现 `TSMQServer.Db` 中定义的数据库接口即可。架构已通过接口层解耦，核心业务代码无需修改。

### Q6：支持哪些 MQTT 客户端连接？

任何标准 MQTT 3.1.1 / 5.0 协议的客户端均可连接。推荐使用 [MQTTX](https://mqttx.app/)、[MQTT Explorer](http://mqtt-explorer.com/)、Eclipse Paho 等客户端进行测试。

### Q7：如何配置 MQTT SSL/TLS 加密连接？

在管理界面中，导航到 MQTT 管理 → 证书管理，上传 PFX 或 PEM 格式的 SSL 证书，然后在添加 MQTT 服务器时选择对应的证书即可。

---

## 为什么选择 TSMQ？

市面上已有大量成熟的后台管理系统和权限框架（如 若依、ABP 等），它们功能全面但往往带来 **学习成本高、项目臃肿、部署复杂** 的问题。TSMQ 的定位是：**最容易上手、最容易改造、最轻量级**。

### 核心优势

**1. MQTT Broker 内置管理**

TSMQ 不只是一个权限系统，更内置了完整的 MQTT Broker 管理能力——多端口监听、连接认证、SSL 证书、主题监控、消息统计、历史快照等一应俱全。如果你在做 IoT 或需要 MQTT 消息中间件的项目，一个项目即可解决权限管理 + MQTT 管理的全部需求。

**2. 一套接口，三种协议，一行切换**

大多数系统只提供 HTTP REST API。TSMQ 基于 TouchSocket 的 RPC 能力，同一套服务接口同时支持 **WebApi（HTTP）、DMTP RPC、WebSocket JsonRpc** 三种调用方式。客户端一行代码即可切换：

```csharp
e.Services.UseDmtpRemote();  // 高性能长连接（桌面应用）
e.Services.UseWebApiRemote(); // 标准 HTTP（Web 前端）
```

**3. 极致轻量，零外部依赖**

- 服务端仅依赖 **TouchSocket + SQLite + NLog**，无需 Redis、EF Core、消息队列等重型中间件
- 单文件数据库（`TSMQ.db`），拷贝即可迁移，无需安装数据库服务
- 支持 **AOT 发布**，编译产物体积极小，适合嵌入式和边缘部署场景

**4. 真正的项目模版**

- 代码结构清晰：Core（接口定义）→ Core.Imp（实现）→ Db（数据访问）**三层严格分离**
- 新增接口只需 9 个文件，有完整的开发指南文档
- 全局项目配置统一管理（`Directory.Build.props`），版本号、目标框架等一处修改全局生效

**5. 动态 RPC 授权，运行时即时生效**

每个 RPC 方法的角色映射存储在数据库中，通过管理界面修改后 **立即生效，无需重启服务**。支持方法级（`ClassName.MethodName`）和类级（`ClassName`）两种粒度的授权配置。

**6. 桌面 + Web 双端完整演示**

同时提供 **WPF、Avalonia、WinForms** 三种桌面客户端和 **Vue 3** Web 前端。客户端通过 `IRemoteService` 接口抽象远程调用，UI 层与通信层完全解耦，可快速切换 UI 框架。

**7. 学习 TouchSocket RPC 的最佳实战**

如果你正在学习 TouchSocket 框架或想在项目中使用其 RPC 能力，TSMQ 是一个完整、可运行的生产级示例，覆盖了 WebApi、DMTP、JsonRpc 三种 RPC 的定义、实现、鉴权、序列化等全部最佳实践。

### 适用场景

| 场景 | 说明 |
|------|------|
| IoT / MQTT 项目后台 | 内置 MQTT Broker，管理 MQTT 服务器、客户端、主题、连接授权 |
| 小型后台管理系统 | 功能够用、不过度设计、开箱即用 |
| 桌面应用后端 | 需要长连接 RPC 而非纯 HTTP 的桌面应用 |
| 工控上位机 | 轻量级、支持 AOT、单文件部署 |
| TouchSocket 学习 | 完整的 RPC 实战参考项目 |
| 二次开发基础 | 代码结构清晰，快速搭建自己的系统 |

---

## 项目依赖精简版

### 生产依赖

```
TSMQServer.App
  ├── TouchSocket (核心通信框架)
  │   ├── TouchSocket.Dmtp (DMTP RPC)
  │   ├── TouchSocket.WebApi (HTTP WebApi)
  │   └── TouchSocket.JsonRpc (WebSocket JsonRpc)
  ├── TouchSocket.TORM (轻量级 ORM)
  ├── Microsoft.Data.Sqlite (SQLite 驱动)
  ├── System.IdentityModel.Tokens.Jwt (JWT)
  ├── NLog.Extensions.Logging (日志)
  └── Microsoft.Extensions.Hosting.WindowsServices (Windows 服务)

TSMQWeb (前端)
  ├── vue 3.4 + vue-router + pinia
  ├── element-plus (UI 组件库)
  ├── echarts (数据图表)
  ├── axios (HTTP 客户端)
  └── typescript + vite
```

---

## 相关链接

| 资源 | 地址 |
|------|------|
| TouchSocket 官网 | [https://touchsocket.net/](https://touchsocket.net/) |
| TouchSocket GitHub | [https://github.com/RRQM/TouchSocket](https://github.com/RRQM/TouchSocket) |
| 作者 CSDN 博客 | [https://blog.csdn.net/qq_40374647](https://blog.csdn.net/qq_40374647) |
| 作者哔哩哔哩 | [https://space.bilibili.com/94253567](https://space.bilibili.com/94253567) |
| 技术交流 QQ 群 | [234762506](https://jq.qq.com/?_wv=1027&k=5bgUO6cI) |

---

## 开源协议

本项目采用 [Apache License 2.0](LICENSE) 开源协议，允许自由使用、修改和分发。

---

<p align="center">
  <b>TSMQ</b> — 一个项目，既是权限管理系统，也是 MQTT Broker 管理器<br>
  Made with ❤️ by <a href="https://touchsocket.net/">若汝棋茗</a>
</p>
