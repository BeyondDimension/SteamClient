# SteamClient
Steam 客户端 API 调用

### 🏗️ 项目结构
- BD.SteamClient8
  - Extensions 扩展函数
  - Helpers 静态助手类
  - Services 服务接口
    - PInvoke 平台调用服务的**接口定义**
    - WebApi 服务**接口定义**
- BD.SteamClient8.Impl
  - PInvoke 平台调用服务的**实现类**
  - WebApi 服务的**实现类**
  - Extensions 接口与实现类的 Ioc 注入扩展函数
- BD.SteamClient8.Models
  - PInvoke 平台调用服务的**模型类**
  - WebApi 服务的**模型类**
- BD.SteamClient8.Primitives
  - Constants 静态类常量
  - Enums 枚举定义
    - PInvoke 平台调用服务的**枚举**
    - WebApi 服务的**枚举**
- BD.SteamClient8.ViewModels
  - PInvoke 平台调用服务的**视图模型类**
  - WebApi 服务的**视图模型类**