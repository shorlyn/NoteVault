<template>
  <main class="workspace" :class="[{ 'is-fullscreen': fullscreen, 'is-dark': ui.dark }, `theme-${ui.theme}`]">
    <SidebarNav
      :dark="ui.dark"
      :theme="ui.theme"
      :active-view="activeView"
      :active-note-count="activeNoteCount"
      :recent-count="recentCount"
      :pinned-count="pinnedCount"
      :trash-count="deleted ? notes.length : 0"
      :folder-rows="folderRows"
      :tags="tags"
      :selected-folder="selectedFolder"
      :selected-tag="selectedTag"
      :folder-note-count="folderNoteCount"
      :tag-note-count="tagNoteCount"
      @toggle-dark="ui.toggleDark"
      @set-theme="ui.setTheme"
      @create-note="createNote"
      @show-all="showAllNotes"
      @show-recent="showRecent"
      @show-favorites="showFavorites"
      @show-trash="showTrash"
      @create-folder="createFolder"
      @create-tag="createTag"
      @select-folder="selectFolder"
      @toggle-tag="toggleTag"
      @open-settings="settingsDialogVisible = true"
    />

    <NoteListPanel
      :dark="ui.dark"
      :query="q"
      :loading="loading"
      :list-title="listTitle"
      :current-id="current?.id"
      :trash-mode="deleted"
      :notes="notes"
      :pinned-notes="pinnedNotes"
      :regular-notes="regularNotes"
      @update:query="q = $event"
      @load-notes="loadNotes"
      @show-all="showAllNotes"
      @open-note="openNote"
      @toggle-pin="togglePin"
      @delete-note="deleteNote"
      @restore-note="restoreNote"
    />

    <section class="editor-column">
      <template v-if="current">
        <EditorTopbar
          :dark="ui.dark"
          :save-state="saveState"
          :deleted="deleted"
          :is-pinned="current.isPinned"
          :username="currentUsername"
          :user-initial="userInitial"
          @save="saveNow"
          @toggle-pin="toggleCurrentPin"
          @encrypt="openEncryptDialog"
          @toggle-fullscreen="fullscreen = !fullscreen"
          @restore="restoreCurrentNote"
          @delete="deleteCurrentNote"
        />

        <DocHeader
          :dark="ui.dark"
          v-model:title="current.title"
          v-model:folder-picker-visible="folderPickerVisible"
          v-model:tag-picker-visible="tagPickerVisible"
          :folder-id="current.folderId"
          :current-folder-name="currentFolderName"
          :folder-rows="folderRows"
          :tags="tags"
          :note-tags="current.tags"
          :content-type="current.contentType"
          :show-preview="showPreview"
          @update:title="queueSave"
          @set-folder="setCurrentFolder"
          @remove-tag="removeCurrentTag"
          @toggle-tag="toggleCurrentTag"
          @create-tag="createTagFromPicker"
          @convert-content-type="requestConvertContentType"
          @toggle-preview="togglePreview"
        />

        <EditorFrame
          v-model:content="plainContent"
          :content-type="current.contentType"
          :editor="editor || undefined"
          :dark="ui.dark"
          :show-preview="showPreview"
          :md-editor-key="mdEditorKey"
          :md-editor-class="mdEditorClass"
          :word-count="wordCount"
          :line-count="lineCount"
          @queue-save="queueSave"
          @upload-images="uploadMarkdownImages"
        />

        <div class="doc-footer">
          最后更新: {{ formatDateTime(current.updatedAt) }}
        </div>
      </template>
      <div v-else class="editor-empty">
        <FilePlus2 :size="34" />
        <p>选择或新建一篇笔记</p>
      </div>
    </section>

    <div v-if="tagDialogVisible" class="modal-layer" @mousedown.self="tagDialogVisible = false">
      <section class="action-modal compact-modal" @keydown.stop>
        <button class="action-modal-close" type="button" @click="tagDialogVisible = false">
          <X :size="24" />
        </button>
        <header class="action-modal-header">
          <div class="action-modal-icon">
            <TagIcon :size="24" />
          </div>
          <div>
            <h2>添加标签</h2>
            <p>给笔记添加一个新的标签分类</p>
          </div>
        </header>
        <label class="action-modal-label" for="tag-name-input">标签名称</label>
        <div class="action-input-shell" @click="tagNameInput?.focus()">
          <TagIcon :size="20" />
          <input
            id="tag-name-input"
            ref="tagNameInput"
            v-model="tagName"
            maxlength="24"
            placeholder="输入标签名称"
            @input="onTagNameInput"
            @keydown.enter="confirmCreateTag"
          />
          <span>{{ tagName.length }} / 24</span>
        </div>
        <footer class="action-modal-actions">
          <button class="action-cancel" type="button" @click="tagDialogVisible = false">取消</button>
          <button class="action-confirm" type="button" :disabled="!tagName.trim()" @click="confirmCreateTag">创建</button>
        </footer>
      </section>
    </div>
    <div v-if="createTypeDialogVisible" class="modal-layer" @mousedown.self="createTypeDialogVisible = false">
      <section class="action-modal compact-modal" @keydown.stop>
        <button class="action-modal-close" type="button" @click="createTypeDialogVisible = false">
          <X :size="24" />
        </button>
        <header class="action-modal-header">
          <div class="action-modal-icon">
            <FilePlus2 :size="24" />
          </div>
          <div>
            <h2>新建笔记</h2>
            <p>先选择文档类型，创建后默认保持不变</p>
          </div>
        </header>
        <div class="default-type-hint">
          默认：{{ ui.defaultContentType === 'markdown' ? 'Markdown' : '富文本' }}
          <button type="button" @click="createNoteWithType(ui.defaultContentType)">按默认类型创建</button>
        </div>
        <div class="type-choice-grid">
          <button type="button" :class="{ active: ui.defaultContentType === 'markdown' }" @click="createNoteWithType('markdown')">
            <strong>Markdown</strong>
            <span>适合代码、结构化笔记和双栏预览</span>
          </button>
          <button type="button" :class="{ active: ui.defaultContentType === 'html' }" @click="createNoteWithType('html')">
            <strong>富文本</strong>
            <span>适合直接排版、勾选清单和所见即所得编辑</span>
          </button>
        </div>
      </section>
    </div>
    <div v-if="settingsDialogVisible" class="modal-layer" @mousedown.self="settingsDialogVisible = false">
      <section class="action-modal settings-modal" @keydown.stop>
        <button class="action-modal-close" type="button" @click="settingsDialogVisible = false">
          <X :size="24" />
        </button>
        <header class="action-modal-header">
          <div class="action-modal-icon">
            <SettingsIcon :size="24" />
          </div>
          <div>
            <h2>设置</h2>
            <p>调整外观、新建偏好和账号操作</p>
          </div>
        </header>
        <div class="settings-body">
          <section class="settings-section">
            <div>
              <strong>夜间模式</strong>
              <span>切换浅色或深色界面</span>
            </div>
            <button class="switch-button" type="button" :class="{ active: ui.dark }" @click="ui.setDark(!ui.dark)">
              {{ ui.dark ? '已开启' : '已关闭' }}
            </button>
          </section>
          <section class="settings-section vertical">
            <div>
              <strong>主题色</strong>
              <span>选择界面的主色调</span>
            </div>
            <div class="settings-options">
              <button
                v-for="option in themeOptions"
                :key="option.value"
                type="button"
                :class="{ active: ui.theme === option.value }"
                @click="ui.setTheme(option.value)"
              >
                <span class="theme-swatch" :class="`theme-${option.value}`" />
                {{ option.label }}
              </button>
            </div>
          </section>
          <section class="settings-section vertical">
            <div>
              <strong>默认新建类型</strong>
              <span>点击新建时优先使用这个文档类型</span>
            </div>
            <div class="settings-options">
              <button type="button" :class="{ active: ui.defaultContentType === 'markdown' }" @click="ui.setDefaultContentType('markdown')">
                Markdown
              </button>
              <button type="button" :class="{ active: ui.defaultContentType === 'html' }" @click="ui.setDefaultContentType('html')">
                富文本
              </button>
            </div>
          </section>
        </div>
        <footer class="action-modal-actions settings-actions">
          <button class="action-cancel danger-text" type="button" @click="logout">
            <LogOut :size="16" />
            退出登录
          </button>
          <button class="action-confirm" type="button" @click="settingsDialogVisible = false">完成</button>
        </footer>
      </section>
    </div>
    <div v-if="convertDialogVisible" class="modal-layer" @mousedown.self="convertDialogVisible = false">
      <section class="action-modal compact-modal" @keydown.stop>
        <button class="action-modal-close" type="button" @click="convertDialogVisible = false">
          <X :size="24" />
        </button>
        <header class="action-modal-header">
          <div class="action-modal-icon warning-icon">
            <FilePlus2 :size="24" />
          </div>
          <div>
            <h2>转换文档类型</h2>
            <p>这会生成一份转换后的内容，复杂样式可能无法完全保留。</p>
          </div>
        </header>
        <div class="convert-summary">
          {{ current?.contentType === 'markdown' ? 'Markdown → 富文本' : '富文本 → Markdown' }}
        </div>
        <footer class="action-modal-actions">
          <button class="action-cancel" type="button" @click="convertDialogVisible = false">取消</button>
          <button class="action-confirm" type="button" @click="confirmConvertContentType">确认转换</button>
        </footer>
      </section>
    </div>
    <div v-if="encryptDialog" class="modal-layer" @mousedown.self="encryptDialog = false">
      <section class="action-modal compact-modal" @keydown.stop>
        <button class="action-modal-close" type="button" @click="encryptDialog = false">
          <X :size="24" />
        </button>
        <header class="action-modal-header">
          <div class="action-modal-icon">
            <Lock :size="24" />
          </div>
          <div>
            <h2>加密笔记</h2>
            <p>使用本地密码保护当前笔记内容</p>
          </div>
        </header>
        <label class="action-modal-label" for="encrypt-password-input">加密密码</label>
        <div class="action-input-shell" @click="encryptPasswordInput?.focus()">
          <Lock :size="20" />
          <input
            id="encrypt-password-input"
            ref="encryptPasswordInput"
            v-model="encryptPassword"
            type="password"
            placeholder="输入本地加密密码"
            @input="onEncryptPasswordInput"
            @keydown.enter="encryptCurrent"
          />
        </div>
        <footer class="action-modal-actions">
          <button class="action-cancel" type="button" @click="encryptDialog = false">取消</button>
          <button class="action-confirm" type="button" :disabled="!encryptPassword" @click="encryptCurrent">加密保存</button>
        </footer>
      </section>
    </div>
    <div v-if="decryptDialog" class="modal-layer">
      <section class="action-modal compact-modal" @keydown.stop>
        <button class="action-modal-close" type="button" @click="closeDecryptDialog">
          <X :size="24" />
        </button>
        <header class="action-modal-header">
          <div class="action-modal-icon">
            <Lock :size="24" />
          </div>
          <div>
            <h2>解锁笔记</h2>
            <p>输入密码查看这篇加密笔记</p>
          </div>
        </header>
        <label class="action-modal-label" for="decrypt-password-input">解锁密码</label>
        <div class="action-input-shell" @click="decryptPasswordInput?.focus()">
          <Lock :size="20" />
          <input
            id="decrypt-password-input"
            ref="decryptPasswordInput"
            v-model="decryptPassword"
            type="password"
            placeholder="输入该笔记密码"
            @input="onDecryptPasswordInput"
            @keydown.enter="decryptCurrent"
          />
        </div>
        <footer class="action-modal-actions">
          <button class="action-cancel" type="button" @click="closeDecryptDialog">取消</button>
          <button class="action-confirm" type="button" :disabled="!decryptPassword" @click="decryptCurrent">解锁</button>
        </footer>
      </section>
    </div>
    <HardDeleteDialog
      v-if="hardDeleteDialogVisible"
      :title="hardDeleteTarget?.title"
      @cancel="closeHardDeleteDialog(false)"
      @confirm="closeHardDeleteDialog(true)"
    />
    <FolderDialog
      v-if="folderDialogVisible"
      :name="folderName"
      @update:name="folderName = $event"
      @cancel="folderDialogVisible = false"
      @create="confirmCreateFolder"
    />
  </main>
