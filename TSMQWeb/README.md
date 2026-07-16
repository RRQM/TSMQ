# TSMQ Web 前端

基于 Vue 3 + Vite + Element Plus 的权限管理系统前端

## 功能特性

- 用户管理：用户增删改查、角色分配
- 角色管理：角色增删改查、权限分配
- 权限管理：权限增删改查、树形结构
- 登录认证：JWT Token 认证
- 路由守卫：基于权限的路由拦截
- 权限指令：v-permission 自定义指令

## 技术栈

- Vue 3.4
- Vite 5.0
- Vue Router 4.2
- Pinia 2.1
- Element Plus 2.5
- Axios 1.6

## 目录结构

```
TSMQWeb/
├── src/
│   ├── api/              # API 接口
│   ├── directives/       # 自定义指令
│   ├── layout/           # 布局组件
│   ├── router/           # 路由配置
│   ├── stores/           # Pinia 状态管理
│   ├── utils/            # 工具函数
│   ├── views/            # 页面组件
│   │   ├── user/         # 用户管理
│   │   ├── role/         # 角色管理
│   │   └── permission/   # 权限管理
│   ├── App.vue
│   └── main.js
├── index.html
├── package.json
└── vite.config.js
```

## 快速开始

### 安装依赖

```bash
cd TSMQWeb
npm install
```

### 启动开发服务器

```bash
npm run dev
```

访问 http://localhost:3000

### 构建生产版本

```bash
npm run build
```

## API 接口说明

所有接口通过代理转发到后端服务器 (默认 http://localhost:7789)

### 用户管理接口
- `/IUserManagerService/Login` - 登录
- `/IUserManagerService/Register` - 注册

### 角色管理接口
- `/IRoleManagerService/CreateRole` - 创建角色
- `/IRoleManagerService/UpdateRole` - 更新角色
- `/IRoleManagerService/DeleteRole` - 删除角色
- `/IRoleManagerService/GetRoleList` - 获取角色列表
- `/IRoleManagerService/AssignRolePermissions` - 分配角色权限
- `/IRoleManagerService/AssignUserRoles` - 分配用户角色

### 权限管理接口
- `/IPermissionManagerService/CreatePermission` - 创建权限
- `/IPermissionManagerService/UpdatePermission` - 更新权限
- `/IPermissionManagerService/DeletePermission` - 删除权限
- `/IPermissionManagerService/GetPermissionList` - 获取权限列表
- `/IPermissionManagerService/CheckUserPermission` - 检查用户权限

## 权限控制

### 路由权限

在路由配置中通过 meta.permission 设置权限标识：

```javascript
{
  path: 'users',
  component: UserList,
  meta: { 
    title: '用户管理', 
    permission: 'user:view' 
  }
}
```

### 按钮权限

使用 v-permission 指令控制按钮显示：

```vue
<el-button v-permission="'user:add'">添加用户</el-button>
<el-button v-permission="'user:edit'">编辑</el-button>
<el-button v-permission="'user:delete'">删除</el-button>
```

### 代码权限

在代码中检查权限：

```javascript
import { hasPermission } from '@/utils/permission'

if (hasPermission('user:add')) {
  // 有权限执行的操作
}
```

## 配置说明

### 代理配置

修改 vite.config.js 中的代理配置：

```javascript
server: {
  proxy: {
    '/api': {
      target: 'http://localhost:7789',  // 后端服务地址
      changeOrigin: true,
      rewrite: (path) => path.replace(/^\/api/, '')
    }
  }
}
```
