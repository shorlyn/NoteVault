<template>
  <div class="editor-frame" :class="{ 'is-dark': dark, 'is-reading': !editing }">
    <div v-if="contentType === 'html' && editing" class="format-toolbar">
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
      v-if="contentType === 'markdown' && editing"
      :key="mdEditorKey"
      :model-value="normalizedMarkdown"
      :class="mdEditorClass"
      language="zh-CN"
      :theme="dark ? 'dark' : 'light'"
      :preview-theme="'default'"
      :preview="showPreview"
      :style="{ height: 'calc(100vh - 286px)' }"
      @update:model-value="onMarkdownUpdate"
      @on-change="onMarkdownChange"
      @on-upload-img="onUploadImages"
    />
    <div v-else-if="contentType === 'markdown'" class="markdown-reader">
      <MdPreview
        :id="`note-preview-${mdEditorKey}`"
        :model-value="normalizedMarkdown"
        :theme="dark ? 'dark' : 'light'"
        :preview-theme="'default'"
      />
    </div>
    <editor-content v-else :editor="editor" class="rich-editor" />

    <footer class="editor-footer">
      <span>字数: {{ wordCount }}</span>
      <span>行数: {{ lineCount }}</span>
      <strong>{{ contentType === 'markdown' ? 'Markdown' : 'HTML' }}</strong>
    </footer>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { EditorContent } from '@tiptap/vue-3'
import type { Editor } from '@tiptap/vue-3'
import { MdEditor, MdPreview } from 'md-editor-v3'
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
  editing: boolean
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

const normalizedMarkdown = computed(() => isEditorPlaceholderContent(props.content) ? '' : props.content)

function onUploadImages(files: File[], callback: (urls: string[]) => void) {
  emit('uploadImages', files, callback)
}

function onMarkdownUpdate(value: string) {
  if (!props.editing) return
  emit('update:content', value)
}

function onMarkdownChange() {
  if (!props.editing) return
  emit('queueSave')
}

function isEditorPlaceholderContent(value: string) {
  return ['', '<p></p>', '<p><br></p>', '<p><br /></p>'].includes(value.trim())
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

.editor-frame.is-reading .rich-editor {
  height: calc(100vh - 214px) !important;
  min-height: calc(100vh - 214px);
}

.markdown-reader {
  height: calc(100vh - 214px);
  overflow: auto;
  padding: 34px 44px;
}

.markdown-reader :deep(.md-editor-previewOnly) {
  background: transparent;
}

.markdown-reader :deep(.md-editor-preview) {
  color: #374151;
  font-size: 15px;
  line-height: 1.78;
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
  padding: 34px 42px;
}

:deep(.md-editor-preview h1) {
  margin: 0 0 18px;
  color: #111827;
  font-size: 24px;
  line-height: 1.32;
  font-weight: 800;
}

:deep(.md-editor-preview) {
  color: #374151;
  font-size: 15px;
  line-height: 1.78;
  letter-spacing: 0;
}

:deep(.md-editor-preview h2) {
  margin: 28px 0 14px;
  color: #111827;
  font-size: 20px;
  line-height: 1.35;
  font-weight: 750;
}

:deep(.md-editor-preview h3) {
  margin: 22px 0 10px;
  color: #1f2937;
  font-size: 17px;
  line-height: 1.4;
  font-weight: 750;
}

:deep(.md-editor-preview h4),
:deep(.md-editor-preview h5),
:deep(.md-editor-preview h6) {
  margin: 18px 0 8px;
  color: #374151;
  font-size: 15px;
  line-height: 1.45;
  font-weight: 700;
}

:deep(.md-editor-preview p) {
  margin: 0 0 13px;
}

:deep(.md-editor-preview strong) {
  color: #111827;
  font-weight: 750;
}

:deep(.md-editor-preview em) {
  color: #4b5563;
}

:deep(.md-editor-preview a) {
  color: var(--nv-primary, #635bff);
  text-decoration: none;
  border-bottom: 1px solid color-mix(in srgb, var(--nv-primary, #635bff) 35%, transparent);
}

:deep(.md-editor-preview ol),
:deep(.md-editor-preview ul) {
  margin: 10px 0 16px 0;
  padding-left: 1.35em;
}

:deep(.md-editor-preview ol) {
  list-style: decimal;
}

:deep(.md-editor-preview ul) {
  list-style: disc;
}

:deep(.md-editor-preview li) {
  display: list-item;
  margin: 6px 0;
  padding-left: 0.15em;
}

:deep(.md-editor-preview li::marker) {
  color: #8b92a1;
  font-size: 0.95em;
}

:deep(.md-editor-preview blockquote) {
  margin: 18px 0;
  padding: 12px 16px;
  border-left: 3px solid color-mix(in srgb, var(--nv-primary, #635bff) 42%, #9ca3af);
  border-radius: 0 8px 8px 0;
  background: #f7f8fc;
  color: #4b5563;
}

:deep(.md-editor-preview blockquote p) {
  margin: 0;
}

:deep(.md-editor-preview pre) {
  margin: 16px 0;
  padding: 14px 16px;
  border-radius: 10px;
  background: #111827;
  color: #e5e7eb;
}

:deep(.md-editor-preview code:not(pre code)) {
  padding: 2px 6px;
  border-radius: 6px;
  background: #f1f3f7;
  color: #374151;
  font-size: 0.92em;
}

:deep(.md-editor-preview hr) {
  margin: 24px 0;
  border: 0;
  border-top: 1px solid #e5e7eb;
}

:deep(.md-editor-preview table) {
  width: 100%;
  margin: 16px 0;
  border-collapse: collapse;
  font-size: 14px;
}

:deep(.md-editor-preview th),
:deep(.md-editor-preview td) {
  padding: 9px 10px;
  border: 1px solid #e5e7eb;
}

:deep(.md-editor-preview th) {
  background: #f7f8fc;
  color: #111827;
  font-weight: 700;
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
.editor-frame.is-dark .markdown-reader,
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

.editor-frame.is-dark :deep(.md-editor-preview strong),
.editor-frame.is-dark :deep(.md-editor-preview h1),
.editor-frame.is-dark :deep(.md-editor-preview h2),
.editor-frame.is-dark :deep(.md-editor-preview h3) {
  color: #f8fafc !important;
}

.editor-frame.is-dark :deep(.md-editor-preview h4),
.editor-frame.is-dark :deep(.md-editor-preview h5),
.editor-frame.is-dark :deep(.md-editor-preview h6),
.editor-frame.is-dark :deep(.md-editor-preview em) {
  color: #cbd5e1 !important;
}

.editor-frame.is-dark :deep(.md-editor-preview blockquote) {
  border-left-color: color-mix(in srgb, var(--nv-primary, #635bff) 48%, #94a3b8) !important;
  background: #242b38 !important;
  color: #cbd5e1 !important;
}

.editor-frame.is-dark :deep(.md-editor-preview code:not(pre code)) {
  background: #242b38 !important;
  color: #e5e7eb !important;
}

.editor-frame.is-dark :deep(.md-editor-preview hr),
.editor-frame.is-dark :deep(.md-editor-preview th),
.editor-frame.is-dark :deep(.md-editor-preview td) {
  border-color: #303747 !important;
}

.editor-frame.is-dark :deep(.md-editor-preview th) {
  background: #242b38 !important;
  color: #f8fafc !important;
}
</style>
