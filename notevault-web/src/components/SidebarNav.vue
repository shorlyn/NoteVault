<template>
  <aside class="sidebar" :class="{ 'is-dark': dark }">
    <header class="brand-row">
      <div class="brand-mark">N</div>
      <div class="brand-name">NoteVault</div>
      <div class="theme-menu" ref="themeMenuRef">
        <button class="icon-button ml-auto" type="button" :title="`主题色：${themeLabel}`" @click.stop="themeMenuOpen = !themeMenuOpen">
          <Palette :size="17" />
        </button>
        <div v-if="themeMenuOpen" class="theme-popover" @click.stop>
          <button
            v-for="option in themeOptions"
            :key="option.value"
            class="theme-option"
            :class="{ active: theme === option.value }"
            type="button"
            @click="selectTheme(option.value)"
          >
            <span class="theme-swatch" :class="`theme-${option.value}`" />
            {{ option.label }}
            <Check v-if="theme === option.value" :size="14" />
          </button>
        </div>
      </div>
      <button class="icon-button" type="button" title="切换主题" @click="$emit('toggleDark')">
        <Moon :size="17" />
      </button>
    </header>

    <button class="create-button" type="button" @click="$emit('createNote')">
      <Plus :size="18" />
      新建笔记
      <ChevronDown :size="16" class="ml-auto opacity-80" />
    </button>

    <nav class="nav-section">
      <button class="nav-item" :class="{ 'is-active': activeView === 'all' }" type="button" @click="$emit('showAll')">
        <Files :size="17" />
        全部笔记
        <span>{{ activeNoteCount }}</span>
      </button>
      <button class="nav-item" :class="{ 'is-active': activeView === 'recent' }" type="button" @click="$emit('showRecent')">
        <Clock3 :size="17" />
        最近打开
        <span>{{ recentCount }}</span>
      </button>
      <button class="nav-item" :class="{ 'is-active': activeView === 'favorite' }" type="button" @click="$emit('showFavorites')">
        <Star :size="17" />
        收藏夹
        <span>{{ pinnedCount }}</span>
      </button>
      <button class="nav-item" :class="{ 'is-active': activeView === 'trash' }" type="button" @click="$emit('showTrash')">
        <Trash2 :size="17" />
        回收站
        <span>{{ trashCount || '' }}</span>
      </button>
    </nav>

    <div class="section-heading">
      <span>文件夹</span>
      <button class="tiny-button" type="button" @click="$emit('createFolder', null)"><Plus :size="14" /></button>
    </div>
    <n-scrollbar class="folder-scroll sidebar-group">
      <div class="folder-list">
        <button
          v-for="folder in folderRows"
          :key="folder.id || 'all'"
          class="folder-row"
          :class="{ 'is-selected': selectedFolder === folder.id }"
          :style="{ '--folder-depth': folder.depth }"
          type="button"
          @click="$emit('selectFolder', folder.id)"
        >
          <ChevronDown v-if="folder.hasChildren" class="folder-caret" :size="14" />
          <ChevronRight v-else class="folder-caret muted" :size="14" />
          <FolderIcon :size="18" />
          <span>{{ folder.name }}</span>
          <em>{{ folder.id ? folderNoteCount(folder.id) : activeNoteCount }}</em>
        </button>
      </div>
    </n-scrollbar>

    <div class="section-heading">
      <span>标签</span>
      <button class="tiny-button" type="button" @click="$emit('createTag')"><Plus :size="14" /></button>
    </div>
    <div class="tag-list sidebar-group">
      <button
        v-for="tag in tags"
        :key="tag.id"
        class="tag-filter"
        :class="{ 'is-selected': selectedTag === tag.id }"
        type="button"
        @click="$emit('toggleTag', tag.id)"
      >
        <span class="tag-pill">{{ tag.name }}</span>
        <em>{{ tagNoteCount(tag.id) }}</em>
      </button>
    </div>

    <div class="sidebar-footer">
      <a class="github-sidebar" href="https://github.com/shorlyn/NoteVault.git" target="_blank" rel="noreferrer">
        <Github :size="17" />
        GitHub
      </a>
      <button class="settings-button" type="button" @click="$emit('openSettings')">
        <Settings :size="17" />
        设置
      </button>
    </div>
  </aside>
</template>