</template>

<script setup lang="ts">
import { computed, nextTick, onMounted, onUnmounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useEditor } from '@tiptap/vue-3'
import StarterKit from '@tiptap/starter-kit'
import { useMessage } from 'naive-ui'
import {
  FilePlus2,
  Lock,
  LogOut,
  Settings as SettingsIcon,
  Tag as TagIcon,
  X,
} from 'lucide-vue-next'
import { foldersApi, notesApi, tagsApi, uploadApi } from '../api'
import DocHeader from '../components/DocHeader.vue'
import EditorFrame from '../components/EditorFrame.vue'
import EditorTopbar from '../components/EditorTopbar.vue'
import FolderDialog from '../components/FolderDialog.vue'
import HardDeleteDialog from '../components/HardDeleteDialog.vue'
import NoteListPanel from '../components/NoteListPanel.vue'
import SidebarNav from '../components/SidebarNav.vue'
import { decryptContent, encryptContent } from '../crypto'
import { useAuthStore } from '../stores/auth'
import { useUiStore } from '../stores/ui'
import type { AccentTheme } from '../stores/ui'
import type { ContentType, Folder, NoteDetail, NoteList, Tag } from '../types'

const themeOptions: Array<{ value: AccentTheme; label: string }> = [
  { value: 'violet', label: '紫色' },
  { value: 'blue', label: '蓝色' },
  { value: 'emerald', label: '绿色' },
  { value: 'rose', label: '玫红' },
]

