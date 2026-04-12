# JMComic C# Library

一个用于访问和下载禁漫天堂（JMComic）内容的C#库。

## 项目架构

### 命名空间结构

本项目采用分层命名空间设计，所有命名空间都以 `zms9110750` 开头：

#### 1. 核心命名空间
- `zms9110750.JMComic` - 主API入口和核心功能
- `zms9110750.JMComic.Models` - 数据模型和实体类
- `zms9110750.JMComic.Clients` - HTTP客户端实现
- `zms9110750.JMComic.Downloaders` - 下载器实现
- `zms9110750.JMComic.Utilities` - 工具类和辅助功能
- `zms9110750.JMComic.Configuration` - 配置相关类
- `zms9110750.JMComic.Exceptions` - 自定义异常类

### 核心组件

#### 1. 数据模型 (Models)
- `JmImageDetail` - 图片详情
- `JmPhotoDetail` - 章节详情
- `JmAlbumDetail` - 本子详情

#### 2. 客户端 (Clients)
- `IJmClient` - 客户端接口
- `JmClient` - 基础客户端实现
- `JmApiClient` - API客户端（移动端）
- `JmHtmlClient` - HTML客户端（网页端）

#### 3. 下载器 (Downloaders)
- `IJmDownloader` - 下载器接口
- `JmDownloader` - 下载器实现
- 支持并发下载和图片解密

#### 4. 工具类 (Utilities)
- `JmCrypto` - 加密解密工具（核心）
- `JmToolkit` - 通用工具方法
- `JmImageTool` - 图片处理工具

#### 5. 配置 (Configuration)
- `IJmOption` - 配置接口
- `JmOption` - 配置实现
- 支持灵活配置，不绑定特定格式

### 设计原则

1. **最小依赖** - 只依赖必要的NuGet包
2. **完全异步** - 所有IO操作使用async/await
3. **接口驱动** - 核心功能通过接口暴露
4. **可扩展性** - 支持自定义实现
5. **无格式绑定** - 配置不强制使用特定格式

### 技术栈

- **目标框架**: .NET 10.0
- **HTTP客户端**: Flurl.Http
- **图片处理**: SixLabors.ImageSharp
- **异步编程**: async/await
- **依赖注入**: 支持但不强制

### 使用示例

```csharp
using zms9110750.JMComic;

// 简单使用
await JmComicApi.DownloadAlbumAsync("12345", "./downloads");

// 高级使用
var option = new JmOption
{
    BaseDir = "./downloads",
    ClientType = ClientType.Api
};

var downloader = new JmDownloader(option);
await downloader.DownloadAlbumAsync("12345");
```

### 项目状态

🚧 **开发中** - 核心功能正在实现

### 许可证

MIT License