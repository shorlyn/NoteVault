<div align="center">
  <h1>NoteVault</h1>
  <p><strong>Your notes, your keys, your vault.</strong></p>
  <p>一个支持 Markdown、富文本、标签、文件夹和单篇加密的私有云笔记系统。</p>
  <p>
    <a href="https://github.com/shorlyn/NoteVault.git">GitHub</a>
    ·
    <span>v1.0.0</span>
  </p>
</div>

## 简介

NoteVault 是一个私有云笔记项目，包含 Web 端、Flutter App 和 .NET API。它适合个人或小团队自部署，用来管理 Markdown 笔记、富文本笔记、标签、文件夹、附件和加密内容。

当前版本重点是稳定、清爽和可控：文档类型在创建时确定，后续需要显式转换，避免 Markdown 与富文本反复切换导致内容格式损坏。

## 功能亮点

- 现代 SaaS 风格 Web 界面，支持浅色、夜间模式和主题色切换
- Markdown 编辑、预览开关、图片上传和代码块展示
- 富文本编辑，适合所见即所得记录
- 新建笔记时选择 Markdown / 富文本，并可设置默认新建类型
- 文档类型锁定，提供显式转换流程
- 文件夹、标签、收藏夹、最近打开和回收站
- 回收站支持恢复和二次确认彻底删除
- 单篇笔记前端加密，后端只保存密文
- 800ms 防抖自动保存和 Ctrl / Command + S 手动保存
- JWT 登录、Refresh Token、BCrypt 密码存储和登录频率限制
- 七牛云服务端上传，前端和 App 不暴露云存储密钥
- Flutter App 支持移动端随手记

## 技术栈

| 模块 | 技术 |
| --- | --- |
| Web | Vue 3, TypeScript, Vite, Pinia, Naive UI, md-editor-v3, Tiptap |
| API | .NET 8 / .NET 9, Minimal API, SqlSugar, SQLite / MariaDB, JWT |
| App | Flutter |
| 加密 | Web Crypto API, PBKDF2, AES-GCM |
| 上传 | Qiniu SDK |

## 项目结构

```text
NoteVault/
  NoteVault.Api/          # .NET Minimal API
    DTOs/
    Extensions/
    Models/
    Repositories/
    Services/
  notevault-web/          # Vue 3 Web 客户端
    src/api/
    src/components/
    src/stores/
    src/views/
  notevault_app/          # Flutter App
  NoteVault.sln
```

## 快速开始

克隆项目：

```bash
git clone https://github.com/shorlyn/NoteVault.git
cd NoteVault
```

启动后端：

```bash
cd NoteVault.Api
dotnet run --launch-profile http
```

默认 API 地址：`http://localhost:5111`

启动 Web：

```bash
cd notevault-web
npm install
npm run dev
```

默认 Web 地址：`http://localhost:5173`

启动 Flutter App：

```bash
cd notevault_app
flutter pub get
flutter run --dart-define=API_BASE=http://你的后端地址
```

## 默认账号

- 账号：`admin`
- 密码：`admin123`

首次启动如果没有用户，后端会自动创建默认管理员。

## 环境配置

后端配置文件：[appsettings.json](NoteVault.Api/appsettings.json)

常用配置：

- `Database:Provider`：数据库类型，支持 `Sqlite`、`MariaDB` / `MySQL`
- `Database:Path`：SQLite 数据库文件路径
- `Database:ConnectionString`：MariaDB / MySQL 连接字符串
- `Jwt:*`：JWT issuer、audience、secret 和有效期
- `Qiniu:*`：七牛云 AccessKey、SecretKey、Bucket、Domain、Zone、上传大小和 MIME 白名单
- `ApiSignature:Enabled`：可选请求签名校验，默认关闭

Web 端 API 地址可通过环境变量覆盖：

```bash
# 生产环境同域 Nginx 反代 /api/ 时不用配置，默认使用 /
# 本地开发直连后端时可配置：
VITE_API_BASE_URL=http://localhost:5111
```

可参考示例配置：

- [NoteVault.Api/appsettings.Local.example.json](NoteVault.Api/appsettings.Local.example.json)
- [notevault-web/.env.example](notevault-web/.env.example)

本地密钥推荐放到 `NoteVault.Api/appsettings.Local.json`，该文件已被 `.gitignore` 忽略，不会被提交：

```bash
cp NoteVault.Api/appsettings.Local.example.json NoteVault.Api/appsettings.Local.json
```

也可以使用环境变量覆盖配置，适合服务器部署：

```bash
export Qiniu__AccessKey=你的七牛AccessKey
export Qiniu__SecretKey=你的七牛SecretKey
export Qiniu__Bucket=你的Bucket
export Qiniu__Domain=https://你的 CDN 域名
export Jwt__Secret=替换成强随机字符串
```

## 数据模型

- `User`：用户、密码哈希、刷新令牌等
- `Folder`：文件夹树
- `Note`：笔记主体、文档类型、加密字段、收藏、回收站状态
- `Tag`：标签
- `NoteTag`：笔记标签关联
- `Attachment`：附件元数据

启动时 SqlSugar CodeFirst 会自动建表和迁移。

## 开发命令

后端：

```bash
cd NoteVault.Api
dotnet build
dotnet run --launch-profile http
```

Web：

```bash
cd notevault-web
npm install
npm run dev
npm run build
```

App：

```bash
cd notevault_app
flutter pub get
flutter run --dart-define=API_BASE=http://localhost:5111
```

## 部署提醒

- 生产环境请启用 HTTPS
- 同域 Nginx 反代 `/api/` 时，前端打包不需要设置 `VITE_API_BASE_URL`
- 替换默认管理员密码
- 使用足够强的 JWT Secret
- 不要把七牛云密钥提交到公开仓库
- 建议对数据库和附件存储做定期备份

## License

MIT