const auth = useAuthStore()
const ui = useUiStore()
const router = useRouter()
const message = useMessage()
const folders = ref<Folder[]>([])
const tags = ref<Tag[]>([])
const notes = ref<NoteList[]>([])
const countNotes = ref<NoteList[]>([])
const current = ref<NoteDetail | null>(null)
const plainContent = ref('')
const selectedFolder = ref('')
const selectedTag = ref('')
const q = ref('')
const deleted = ref(false)
const recent = ref(false)
const favorite = ref(false)
const loading = ref(false)
const saveState = ref('已保存')
const fullscreen = ref(false)
const showPreview = ref(true)
const encryptDialog = ref(false)
const decryptDialog = ref(false)
const tagDialogVisible = ref(false)
const createTypeDialogVisible = ref(false)
const convertDialogVisible = ref(false)
const settingsDialogVisible = ref(false)
const tagPickerVisible = ref(false)
const folderPickerVisible = ref(false)
const folderDialogVisible = ref(false)
const folderDialogParentId = ref<string | null>(null)
const folderName = ref('')
const tagName = ref('')
const tagNameInput = ref<HTMLInputElement | null>(null)
const encryptPassword = ref('')
const encryptPasswordInput = ref<HTMLInputElement | null>(null)
const decryptPassword = ref('')
const decryptPasswordInput = ref<HTMLInputElement | null>(null)
const hardDeleteDialogVisible = ref(false)
const hardDeleteTarget = ref<NoteList | null>(null)
let saveTimer: number | undefined
let hardDeleteResolver: ((confirmed: boolean) => void) | null = null

const editor = useEditor({
  extensions: [StarterKit],
  content: '',
  onUpdate: ({ editor }) => {
    plainContent.value = editor.getHTML()
    queueSave()
  },
})

const folderRows = computed(() => {
  const rows: Array<Folder & { depth: number; hasChildren: boolean }> = [{ id: '', parentId: null, name: '全部笔记', sort: 0, createdAt: '', updatedAt: '', depth: 0, hasChildren: folders.value.some(f => !f.parentId) }]
  const walk = (parentId: string | null, depth: number) => {
    folders.value
      .filter(folder => (folder.parentId || null) === parentId)
      .sort((a, b) => a.sort - b.sort || a.name.localeCompare(b.name))
      .forEach((folder) => {
        rows.push({ ...folder, depth, hasChildren: folders.value.some(item => item.parentId === folder.id) })
        walk(folder.id, depth + 1)
      })
  }
  walk(null, 1)
  return rows
})
const pinnedNotes = computed(() => notes.value.filter(note => note.isPinned))
const regularNotes = computed(() => notes.value.filter(note => !note.isPinned))
const activeNoteCount = computed(() => countNotes.value.filter(note => !note.isDeleted).length || notes.value.length)
const pinnedCount = computed(() => countNotes.value.filter(note => note.isPinned && !note.isDeleted).length || notes.value.filter(note => note.isPinned).length)
const recentCount = computed(() => countNotes.value.filter(note => note.lastOpenedAt && !note.isDeleted).length || notes.value.filter(note => note.lastOpenedAt).length)
const activeView = computed(() => deleted.value ? 'trash' : recent.value ? 'recent' : favorite.value ? 'favorite' : 'all')
const listTitle = computed(() => deleted.value ? '回收站' : recent.value ? '最近打开' : favorite.value ? '收藏夹' : selectedTag.value ? '标签笔记' : '全部笔记')
const currentFolderName = computed(() => {
  if (!current.value?.folderId) return '全部笔记'
  return folders.value.find(folder => folder.id === current.value?.folderId)?.name || '未分类'
})
const currentUsername = computed(() => auth.user?.username || 'admin')
const userInitial = computed(() => currentUsername.value.slice(0, 1).toUpperCase())
const wordCount = computed(() => plainContent.value.replace(/\s/g, '').length)
const lineCount = computed(() => Math.max(1, plainContent.value.split('\n').length))
const mdEditorClass = computed(() => showPreview.value ? 'vault-md-editor' : 'vault-md-editor no-preview')
const mdEditorKey = computed(() => `${current.value?.id || 'empty'}-${current.value?.contentType || 'markdown'}-${showPreview.value ? 'preview' : 'edit'}`)

