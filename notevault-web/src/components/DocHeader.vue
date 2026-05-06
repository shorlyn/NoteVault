<template>
  <div class="doc-header" :class="{ 'is-dark': dark }">
    <input
      v-if="editing"
      :value="title"
      class="title-input"
      placeholder="随便记一个吧"
      @input="onTitleInput"
    />
    <h1 v-else class="title-display">{{ title || '未命名笔记' }}</h1>
    <div class="meta-row">
      <div class="meta-menu-wrap" ref="folderPickerRef">
        <button class="meta-chip" type="button" :disabled="!editing" @click.stop="toggleFolderPicker">
          <FolderIcon :size="14" />
          {{ currentFolderName }}
        </button>
        <div v-if="editing && folderPickerVisible" class="meta-picker" @click.stop>
          <button
            v-for="folder in folderRows"
            :key="folder.id || 'root-folder'"
            type="button"
            :class="{ active: (folderId || '') === folder.id }"
            :style="{ '--folder-depth': folder.depth }"
            @click="$emit('setFolder', folder.id)"
          >
            <FolderIcon :size="14" />
            <span>{{ folder.name }}</span>
          </button>
        </div>
      </div>

      <span v-for="tag in noteTags" :key="tag.id" class="meta-chip purple">
        <span class="tag-dot" />
        {{ tag.name }}
        <button v-if="editing" type="button" title="移除标签" @click="$emit('removeTag', tag.id)">
          <X :size="12" />
        </button>
      </span>

      <div v-if="editing" class="meta-menu-wrap" ref="tagPickerRef">
        <button class="meta-chip" type="button" @click.stop="toggleTagPicker">添加标签 +</button>
        <div v-if="tagPickerVisible" class="meta-picker tag-picker" @click.stop>
          <button
            v-for="tag in tags"
            :key="tag.id"
            type="button"
            :class="{ active: currentHasTag(tag.id) }"
            @click="$emit('toggleTag', tag)"
          >
            <span class="tag-dot" />
            <span>{{ tag.name }}</span>
            <Check v-if="currentHasTag(tag.id)" :size="14" />
          </button>
          <button class="picker-create" type="button" @click="$emit('createTag')">
            <Plus :size="14" />
            新建标签
          </button>
        </div>
      </div>

      <span v-if="editing" class="autosave-dot" />
      <span v-if="editing" class="autosave-text">自动保存中...</span>
    </div>

    <div class="mode-row">
      <span class="type-badge">{{ contentType === 'markdown' ? 'Markdown 文档' : '富文本 文档' }}</span>
      <button
        v-if="contentType === 'markdown' && editing"
        class="text-button"
        :class="{ active: showPreview }"
        type="button"
        @click="$emit('togglePreview')"
      >
        {{ showPreview ? '关闭预览' : '预览' }}
      </button>
      <button v-if="editing" class="text-button convert-button" type="button" @click="$emit('convertContentType')">
        转换为{{ contentType === 'markdown' ? '富文本' : 'Markdown' }}
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue'
import { Check, Folder as FolderIcon, Plus, X } from 'lucide-vue-next'
import type { ContentType, Folder, Tag } from '../types'

type FolderRow = Folder & { depth: number; hasChildren: boolean }

const props = defineProps<{
  title: string
  folderId?: string | null
  currentFolderName: string
  folderRows: FolderRow[]
  tags: Tag[]
  noteTags: Tag[]
  contentType: ContentType
  showPreview: boolean
  editing: boolean
  folderPickerVisible: boolean
  tagPickerVisible: boolean
  dark: boolean
}>()

const emit = defineEmits<{
  'update:title': [value: string]
  'update:folderPickerVisible': [value: boolean]
  'update:tagPickerVisible': [value: boolean]
  setFolder: [id: string]
  removeTag: [id: string]
  toggleTag: [tag: Tag]
  createTag: []
  convertContentType: []
  togglePreview: []
}>()

const folderPickerRef = ref<HTMLElement | null>(null)
const tagPickerRef = ref<HTMLElement | null>(null)

onMounted(() => {
  window.addEventListener('pointerdown', onGlobalPointerDown)
})

onUnmounted(() => {
  window.removeEventListener('pointerdown', onGlobalPointerDown)
})

function onTitleInput(event: Event) {
  emit('update:title', (event.target as HTMLInputElement).value)
}

function currentHasTag(id: string) {
  return props.noteTags.some(tag => tag.id === id)
}

function toggleFolderPicker() {
  if (!props.editing) return
  emit('update:folderPickerVisible', !props.folderPickerVisible)
  emit('update:tagPickerVisible', false)
}

function toggleTagPicker() {
  if (!props.editing) return
  emit('update:tagPickerVisible', !props.tagPickerVisible)
  emit('update:folderPickerVisible', false)
}

function onGlobalPointerDown(event: PointerEvent) {
  const target = event.target as Node
  if (props.folderPickerVisible && folderPickerRef.value && !folderPickerRef.value.contains(target)) {
    emit('update:folderPickerVisible', false)
  }
  if (props.tagPickerVisible && tagPickerRef.value && !tagPickerRef.value.contains(target)) {
    emit('update:tagPickerVisible', false)
  }
}
</script>

<style scoped>
.doc-header {
  padding: 24px 32px 0;
  transition: background 0.22s ease;
}

.title-input {
  width: 100%;
  height: 44px;
  border: 0;
  outline: 0;
  color: #111827;
  background: transparent;
  font-size: 28px;
  line-height: 1.2;
  font-weight: 800;
}

