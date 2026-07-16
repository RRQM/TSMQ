---
agent: agent
---

目的：
本提示词集合用于在 `TouchAdminServer.Core` 项目中新增 RPC 接口时，生成接口定义、请求/响应 DTO、实现骨架、序列化上下文更新、授权配置片段与单元测试模板。所有输出应遵循项目约定（接口标注、DTO 继承、目标键规则、JsonSerializerContext 注册等）。

约定摘要：
- 接口：添加于 `TouchAdminServer.Core` 下的 `Services` 命名空间或子命名空间，接口上使用 `[GeneratorRpcProxy]`（若为新服务）、方法上应标注 `[WebApi(Method = HttpMethodType.Post)]`、`[DmtpRpc]` 与 `[Description("...")]`，接口继承 `IRpcServer`。
- 请求 DTO 继承 `RequestBase`；响应 DTO 继承 `ResponseBase` 或自定义包含数据的响应类。
- 目标键：类级为 `ServiceName`，方法级为 `ServiceName.MethodName`。
- 授权：使用 `RpcMethodRoleInfo` 记录 `{ TargetKey, RoleCodes, Description }`，空数组或 `null` 表示移除限制。
- 序列化：在 `TouchAdminServer.Core/Json/TouchAdminServerCoreJsonSerializerContext.cs` 中添加 `[JsonSerializable(typeof(...))]` 声明以支持新的 DTO。
- 服务和 DTO 命名空间不再按模块细分，统一使用 `TouchAdminServer.Core.Services`。

Prompt A - 生成接口定义（单方法或多方法接口）
说明：生成 C# 接口代码片段，遵循项目约定；包含必要的 `using`，接口和方法 XML 注释，方法应返回 `Task<YourResponse>` 或 `Task<ResponseBase>`。
必填信息：`ServiceName`、`MethodName`、`MethodDescription`、`RequestDto`、`ResponseDto`、`是否类级注解[GeneratorRpcProxy]`。
输出格式：可直接粘贴到 `TouchAdminServer.Core/Services/` 下的文件，示例路径：TouchAdminServer/TouchAdminServer.Core/Services/YourService/IYourService.cs。

Prompt B - 生成请求/响应 DTO
说明：生成 DTO 类，`Request` 继承 `RequestBase`，`Response` 继承 `ResponseBase` 或自定义响应类。包含属性、XML 注释和空值注解（如需）。同时返回需在 `TouchAdminServerCoreJsonSerializerContext.cs` 中注册的类型列表。
必填信息：`DtoName`、字段列表（`Name:Type:说明:是否可空`）。
输出格式：完整类代码与 `[JsonSerializable(typeof(...))]` 建议行。

Prompt C - 生成实现骨架
说明：在 `TouchAdminServer.Core.Imp` 命名空间下生成服务实现类骨架，包含构造函数注入（常见依赖示例：`IRoleManagerService`、`IUserManagerService`）、方法体占位实现（抛出 `NotImplementedException` 或返回占位 `ResponseBase`）。如涉及到需要在方法注册 `RpcMethodInfo` 的示例，请生成 `RpcMethodInfo` 填充示例（`ServiceName`、`MethodName`、`ClassTargetKey`、`MethodTargetKey`、`Description`）。
必填信息：`ImplClassName`、所需依赖列表、方法行为简述。
输出格式：可直接写入 `TouchAdminServer.Core.Imp/Services/...` 的实现文件。

Prompt D - 序列化上下文更新补丁说明
说明：为新增 DTO 生成应加入 `TouchAdminServerCoreJsonSerializerContext.cs` 的 `[JsonSerializable(typeof(...))]` 列表，按类型与集合类型列出，并给出插入建议位置（按文件中现有分组位置）。
必填信息：需要注册的类型数组。
输出格式：可直接用于 `apply_patch` 的添加行文本与目标文件路径提示。

复合示例 Prompt（一次性提交给生成器）
说明：将以下内容按顺序提供给 AI：
1) 项目约定（简要列出上文“约定摘要”）
2) 要生成的项（接口/DTO/实现/JsonContext/授权示例/单元测试）
3) 具体命名（`ServiceName`、`MethodName`、DTO 名称与字段）
4) 放置路径（建议的目标文件路径）
5) 禁止修改项（不要更改现有接口签名或移除已有授权配置）
期望输出：分段的、可直接粘贴到仓库的代码片段，每段注注明确的目标文件路径与插入点。

使用示例（供参考，非代码粘贴）：
- 输入示例字段：ServiceName=UserService, MethodName=GetUserInfo, MethodDescription=获取用户信息, RequestDto=GetUserInfoRequest, ResponseDto=GetUserInfoResponse, Fields=UserId:long:用户ID:false
- 期望产生文件：
  - 接口片段 -> TouchAdminServer/TouchAdminServer.Core/Services/UserManager/IUserManagerService.cs
  - DTO -> TouchAdminServer/TouchAdminServer.Core/Services/UserManager/Dto/GetUserInfoRequest.cs
  - 实现骨架 -> TouchAdminServer/TouchAdminServer.Core.Imp/Services/UserManager/UserManagerService.cs
  - JsonContext 更新 -> TouchAdminServer/TouchAdminServer.Core/Json/TouchAdminServerCoreJsonSerializerContext.cs