watch(() => current.value?.contentType, () => {
  if (current.value?.contentType === 'html') editor.value?.commands.setContent(plainContent.value || '<p></p>')
})

onMounted(async () => {
  await loadInitialData()
  await openFirstNote()
  window.addEventListener('keydown', onKey)
})
onUnmounted(() => {
  window.removeEventListener('keydown', onKey)
})

async function loadFolders() { folders.value = await foldersApi.list() }
async function loadTags() {
  const seen = new Set<string>()
  tags.value = (await tagsApi.list()).filter((tag) => {
    const key = tag.name.trim().toLowerCase()
    if (seen.has(key)) return false
    seen.add(key)
    return true
  })
}
async function loadNoteCounts() {
  countNotes.value = await notesApi.list({ deleted: false, recent: false })
}
async function loadInitialData() {
  try {
    await loadFolders()
    await loadTags()
    await loadNoteCounts()
    await loadNotes()
  } catch (error) {
    console.error('Failed to load workspace data', error)
    message.error('加载笔记数据失败，请刷新或重新登录')
  }
}
async function loadNotes() {
  const shouldShowLoading = notes.value.length === 0
  if (shouldShowLoading) loading.value = true
  try {
    const result = await notesApi.list({ folderId: selectedFolder.value || undefined, tagId: selectedTag.value || undefined, q: q.value || undefined, deleted: deleted.value, recent: recent.value })
    notes.value = favorite.value ? result.filter(note => note.isPinned) : result
  }
  finally { if (shouldShowLoading) loading.value = false }
}
async function refreshAndOpen() {
  await loadNotes()
  await openFirstNote()
}
async function openFirstNote() {
  if (current.value || !notes.value.length) return
  const first = notes.value.find(note => !note.isEncrypted) ?? notes.value[0]
  await openNote(first.id)
}
function showAllNotes() {
  if (!selectedFolder.value && !selectedTag.value && !deleted.value && !recent.value && !favorite.value) return
  selectedFolder.value = ''; selectedTag.value = ''; deleted.value = false; recent.value = false; favorite.value = false; current.value = null; refreshAndOpen()
}
function showRecent() {
  if (recent.value && !deleted.value && !favorite.value && !selectedFolder.value && !selectedTag.value) return
  selectedFolder.value = ''; selectedTag.value = ''; deleted.value = false; recent.value = true; favorite.value = false; current.value = null; refreshAndOpen()
}
function showFavorites() {
  if (favorite.value && !deleted.value && !recent.value && !selectedFolder.value && !selectedTag.value) return
  selectedFolder.value = ''; selectedTag.value = ''; deleted.value = false; recent.value = false; favorite.value = true; current.value = null; refreshAndOpen()
}
function showTrash() {
  if (deleted.value && !recent.value && !favorite.value && !selectedFolder.value && !selectedTag.value) return
  selectedFolder.value = ''; selectedTag.value = ''; deleted.value = true; recent.value = false; favorite.value = false; current.value = null; refreshAndOpen()
}
function toggleTag(id: string) {
  const nextTag = selectedTag.value === id ? '' : id
  if (selectedTag.value === nextTag && !deleted.value && !recent.value && !favorite.value) return
  selectedFolder.value = ''; selectedTag.value = nextTag; deleted.value = false; recent.value = false; favorite.value = false; current.value = null; refreshAndOpen()
}
function selectFolder(id: string) {
  if (selectedFolder.value === id && !selectedTag.value && !deleted.value && !recent.value && !favorite.value) return
  selectedFolder.value = id; selectedTag.value = ''; deleted.value = false; recent.value = false; favorite.value = false; current.value = null; refreshAndOpen()
}
function folderNoteCount(id: string) { return countNotes.value.filter(note => note.folderId === id && !note.isDeleted).length || '' }
function tagNoteCount(id: string) { return countNotes.value.filter(note => note.tags.some(tag => tag.id === id) && !note.isDeleted).length || '' }
async function createFolder(parentId: string | null) {
  folderDialogParentId.value = parentId
  folderName.value = ''
  folderDialogVisible.value = true
}
async function confirmCreateFolder() {
  const name = folderName.value.trim()
  if (!name) return
  await foldersApi.create({ parentId: folderDialogParentId.value, name, sort: folders.value.length + 1 })
  folderDialogVisible.value = false
  await loadFolders()
}
async function createTag() {
  tagName.value = ''
  tagDialogVisible.value = true
  await nextTick()
  window.setTimeout(() => tagNameInput.value?.focus(), 80)
}
async function confirmCreateTag() {
  const name = tagName.value.trim()
  if (!name) return
  if (tags.value.some(tag => tag.name.trim().toLowerCase() === name.toLowerCase())) {
    message.warning('标签已存在')
    tagDialogVisible.value = false
    return
  }
  await tagsApi.create({ name, color: '#7C5CFF' })
  tagDialogVisible.value = false
  await loadTags()
  await loadNoteCounts()
}
async function createTagFromPicker() {
  tagPickerVisible.value = false
  await createTag()
}
function currentHasTag(id: string) {
  return !!current.value?.tags.some(tag => tag.id === id)
}
async function toggleCurrentTag(tag: Tag) {
  if (!current.value) return
  if (currentHasTag(tag.id)) {
    await removeCurrentTag(tag.id)
  } else {
    current.value.tags = [...current.value.tags, tag]
    await saveCurrentMeta()
  }
}
async function removeCurrentTag(id: string) {
  if (!current.value) return
  current.value.tags = current.value.tags.filter(tag => tag.id !== id)
  await saveCurrentMeta()
}
async function setCurrentFolder(id: string) {
  if (!current.value || (current.value.folderId || '') === id) {
    folderPickerVisible.value = false
    return
  }
  current.value.folderId = id || null
  folderPickerVisible.value = false
  await saveCurrentMeta()
}
async function saveCurrentMeta() {
  if (!current.value) return
  try {
    saveState.value = '正在保存'
    current.value.content = current.value.isEncrypted ? '' : plainContent.value
    const saved = await notesApi.update(current.value.id, { ...current.value, tagIds: current.value.tags.map(tag => tag.id) })
    current.value = saved
    saveState.value = '已保存'
    await loadNoteCounts()
    await loadNotes()
  } catch {
    saveState.value = '保存失败'
  }
}
function onTagNameInput(event: Event) {
  tagName.value = (event.target as HTMLInputElement).value
}
function togglePreview() {
  showPreview.value = !showPreview.value
}
function createNote() {
  createTypeDialogVisible.value = true
}
async function createNoteWithType(type: ContentType) {
  createTypeDialogVisible.value = false
  const note = await notesApi.create({
    folderId: selectedFolder.value || null,
    title: '随便记一个吧',
    content: type === 'markdown'
      ? '# 随便写点什么吧\n\n从这里开始记录。'
      : '<p>从这里开始记录。</p>',
    contentType: type,
    isEncrypted: false,
    isPinned: false,
    tagIds: [],
  })
  notes.value.unshift(note)
  await loadNoteCounts()
  await openNote(note.id)
}
function requestConvertContentType() {
  if (!current.value) return
  if (current.value.isEncrypted) {
    message.warning('加密笔记暂不支持转换文档类型')
    return
  }
  convertDialogVisible.value = true
}
async function confirmConvertContentType() {
  if (!current.value) return
  convertDialogVisible.value = false
  const nextType: ContentType = current.value.contentType === 'markdown' ? 'html' : 'markdown'
  plainContent.value = nextType === 'html'
    ? markdownToHtml(plainContent.value)
    : htmlToMarkdown(plainContent.value)
  current.value.contentType = nextType
  if (nextType === 'html') {
    await nextTick()
    editor.value?.commands.setContent(plainContent.value || '<p></p>')
  }
  await saveNow()
  message.success(`已转换为${nextType === 'markdown' ? 'Markdown' : '富文本'}文档`)
}
async function openNote(id: string) {
  const note = await notesApi.detail(id)
  await notesApi.open(id)
  current.value = note
  saveState.value = '已保存'
  if (note.isEncrypted) {
    plainContent.value = ''
    decryptDialog.value = true
    nextTick(() => window.setTimeout(() => decryptPasswordInput.value?.focus(), 80))
  } else {
    plainContent.value = note.content || ''
    if (note.contentType === 'html') editor.value?.commands.setContent(plainContent.value || '<p></p>')
  }
}
async function openEncryptDialog() {
  encryptPassword.value = ''
  encryptDialog.value = true
  await nextTick()
  window.setTimeout(() => encryptPasswordInput.value?.focus(), 80)
}
function onEncryptPasswordInput(event: Event) {
  encryptPassword.value = (event.target as HTMLInputElement).value
}
function onDecryptPasswordInput(event: Event) {
  decryptPassword.value = (event.target as HTMLInputElement).value
}
function closeDecryptDialog() {
  decryptDialog.value = false
  decryptPassword.value = ''
  plainContent.value = ''
  current.value = null
}
function queueSave() {
  if (!current.value || current.value.isEncrypted) return
  saveState.value = '正在保存'
  window.clearTimeout(saveTimer)
  saveTimer = window.setTimeout(saveNow, 800)
}
async function saveNow() {
  if (!current.value) return
  try {
    current.value.content = plainContent.value
    const saved = await notesApi.update(current.value.id, { ...current.value, tagIds: current.value.tags.map(t => t.id) })
    current.value = saved
    saveState.value = '已保存'
    await loadNotes()
  } catch {
    saveState.value = '保存失败'
  }
}
async function togglePin(note: NoteList) {
  const detail = current.value?.id === note.id ? current.value : await notesApi.detail(note.id)
  const nextPinned = !detail.isPinned
  const saved = await notesApi.update(detail.id, { ...detail, isPinned: nextPinned, tagIds: detail.tags.map(t => t.id) })
  if (current.value?.id === saved.id) current.value = saved
  await loadNoteCounts()
  await loadNotes()
  if (favorite.value && !nextPinned && current.value?.id === saved.id) {
    current.value = null
    plainContent.value = ''
    await openFirstNote()
  }
}
async function toggleCurrentPin() {
  if (!current.value) return
  await togglePin(current.value)
}
async function deleteNote(note: NoteList) {
  if (deleted.value) {
    const ok = await confirmHardDelete(note)
    if (!ok) return
    await notesApi.hardRemove(note.id)
  } else {
    await notesApi.remove(note.id)
  }
  if (current.value?.id === note.id) {
    current.value = null
    plainContent.value = ''
  }
  await loadNoteCounts()
  await refreshAndOpen()
}
function confirmHardDelete(note: NoteList) {
  return new Promise<boolean>((resolve) => {
    hardDeleteTarget.value = note
    hardDeleteDialogVisible.value = true
    hardDeleteResolver = resolve
  })
}
function closeHardDeleteDialog(confirmed: boolean) {
  hardDeleteDialogVisible.value = false
  hardDeleteTarget.value = null
  hardDeleteResolver?.(confirmed)
  hardDeleteResolver = null
}
async function deleteCurrentNote() {
  if (!current.value) return
  await deleteNote(current.value)
}
async function restoreNote(note: NoteList) {
  await notesApi.restore(note.id)
  if (current.value?.id === note.id) {
    current.value = null
    plainContent.value = ''
  }
  await loadNoteCounts()
  await refreshAndOpen()
}
async function restoreCurrentNote() {
  if (!current.value) return
  await restoreNote(current.value)
}
async function encryptCurrent() {
  if (!current.value || !encryptPassword.value) return false
  const encrypted = await encryptContent(plainContent.value, encryptPassword.value)
  const saved = await notesApi.update(current.value.id, { ...current.value, content: '', isEncrypted: true, ...encrypted, tagIds: current.value.tags.map(t => t.id) })
  current.value = saved
  plainContent.value = ''
  encryptPassword.value = ''
  encryptDialog.value = false
  await loadNoteCounts()
  await loadNotes()
}
async function decryptCurrent() {
  if (!current.value?.cipherText || !current.value.salt || !current.value.iv) return false
  try {
    plainContent.value = await decryptContent(current.value.cipherText, decryptPassword.value, current.value.salt, current.value.iv)
    if (current.value.contentType === 'html') editor.value?.commands.setContent(plainContent.value || '<p></p>')
    decryptPassword.value = ''
    decryptDialog.value = false
  } catch {
    message.error('密码不正确')
    return false
  }
}
async function uploadMarkdownImages(files: File[], callback: (urls: string[]) => void) {
  const urls = await Promise.all(files.map(f => uploadApi.file(f, current.value?.id).then(x => x.fileUrl)))
  callback(urls)
}
function onKey(e: KeyboardEvent) {
  if (e.key === 'Escape') {
    tagPickerVisible.value = false
    folderPickerVisible.value = false
  }
  if ((e.metaKey || e.ctrlKey) && e.key.toLowerCase() === 's') {
    e.preventDefault()
    saveNow()
  }
  if ((e.metaKey || e.ctrlKey) && e.key.toLowerCase() === 'k') {
    e.preventDefault()
    document.querySelector<HTMLInputElement>('.search-shell input')?.focus()
  }
}
function logout() {
  auth.logout()
  router.push('/login')
}

