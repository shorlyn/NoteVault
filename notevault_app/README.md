# NoteVault App

NoteVault 的 Flutter 移动端客户端，用于随手记录和访问私有云笔记。

## 启动

```bash
flutter pub get
flutter run --dart-define=API_BASE=http://你的后端地址
```

本机模拟器访问本机后端时，地址可能需要按平台调整：

- iOS Simulator：`http://localhost:5111`
- Android Emulator：`http://10.0.2.2:5111`
- 真机：使用局域网 IP，例如 `http://192.168.1.10:5111`

## 主要能力

- 登录 NoteVault API
- 查看、搜索、新建和编辑笔记
- 上传图片
- 加密和解密笔记内容
- 与 Web 端共用同一套 REST API

## 后端要求

先启动 `NoteVault.Api`：

```bash
cd ../NoteVault.Api
dotnet run --launch-profile http
```

默认 API 端口是 `5111`。
