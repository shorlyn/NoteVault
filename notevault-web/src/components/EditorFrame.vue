<template>
  <div class="editor-frame" :class="{ 'is-dark': dark }">
    <div v-if="contentType === 'html'" class="format-toolbar">
      <button type="button" title="加粗" @click="runRichCommand('bold')"><Bold :size="16" /></button>
      <button type="button" title="斜体" @click="runRichCommand('italic')"><Italic :size="16" /></button>
      <button type="button" title="删除线" @click="runRichCommand('strike')"><Strikethrough :size="16" /></button>
      <button type="button" title="行内代码" @click="runRichCommand('code')"><Code2 :size="16" /></button>
      <button type="button" title="引用" @click="runRichCommand('quote')"><Quote :size="16" /></button>
      <button type="button" title="一级标题" @click="runRichCommand('h1')"><Heading1 :size="16" /></button>
      <button type="button" title="二级标题" @click="runRichCommand('h2')"><Heading2 :size="16" /></button>
      <button type="button" title="无序列表" @click="runRichCommand('bulletList')"><List :size="16" /></button>
      <button type="button" title="有序列表" @click="runRichCommand('orderedList')"><ListChecks :size="16" /></button>
      <button class="export-button" type="button">
        <Upload :size="15" />
        导出
        <ChevronDown :size="14" />
      </button>
    </div>

    <MdEditor
      v-if="contentType === 'markdown'"
      :key="mdEditorKey"
      :model-value="content"
      :class="mdEditorClass"
      language="zh-CN"
      :theme="dark ? 'dark' : 'light'"
      :preview-theme="'default'"
      :preview="showPreview"
      :style="{ height: 'calc(100vh - 286px)' }"
      @update:model-value="$emit('update:content', $event)"
      @on-change="$emit('queueSave')"
      @on-upload-img="onUploadImages"
    />
    <editor-content v-else :editor="editor" class="rich-editor" />

    <footer class="editor-footer">
      <span>字数: {{ wordCount }}</span>
      <span>行数: {{ lineCount }}</span>
      <strong>{{ contentType === 'markdown' ? 'Markdown' : 'HTML' }}</strong>
    </footer>
  </div>
</template>

<script setup lang="ts">
import { EditorContent } from '@tiptap/vue-3'
import type { Editor } from '@tiptap/vue-3'
import { MdEditor } from 'md-editor-v3'
import {
  Bold,
  ChevronDown,
  Code2,
  Heading1,
  Heading2,
  Italic,
  List,
  ListChecks,
  Quote,
  Strikethrough,
  Upload,
} from 'lucide-vue-next'
import type { ContentType } from '../types'

const props = defineProps<{
  content: string
  contentType: ContentType
  editor?: Editor
  dark: boolean
  showPreview: boolean
  mdEditorKey: string
  mdEditorClass: string
  wordCount: number
  lineCount: number
}>()

const emit = defineEmits<{
  'update:content': [value: string]
  queueSave: []
  uploadImages: [files: File[], callback: (urls: string[]) => void]
}>()

function onUploadImages(files: File[], callback: (urls: string[]) => void) {
  emit('uploadImages', files, callback)
}

function runRichCommand(command: 'bold' | 'italic' | 'strike' | 'code' | 'quote' | 'h1' | 'h2' | 'bulletList' | 'orderedList') {
  if (!props.editor) return
  const chain = props.editor.chain().focus()
  if (command === 'bold') chain.toggleBold().run()
  if (command === 'italic') chain.toggleItalic().run()
  if (command === 'strike') chain.toggleStrike().run()
  if (command === 'code') chain.toggleCode().run()
  if (command === 'quote') chain.toggleBlockquote().run()
  if (command === 'h1') chain.toggleHeading({ level: 1 }).run()
  if (command === 'h2') chain.toggleHeading({ level: 2 }).run()
  if (command === 'bulletList') chain.toggleBulletList().run()
  if (command === 'orderedList') chain.toggleOrderedList().run()
}
</script>

