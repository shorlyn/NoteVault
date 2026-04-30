# NoteVault Web

NoteVault 的 Vue 3 Web 客户端。

## 技术栈

- Vue 3 + TypeScript + Vite
- Pinia
- Naive UI
- lucide-vue-next
- md-editor-v3
- Tiptap

## 启动

```bash
npm install
npm run dev
```

默认访问地址：`http://localhost:5173`

默认 API 地址：`/`，生产环境会走同域 `/api/` 代理。

如需覆盖 API 地址：

```bash
VITE_API_BASE_URL=http://localhost:5111 npm run dev
```

也可以复制 `.env.example`：

```bash
cp .env.example .env.local
```

## 构建

```bash
npm run build
```

编辑器依赖体积较大，构建时可能出现 Vite chunk size 提醒，这是当前依赖结构导致的，不影响运行。

## 主要页面

- `src/views/LoginView.vue`：登录页
- `src/views/WorkspaceView.vue`：工作区容器
- `src/components/SidebarNav.vue`：左侧导航和设置入口
- `src/components/NoteListPanel.vue`：笔记列表
- `src/components/DocHeader.vue`：笔记标题、标签、文件夹、类型控制
- `src/components/EditorFrame.vue`：Markdown / 富文本编辑区

## 交互约定

- 新建笔记时选择 Markdown 或富文本
- 文档创建后不支持随意切换类型，只能通过显式转换流程转换
- 主题色、夜间模式和默认新建类型会保存到 `localStorage`
- 加密笔记需要输入密码后才能查看内容