<script setup lang="ts">
import {
  ChevronDown,
  ChevronRight,
  Check,
  Clock3,
  Files,
  Folder as FolderIcon,
  Github,
  Moon,
  Palette,
  Plus,
  Settings,
  Star,
  Trash2,
} from 'lucide-vue-next'
import type { Folder, Tag } from '../types'
import { computed, onMounted, onUnmounted, ref } from 'vue'

type ActiveView = 'all' | 'recent' | 'favorite' | 'trash'
type FolderRow = Folder & { depth: number; hasChildren: boolean }

type AccentTheme = 'violet' | 'blue' | 'emerald' | 'rose'

const themeOptions: Array<{ value: AccentTheme; label: string }> = [
  { value: 'violet', label: '紫色' },
  { value: 'blue', label: '蓝色' },
  { value: 'emerald', label: '绿色' },
  { value: 'rose', label: '玫红' },
]

const themeLabelMap: Record<AccentTheme, string> = {
  violet: '紫色',
  blue: '蓝色',
  emerald: '绿色',
  rose: '玫红',
}

const props = defineProps<{
  activeView: ActiveView
  activeNoteCount: number
  recentCount: number
  pinnedCount: number
  trashCount: number
  folderRows: FolderRow[]
  tags: Tag[]
  selectedFolder: string
  selectedTag: string
  folderNoteCount: (id: string) => number | string
  tagNoteCount: (id: string) => number | string
  dark: boolean
  theme: AccentTheme
}>()

const emit = defineEmits<{
  toggleDark: []
  setTheme: [theme: AccentTheme]
  createNote: []
  showAll: []
  showRecent: []
  showFavorites: []
  showTrash: []
  createFolder: [parentId: string | null]
  createTag: []
  selectFolder: [id: string]
  toggleTag: [id: string]
  openSettings: []
}>()

const themeLabel = computed(() => themeLabelMap[props.theme])
const themeMenuOpen = ref(false)
const themeMenuRef = ref<HTMLElement | null>(null)

onMounted(() => {
  window.addEventListener('pointerdown', closeThemeMenu)
})

onUnmounted(() => {
  window.removeEventListener('pointerdown', closeThemeMenu)
})

function selectTheme(theme: AccentTheme) {
  emit('setTheme', theme)
  themeMenuOpen.value = false
}

function closeThemeMenu(event: PointerEvent) {
  if (themeMenuRef.value?.contains(event.target as Node)) return
  themeMenuOpen.value = false
}
</script>

<style scoped>
.sidebar {
  width: 248px;
  display: flex;
  flex-direction: column;
  min-height: 0;
  padding: 20px 16px;
  border-right: 1px solid #e5e7eb;
  background: #fff;
  transition: background 0.22s ease, border-color 0.22s ease, color 0.22s ease;
}

.brand-row {
  height: 48px;
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 18px;
}