<style scoped>
.editor-frame {
  height: calc(100vh - 180px);
  margin: 18px 32px 0;
  border: 1px solid #e5e7eb;
  border-radius: 16px;
  overflow: hidden;
  background: #fff;
  box-shadow: 0 12px 32px rgba(17, 24, 39, 0.04);
  animation: frameIn 0.26s ease both;
  transition: background 0.22s ease, border-color 0.22s ease, box-shadow 0.22s ease;
}

.format-toolbar {
  height: 44px;
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 0 12px;
  border-bottom: 1px solid #e5e7eb;
  background: #fff;
}

.format-toolbar button {
  width: 36px;
  height: 36px;
  display: grid;
  place-items: center;
  border: 0;
  border-radius: 10px;
  background: transparent;
  color: #4b5563;
  cursor: pointer;
  transition: background 0.18s ease, color 0.18s ease, transform 0.18s ease;
}

.format-toolbar button:hover {
  background: #f3f4f6;
  transform: translateY(-1px);
}

.format-toolbar .export-button {
  width: auto;
  display: flex;
  gap: 6px;
  margin-left: auto;
  padding: 0 8px;
}

@keyframes frameIn {
  from {
    opacity: 0;
    transform: translateY(8px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.vault-md-editor {
  height: calc(100vh - 258px) !important;
  border-radius: 0;
}

.rich-editor {
  height: calc(100vh - 258px) !important;
  min-height: calc(100vh - 286px);
  padding: 24px;
}

.editor-footer {
  height: 34px;
  display: flex;
  align-items: center;
  gap: 18px;
  padding: 0 14px;
  border-top: 1px solid #e5e7eb;
  color: #6b7280;
  font-size: 12px;
}

.editor-footer strong {
  margin-left: auto;
  font-weight: 500;
}

:deep(.md-editor-content) {
  background: #fff;
}

:deep(.md-editor-input-wrapper) {
  padding: 24px;
  border-right: 1px solid #e5e7eb;
}

.no-preview :deep(.md-editor-input-wrapper) {
  border-right: 0;
}

:deep(.md-editor-preview-wrapper) {
  padding: 32px;
}

:deep(.md-editor-preview h1) {
  margin-bottom: 16px;
  color: #111827;
  font-size: 28px;
  font-weight: 800;
}

:deep(.md-editor-preview) {
  color: #374151;
  font-size: 15px;
  line-height: 1.8;
}

:deep(.md-editor-preview pre) {
  padding: 16px;
  border-radius: 12px;
  background: #111827;
  color: #e5e7eb;
}

.editor-frame.is-dark {
  border-color: #303747 !important;
  background: #1a1f2b !important;
  box-shadow: 0 18px 46px rgba(0, 0, 0, 0.18) !important;
}

.editor-frame.is-dark .format-toolbar,
.editor-frame.is-dark .editor-footer {
  border-color: #303747 !important;
  background: #1a1f2b !important;
}

.editor-frame.is-dark .format-toolbar button {
  color: #cbd5e1 !important;
}

.editor-frame.is-dark .format-toolbar button:hover {
  background: #242b38 !important;
}

.editor-frame.is-dark .rich-editor,
.editor-frame.is-dark :deep(.md-editor),
.editor-frame.is-dark :deep(.md-editor-content),
.editor-frame.is-dark :deep(.md-editor-input),
.editor-frame.is-dark :deep(.md-editor-input-wrapper),
.editor-frame.is-dark :deep(.md-editor-preview),
.editor-frame.is-dark :deep(.md-editor-preview-wrapper) {
  background: #1a1f2b !important;
}

.editor-frame.is-dark :deep(.md-editor-input-wrapper) {
  border-right-color: #303747 !important;
}

.editor-frame.is-dark :deep(.md-editor-input textarea),
.editor-frame.is-dark :deep(.md-editor-preview h1),
.editor-frame.is-dark :deep(.md-editor-preview h2),
.editor-frame.is-dark :deep(.md-editor-preview h3),
.editor-frame.is-dark :deep(.md-editor-preview p),
.editor-frame.is-dark :deep(.md-editor-preview li) {
  color: #f8fafc !important;
}

.editor-frame.is-dark :deep(.md-editor-preview) {
  color: #cbd5e1 !important;
}

.editor-frame.is-dark :deep(.md-editor-preview pre) {
  background: #0b1020 !important;
  color: #e5e7eb !important;
}
</style>