.title-display {
  min-height: 44px;
  margin: 0;
  color: #111827;
  font-size: 28px;
  line-height: 1.2;
  font-weight: 800;
  word-break: break-word;
}

.meta-row,
.mode-row {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-top: 12px;
  position: relative;
  flex-wrap: wrap;
}

.meta-chip {
  height: 32px;
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 0 14px;
  border: 1px solid transparent;
  border-radius: 10px;
  background: #f7f8fc;
  color: #6b7280;
  font-size: 14px;
}

.meta-chip:disabled {
  cursor: default;
}

.meta-chip button {
  width: 18px;
  height: 18px;
  display: grid;
  place-items: center;
  margin-right: -4px;
  border: 0;
  border-radius: 999px;
  background: transparent;
  color: currentColor;
  cursor: pointer;
}

.meta-chip button:hover {
  background: color-mix(in srgb, var(--nv-primary, #635bff) 12%, transparent);
}

.meta-chip.purple {
  border-color: var(--nv-primary-border, #bdb7ff);
  background: var(--nv-primary-soft, #eef0ff);
  color: var(--nv-primary, #4f46e5);
}

.tag-dot {
  width: 7px;
  height: 7px;
  display: inline-block;
  border-radius: 999px;
  background: currentColor;
}

.meta-menu-wrap {
  position: relative;
  display: inline-flex;
}

.meta-picker {
  position: absolute;
  z-index: 30;
  top: calc(100% + 8px);
  left: 0;
  width: 220px;
  max-height: 280px;
  overflow: auto;
  padding: 8px;
  border: 1px solid #e5e7eb;
  border-radius: 14px;
  background: #fff;
  box-shadow: 0 18px 44px rgba(17, 24, 39, 0.14);
  animation: pickerPop 0.16s ease both;
}

.meta-picker button {
  width: 100%;
  height: 34px;
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 0 10px 0 calc(10px + var(--folder-depth, 0) * 14px);
  border: 0;
  border-radius: 10px;
  background: transparent;
  color: #374151;
  cursor: pointer;
  font-size: 13px;
  text-align: left;
}

.meta-picker button:hover,
.meta-picker button.active {
  background: var(--nv-primary-soft, #eef0ff);
  color: var(--nv-primary, #4f46e5);
}

.meta-picker button span {
  min-width: 0;
  flex: 1;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.meta-picker .picker-create {
  margin-top: 6px;
  border-top: 1px solid #e5e7eb;
  border-radius: 0;
  color: var(--nv-primary, #635bff);
  font-weight: 700;
}

.autosave-dot {
  width: 6px;
  height: 6px;
  margin-left: auto;
  border-radius: 999px;
  background: #2fbd83;
}

.autosave-text {
  color: #7b8493;
  font-size: 12px;
}

.mode-row {
  gap: 18px;
}

.type-badge,
.text-button {
  height: 32px;
  display: inline-flex;
  align-items: center;
  padding: 0 14px;
  border: 1px solid transparent;
  border-radius: 10px;
  color: #6b7280;
  font-size: 14px;
}

.type-badge {
  border-color: var(--nv-primary-border, #bdb7ff);
  background: var(--nv-primary-soft, #eef0ff);
  color: var(--nv-primary, #4f46e5);
  font-weight: 700;
}

.text-button {
  background: transparent;
  cursor: pointer;
  transition: background 0.18s ease, border-color 0.18s ease, color 0.18s ease, transform 0.18s ease;
}

.text-button:hover {
  background: #f3f4f6;
  transform: translateY(-1px);
}

.text-button.active {
  border-color: var(--nv-primary-border, #bdb7ff);
  background: var(--nv-primary-soft, #eef0ff);
  color: var(--nv-primary, #4f46e5);
}

.convert-button {
  border-color: #e5e7eb;
}

@keyframes pickerPop {
  from {
    opacity: 0;
    transform: translateY(-4px) scale(0.98);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}

.doc-header.is-dark {
  background: #171a22 !important;
}

.doc-header.is-dark .title-input,
.doc-header.is-dark .title-display {
  color: #f8fafc !important;
}

.doc-header.is-dark .type-badge,
.doc-header.is-dark .text-button,
.doc-header.is-dark .meta-chip,
.doc-header.is-dark .autosave-text {
  color: #cbd5e1 !important;
}

.doc-header.is-dark .meta-chip,
.doc-header.is-dark .meta-picker {
  border-color: #303747 !important;
  background: #1a1f2b !important;
}

.doc-header.is-dark .text-button:hover,
.doc-header.is-dark .meta-picker button:hover {
  background: #242b38 !important;
}

.doc-header.is-dark .type-badge,
.doc-header.is-dark .text-button.active,
.doc-header.is-dark .meta-picker button.active {
  border-color: color-mix(in srgb, var(--nv-primary, #635bff) 62%, white) !important;
  background: color-mix(in srgb, var(--nv-primary, #635bff) 22%, transparent) !important;
  color: color-mix(in srgb, var(--nv-primary, #635bff) 48%, white) !important;
}

.doc-header.is-dark .meta-picker button {
  color: #cbd5e1 !important;
}

.doc-header.is-dark .meta-picker .picker-create {
  border-top-color: #303747 !important;
  color: color-mix(in srgb, var(--nv-primary, #635bff) 48%, white) !important;
}
</style>