function formatDateTime(value?: string | null) {
  if (!value) return ''
  const date = parseApiDate(value)
  return date.toLocaleString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    hour12: false,
  })
}

function parseApiDate(value: string) {
  const hasTimezone = /(?:z|[+-]\d{2}:?\d{2})$/i.test(value)
  return new Date(hasTimezone ? value : `${value}Z`)
}

function markdownToHtml(markdown: string) {
  const lines = markdown.split(/\r?\n/)
  const html: string[] = []
  let listOpen = false
  const closeList = () => {
    if (!listOpen) return
    html.push('</ul>')
    listOpen = false
  }

  for (const rawLine of lines) {
    const line = rawLine.trim()
    if (!line) {
      closeList()
      continue
    }
    const heading = line.match(/^(#{1,3})\s+(.+)$/)
    if (heading) {
      closeList()
      html.push(`<h${heading[1].length}>${escapeHtml(heading[2])}</h${heading[1].length}>`)
      continue
    }
    const listItem = line.match(/^[-*]\s+(.+)$/)
    if (listItem) {
      if (!listOpen) {
        html.push('<ul>')
        listOpen = true
      }
      html.push(`<li>${escapeHtml(listItem[1])}</li>`)
      continue
    }
    closeList()
    html.push(`<p>${escapeHtml(line)}</p>`)
  }
  closeList()
  return html.join('')
}

function htmlToMarkdown(html: string) {
  const doc = new DOMParser().parseFromString(html || '', 'text/html')
  const lines: string[] = []
  const walk = (node: Node) => {
    if (node.nodeType === Node.TEXT_NODE) {
      const text = node.textContent?.trim()
      if (text) lines.push(text)
      return
    }
    if (!(node instanceof HTMLElement)) return
    const text = node.textContent?.trim() || ''
    if (!text) return
    if (/^H[1-6]$/.test(node.tagName)) {
      lines.push(`${'#'.repeat(Number(node.tagName.slice(1)))} ${text}`)
      lines.push('')
      return
    }
    if (node.tagName === 'LI') {
      lines.push(`- ${text}`)
      return
    }
    if (node.tagName === 'P' || node.tagName === 'DIV') {
      lines.push(text)
      lines.push('')
      return
    }
    Array.from(node.childNodes).forEach(walk)
  }
  Array.from(doc.body.childNodes).forEach(walk)
  return lines.join('\n').replace(/\n{3,}/g, '\n\n').trim()
}

function escapeHtml(value: string) {
  return value
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#039;')
}
</script>

<style scoped>
.workspace {
  height: 100vh;
  display: grid;
  grid-template-columns: 248px 360px minmax(0, 1fr);
  overflow: hidden;
  background: #f7f8fc;
  color: #111827;
  font-family: Inter, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
  --nv-primary: #635bff;
  --nv-primary-2: #7c3aed;
  --nv-primary-soft: #eef0ff;
  --nv-primary-border: #bdb7ff;
  --nv-primary-glow: rgba(99, 91, 255, 0.24);
  transition: background 0.22s ease, color 0.22s ease;
}

.theme-blue {
  --nv-primary: #2563eb;
  --nv-primary-2: #06b6d4;
  --nv-primary-soft: #eff6ff;
  --nv-primary-border: #93c5fd;
  --nv-primary-glow: rgba(37, 99, 235, 0.24);
}

.theme-emerald {
  --nv-primary: #059669;
  --nv-primary-2: #10b981;
  --nv-primary-soft: #ecfdf5;
  --nv-primary-border: #6ee7b7;
  --nv-primary-glow: rgba(5, 150, 105, 0.24);
}

.theme-rose {
  --nv-primary: #e11d48;
  --nv-primary-2: #f97316;
  --nv-primary-soft: #fff1f2;
  --nv-primary-border: #fda4af;
  --nv-primary-glow: rgba(225, 29, 72, 0.24);
}

button,
input,
a {
  font-family: inherit;
}

button {
  appearance: none;
}

.editor-column {
  min-width: 0;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  background: #fff;
  transition: background 0.22s ease;
}

.doc-footer {
  padding: 14px 32px 0;
  text-align: right;
  color: #9ca3af;
  font-size: 13px;
}

.editor-empty {
  margin: auto;
  display: grid;
  justify-items: center;
  gap: 12px;
  color: #8b95a6;
}

.workspace.is-fullscreen {
  grid-template-columns: minmax(0, 1fr);
}

.workspace.is-fullscreen .sidebar,
.workspace.is-fullscreen .note-column {
  display: none;
}

.workspace.is-fullscreen .editor-column {
  min-width: 0;
  width: 100%;
}

.modal-layer {
  position: fixed;
  inset: 0;
  z-index: 3000;
  display: grid;
  place-items: center;
  padding: 24px;
  background: rgba(17, 24, 39, 0.42);
  backdrop-filter: blur(10px);
}

.action-modal {
  position: relative;
  width: min(500px, calc(100vw - 32px));
  overflow: hidden;
  border-radius: 18px;
  background: rgba(255, 255, 255, 0.98);
  box-shadow: 0 18px 48px rgba(17, 24, 39, 0.18);
  backdrop-filter: blur(16px);
  color: #111827;
  animation: modalIn 0.18s ease both;
}

.compact-modal {
  width: min(460px, calc(100vw - 32px));
}

.settings-modal {
  width: min(560px, calc(100vw - 32px));
}

.action-modal-close {
  position: absolute;
  top: 18px;
  right: 18px;
  width: 34px;
  height: 34px;
  display: grid;
  place-items: center;
  border: 0;
  border-radius: 10px;
  background: transparent;
  color: #6b7280;
  cursor: pointer;
  transition: background 0.18s ease, color 0.18s ease, transform 0.18s ease;
}

.action-modal-close:hover {
  background: #f3f4f6;
  color: #111827;
  transform: rotate(4deg);
}

.action-modal-header {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 28px 28px 22px;
}

.action-modal-icon {
  width: 52px;
  height: 52px;
  display: grid;
  place-items: center;
  border-radius: 999px;
  color: #fff;
  background: radial-gradient(
    circle at 50% 50%,
    color-mix(in srgb, var(--nv-primary, #635bff) 16%, transparent),
    color-mix(in srgb, var(--nv-primary, #635bff) 8%, transparent)
  );
}

.action-modal-icon svg {
  width: 28px;
  height: 28px;
  padding: 6px;
  border-radius: 8px;
  background: linear-gradient(135deg, var(--nv-primary, #635bff), var(--nv-primary-2, #7c3aed));
  box-shadow: 0 8px 18px var(--nv-primary-glow, rgba(99, 91, 255, 0.24));
}

.warning-icon svg {
  background: linear-gradient(135deg, #f59e0b, #ef4444);
  box-shadow: 0 8px 18px rgba(245, 158, 11, 0.24);
}

.action-modal h2 {
  margin: 0 0 6px;
  color: #111827;
  font-size: 24px;
  line-height: 1.2;
  font-weight: 800;
}

.action-modal p {
  margin: 0;
  color: #6b7280;
  font-size: 15px;
}

.action-modal-label {
  display: block;
  margin: 0 28px 10px;
  color: #111827;
  font-size: 16px;
  font-weight: 800;
}

.action-input-shell {
  height: 48px;
  display: flex;
  align-items: center;
  gap: 12px;
  margin: 0 28px 28px;
  padding: 0 14px;
  border: 2px solid var(--nv-primary, #635bff);
  border-radius: 12px;
  background: #fff;
  box-shadow: 0 0 0 3px color-mix(in srgb, var(--nv-primary, #635bff) 10%, transparent);
  cursor: text;
}

.action-input-shell svg {
  flex: 0 0 auto;
  color: #6b7280;
}

.action-input-shell input {
  min-width: 0;
  flex: 1;
  border: 0;
  outline: 0;
  background: transparent;
  color: #111827;
  font-size: 15px;
  font-weight: 500;
}

.action-input-shell input::placeholder {
  color: #9ca3af;
}

.action-input-shell span {
  flex: 0 0 auto;
  color: #9ca3af;
  font-size: 14px;
}

.type-choice-grid {
  display: grid;
  gap: 12px;
  padding: 0 28px 28px;
}

.default-type-hint {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  margin: 0 28px 12px;
  padding: 10px 12px;
  border-radius: 12px;
  background: #f7f8fc;
  color: #6b7280;
  font-size: 13px;
}

.default-type-hint button {
  border: 0;
  background: transparent;
  color: var(--nv-primary, #4f46e5);
  cursor: pointer;
  font-size: 13px;
  font-weight: 800;
}

.type-choice-grid button {
  min-height: 82px;
  display: grid;
  gap: 7px;
  padding: 16px 18px;
  border: 1px solid #e5e7eb;
  border-radius: 16px;
  background: #fff;
  color: #111827;
  cursor: pointer;
  text-align: left;
  transition: transform 0.18s ease, border-color 0.18s ease, box-shadow 0.18s ease, background 0.18s ease;
}

.type-choice-grid button:hover {
  border-color: var(--nv-primary-border, #bdb7ff);
  background: color-mix(in srgb, var(--nv-primary-soft, #eef0ff) 62%, white);
  box-shadow: 0 12px 28px color-mix(in srgb, var(--nv-primary, #635bff) 12%, transparent);
  transform: translateY(-1px);
}

.type-choice-grid button.active {
  border-color: var(--nv-primary-border, #bdb7ff);
  background: var(--nv-primary-soft, #eef0ff);
}

.type-choice-grid strong {
  font-size: 16px;
  font-weight: 800;
}

.type-choice-grid span {
  color: #6b7280;
  font-size: 13px;
  line-height: 1.6;
}

.convert-summary {
  margin: 0 28px 28px;
  padding: 14px 16px;
  border: 1px solid var(--nv-primary-border, #bdb7ff);
  border-radius: 14px;
  background: var(--nv-primary-soft, #eef0ff);
  color: var(--nv-primary, #4f46e5);
  font-size: 15px;
  font-weight: 800;
}

.settings-body {
  display: grid;
  gap: 12px;
  padding: 0 28px 28px;
}

.settings-section {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 16px;
  border: 1px solid #e5e7eb;
  border-radius: 16px;
  background: #fff;
}

.settings-section.vertical {
  align-items: stretch;
  flex-direction: column;
}

.settings-section strong {
  display: block;
  color: #111827;
  font-size: 15px;
  font-weight: 800;
}

.settings-section span {
  display: block;
  margin-top: 4px;
  color: #6b7280;
  font-size: 13px;
}

.settings-options {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.settings-options button,
.switch-button {
  min-height: 34px;
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 0 12px;
  border: 1px solid #e5e7eb;
  border-radius: 10px;
  background: #fff;
  color: #374151;
  cursor: pointer;
  font-size: 13px;
  font-weight: 700;
  transition: background 0.18s ease, border-color 0.18s ease, color 0.18s ease, transform 0.18s ease;
}

.settings-options button:hover,
.switch-button:hover {
  transform: translateY(-1px);
  border-color: var(--nv-primary-border, #bdb7ff);
}

.settings-options button.active,
.switch-button.active {
  border-color: var(--nv-primary-border, #bdb7ff);
  background: var(--nv-primary-soft, #eef0ff);
  color: var(--nv-primary, #4f46e5);
}

.theme-swatch {
  width: 14px;
  height: 14px;
  border-radius: 999px;
  box-shadow: inset 0 0 0 1px rgba(255, 255, 255, 0.42), 0 3px 8px rgba(17, 24, 39, 0.12);
}

.theme-swatch.theme-violet {
  background: linear-gradient(135deg, #635bff, #7c3aed);
}

.theme-swatch.theme-blue {
  background: linear-gradient(135deg, #2563eb, #06b6d4);
}

.theme-swatch.theme-emerald {
  background: linear-gradient(135deg, #059669, #10b981);
}

.theme-swatch.theme-rose {
  background: linear-gradient(135deg, #e11d48, #f97316);
}

.action-modal-actions {
  height: 86px;
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 12px;
  padding: 0 28px;
  border-top: 1px solid #e5e7eb;
  background: rgba(250, 251, 255, 0.72);
}

.settings-actions .danger-text {
  margin-right: auto;
}

.action-cancel,
.action-confirm {
  height: 40px;
  min-width: 84px;
  padding: 0 20px;
  border-radius: 12px;
  cursor: pointer;
  font-size: 15px;
  font-weight: 700;
  transition: transform 0.18s ease, box-shadow 0.18s ease, background 0.18s ease;
}

.action-cancel {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  border: 1px solid #e5e7eb;
  color: #111827;
  background: #fff;
}

.danger-text {
  color: #ef4444;
}

.action-cancel:hover {
  background: #f3f4f6;
}

.action-confirm {
  border: 0;
  color: #fff;
  background: linear-gradient(135deg, var(--nv-primary, #635bff), var(--nv-primary-2, #7c3aed));
  box-shadow: 0 14px 30px color-mix(in srgb, var(--nv-primary, #635bff) 24%, transparent);
}

.action-confirm:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 18px 34px color-mix(in srgb, var(--nv-primary, #635bff) 30%, transparent);
}

.action-confirm:disabled {
  opacity: 0.48;
  cursor: not-allowed;
  box-shadow: none;
}

@keyframes modalIn {
  from {
    opacity: 0;
    transform: translateY(8px) scale(0.98);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}

@media (max-width: 900px) {
  .workspace {
    grid-template-columns: 360px minmax(0, 1fr);
  }

  .workspace .sidebar {
    display: none;
  }
}

@media (max-width: 640px) {
  .workspace {
    display: block;
    overflow: auto;
  }

  .workspace .note-column,
  .workspace .editor-column {
    min-height: 100vh;
  }
}

.workspace.is-dark {
  background: #0f1117;
  color: #e5e7eb;
}

.workspace.is-dark .editor-column,
.workspace.is-dark .editor-empty {
  background: #171a22;
}

.workspace.is-dark .editor-column {
  background: #171a22 !important;
}

.workspace.is-dark .doc-footer,
.workspace.is-dark .editor-empty,
.workspace.is-dark .action-modal p,
.workspace.is-dark .type-choice-grid span,
.workspace.is-dark .settings-section span,
.workspace.is-dark .default-type-hint {
  color: #cbd5e1;
}

.workspace.is-dark .action-modal,
.workspace.is-dark .action-input-shell,
.workspace.is-dark .type-choice-grid button,
.workspace.is-dark .settings-section,
.workspace.is-dark .settings-options button,
.workspace.is-dark .switch-button,
.workspace.is-dark .default-type-hint {
  border-color: #303747;
  background: #1a1f2b;
  color: #f8fafc;
}

.workspace.is-dark .action-modal-actions {
  border-top-color: #303747;
  background: rgba(18, 22, 33, 0.72);
}

.workspace.is-dark .action-modal h2,
.workspace.is-dark .action-modal-label,
.workspace.is-dark .action-input-shell input,
.workspace.is-dark .type-choice-grid strong,
.workspace.is-dark .settings-section strong {
  color: #f8fafc;
}

.workspace.is-dark .action-modal-close,
.workspace.is-dark .action-input-shell svg {
  color: #cbd5e1;
}

.workspace.is-dark .action-modal-close:hover,
.workspace.is-dark .action-cancel:hover,
.workspace.is-dark .type-choice-grid button:hover,
.workspace.is-dark .settings-options button:hover,
.workspace.is-dark .switch-button:hover {
  background: #242b38;
}

.workspace.is-dark .action-cancel {
  border-color: #303747;
  background: #242b38;
  color: #f8fafc;
}

.workspace.is-dark .convert-summary {
  border-color: color-mix(in srgb, var(--nv-primary, #635bff) 62%, white);
  background: color-mix(in srgb, var(--nv-primary, #635bff) 22%, transparent);
  color: color-mix(in srgb, var(--nv-primary, #635bff) 48%, white);
}

.workspace.is-dark .type-choice-grid button.active,
.workspace.is-dark .settings-options button.active,
.workspace.is-dark .switch-button.active {
  border-color: color-mix(in srgb, var(--nv-primary, #635bff) 62%, white);
  background: color-mix(in srgb, var(--nv-primary, #635bff) 22%, transparent);
  color: color-mix(in srgb, var(--nv-primary, #635bff) 48%, white);
}
</style>