.brand-mark {
  width: 32px;
  height: 32px;
  display: grid;
  place-items: center;
  border-radius: 10px;
  color: #fff;
  background: linear-gradient(135deg, var(--nv-primary, #635bff), var(--nv-primary-2, #7c3aed));
  box-shadow: 0 12px 32px var(--nv-primary-glow, rgba(99, 91, 255, 0.24));
  font-weight: 800;
  transition: transform 0.18s ease, box-shadow 0.22s ease, background 0.22s ease;
}

.brand-mark:hover {
  transform: rotate(-3deg) scale(1.04);
}

.brand-name {
  color: #111827;
  font-size: 20px;
  font-weight: 700;
  letter-spacing: 0;
}

.create-button,
.nav-item,
.tag-filter,
.github-sidebar,
.settings-button,
.tiny-button,
.icon-button {
  appearance: none;
  border: 0;
  font: inherit;
  cursor: pointer;
}

.create-button {
  width: 100%;
  height: 40px;
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 14px; /* 这里的UI不能动，会让按钮高度不对 */
  border-radius: 10px;
  color: #fff;
  background: linear-gradient(135deg, var(--nv-primary, #635bff), var(--nv-primary-2, #7c3aed));
  box-shadow: 0 12px 32px color-mix(in srgb, var(--nv-primary, #635bff) 24%, transparent);
  font-size: 14px;
  font-weight: 650;
  transition: transform 0.18s ease, box-shadow 0.18s ease;
}

.create-button:hover {
  transform: translateY(-1px);
  box-shadow: 0 16px 34px color-mix(in srgb, var(--nv-primary, #635bff) 34%, transparent);
}

.create-button:active {
  transform: translateY(0) scale(0.99);
}

.nav-section {
  padding: 18px 0 14px;
  border-bottom: 1px solid #e5e7eb;
}

.nav-item,
.tag-filter,
.github-sidebar,
.settings-button {
  height: 36px;
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 0 12px;
  border-radius: 10px;
  color: #374151;
  background: transparent;
  font-size: 14px;
  text-align: left;
  transition: background 0.18s ease, color 0.18s ease, transform 0.18s ease;
}

.nav-item {
  width: 100%;
}

.nav-item svg,
.github-sidebar svg,
.settings-button svg {
  width: 18px;
  height: 18px;
}

.nav-item span,
.tag-filter em {
  margin-left: auto;
  color: #6b7280;
  font-style: normal;
  font-size: 12px;
}

.nav-item:hover,
.tag-filter:hover,
.github-sidebar:hover,
.settings-button:hover {
  background: #f3f4f6;
  transform: translateX(2px);
}

.nav-item.is-active {
  position: relative;
  color: var(--nv-primary, #4f46e5);
  background: var(--nv-primary-soft, #eef0ff);
}

.nav-item.is-active::before {
  content: "";
  position: absolute;
  left: 0;
  top: 8px;
  width: 3px;
  height: 20px;
  border-radius: 999px;
  background: var(--nv-primary, #635bff);
}

.section-heading {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-top: 24px;
  margin-bottom: 10px;
  color: #9ca3af;
  font-size: 12px;
  letter-spacing: 0.04em;
}

.tiny-button,
.icon-button {
  width: 28px;
  height: 28px;
  display: grid;
  place-items: center;
  border-radius: 7px;
  background: transparent;
  color: #4b5563;
}

.tiny-button:hover,
.icon-button:hover {
  background: #f1f3f8;
}

.theme-menu {
  position: relative;
  margin-left: auto;
}

.theme-popover {
  position: absolute;
  z-index: 40;
  top: calc(100% + 8px);
  right: 0;
  width: 156px;
  padding: 8px;
  border: 1px solid #e5e7eb;
  border-radius: 14px;
  background: #fff;
  box-shadow: 0 18px 44px rgba(17, 24, 39, 0.14);
  animation: menuPop 0.16s ease both;
}

.theme-option {
  width: 100%;
  height: 34px;
  display: flex;
  align-items: center;
  gap: 9px;
  padding: 0 10px;
  border: 0;
  border-radius: 10px;
  background: transparent;
  color: #374151;
  cursor: pointer;
  font: inherit;
  font-size: 13px;
}

.theme-option:hover,
.theme-option.active {
  background: var(--nv-primary-soft, #eef0ff);
  color: var(--nv-primary, #4f46e5);
}

.theme-option svg {
  margin-left: auto;
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

@keyframes menuPop {
  from {
    opacity: 0;
    transform: translateY(-4px) scale(0.98);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}

.folder-scroll {
  max-height: 218px;
}

.folder-list {
  display: grid;
  gap: 4px;
  padding: 2px 0 4px;
}

.folder-row {
  width: 100%;
  height: 34px;
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 0 10px 0 calc(8px + var(--folder-depth, 0) * 18px);
  border: 0;
  border-radius: 10px;
  background: transparent;
  color: #374151;
  cursor: pointer;
  font-size: 14px;
  text-align: left;
  transition: background 0.18s ease, color 0.18s ease, transform 0.18s ease;
}

.folder-row:hover {
  background: #f3f4f6;
  color: #111827;
  transform: translateX(2px);
}

.folder-row.is-selected {
  background: var(--nv-primary-soft, #eef0ff);
  color: var(--nv-primary, #4f46e5);
}

.folder-row svg {
  flex: 0 0 auto;
  color: #6b7280;
}

.folder-row.is-selected svg {
  color: var(--nv-primary, #635bff);
}

.folder-caret {
  width: 14px;
  color: #6b7280;
}

.folder-caret.muted {
  opacity: 0.72;
}

.folder-row span {
  min-width: 0;
  flex: 1;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  font-weight: 600;
}

.folder-row em {
  flex: 0 0 auto;
  color: #9ca3af;
  font-size: 13px;
  font-style: normal;
}

.tag-list {
  display: grid;
  gap: 8px;
}

.tag-filter {
  height: 30px;
  justify-content: space-between;
  padding: 0;
  color: var(--nv-primary, #4f46e5);
  background: transparent;
  font-weight: 700;
}

.tag-filter > span {
  width: auto;
  min-width: 0;
  height: 24px;
  display: inline-flex;
  align-items: center;
  padding: 0 10px;
  border-radius: 8px;
  background: var(--nv-primary-soft, #eef0ff);
}

.tag-filter > span::before {
  content: "";
  width: 6px;
  height: 6px;
  margin-right: 6px;
  border-radius: 999px;
  background: currentColor;
}

.tag-filter em {
  color: #9ca3af;
  font-weight: 600;
}

.tag-filter.is-selected > span,
.tag-filter:hover > span {
  background: color-mix(in srgb, var(--nv-primary-soft, #eef0ff) 70%, var(--nv-primary, #635bff)) !important;
}

.sidebar-group {
  padding-bottom: 16px;
  border-bottom: 1px solid #e5e7eb;
}

.sidebar-footer {
  margin-top: auto;
  padding-top: 12px;
  border-top: 1px solid #e5e7eb;
}

.github-sidebar,
.settings-button {
  width: 100%;
  color: #4b5563;
  text-decoration: none;
}

.github-sidebar + .settings-button {
  margin-top: 6px;
  padding-top: 6px;
  border-top: 1px solid #eef1f6;
}

.ml-auto {
  margin-left: auto;
}

.opacity-80 {
  opacity: 0.8;
}

.sidebar.is-dark {
  border-color: #2b303b !important;
  background: #171a22 !important;
}

.sidebar.is-dark .brand-name {
  color: #f8fafc !important;
}

.sidebar.is-dark .nav-section,
.sidebar.is-dark .sidebar-group,
.sidebar.is-dark .sidebar-footer,
.sidebar.is-dark .github-sidebar + .settings-button {
  border-color: #2b303b !important;
}

.sidebar.is-dark .nav-item,
.sidebar.is-dark .tag-filter,
.sidebar.is-dark .github-sidebar,
.sidebar.is-dark .settings-button,
.sidebar.is-dark .section-heading {
  color: #cbd5e1 !important;
}

.sidebar.is-dark .nav-item:hover,
.sidebar.is-dark .tag-filter:hover,
.sidebar.is-dark .github-sidebar:hover,
.sidebar.is-dark .settings-button:hover,
.sidebar.is-dark .tiny-button:hover,
.sidebar.is-dark .icon-button:hover,
.sidebar.is-dark .theme-option:hover,
.sidebar.is-dark .theme-option.active {
  background: #242b38 !important;
}

.sidebar.is-dark .theme-popover {
  border-color: #2b303b;
  background: #1a1f2b;
  box-shadow: 0 18px 44px rgba(0, 0, 0, 0.28);
}

.sidebar.is-dark .theme-option {
  color: #cbd5e1;
}

.sidebar.is-dark .theme-option.active {
  color: color-mix(in srgb, var(--nv-primary, #635bff) 48%, white) !important;
}

.sidebar.is-dark .nav-item.is-active,
.sidebar.is-dark .folder-row.is-selected {
  background: color-mix(in srgb, var(--nv-primary, #635bff) 20%, transparent) !important;
  color: color-mix(in srgb, var(--nv-primary, #635bff) 48%, white) !important;
}

.sidebar.is-dark .folder-row {
  color: #cbd5e1 !important;
}

.sidebar.is-dark .folder-row:hover {
  background: #242b38 !important;
  color: #f8fafc !important;
}

.sidebar.is-dark .folder-row em,
.sidebar.is-dark .tag-filter em {
  color: #7b8496;
}

.sidebar.is-dark .tag-filter > span {
  background: color-mix(in srgb, var(--nv-primary, #635bff) 18%, transparent) !important;
}

.sidebar.is-dark .tag-filter.is-selected > span,
.sidebar.is-dark .tag-filter:hover > span {
  background: color-mix(in srgb, var(--nv-primary, #635bff) 30%, transparent) !important;
}
</style>
