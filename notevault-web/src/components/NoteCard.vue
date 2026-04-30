<template>
  <div>
    <div class="note-card-title">
      <Lock v-if="note.isEncrypted" :size="14" />
      <Star v-if="note.isPinned" class="pin-mark" :size="14" fill="currentColor" />
      <strong>{{ note.title }}</strong>
      <div class="note-card-actions">
        <button
          v-if="trashMode"
          type="button"
          class="note-action active"
          title="放回原处"
          @click.stop="$emit('restore')"
        >
          <Undo2 :size="14" />
        </button>
        <button
          v-else
          type="button"
          class="note-action"
          :class="{ active: note.isPinned }"
          :title="note.isPinned ? '取消收藏' : '收藏'"
          @click.stop="$emit('togglePin')"
        >
          <Star :size="14" :fill="note.isPinned ? 'currentColor' : 'none'" />
        </button>
        <button
          type="button"
          class="note-action danger"
          :title="trashMode ? '彻底删除' : '删除'"
          @click.stop="$emit('delete')"
        >
          <Trash2 :size="14" />
        </button>
      </div>
    </div>
    <p class="note-card-desc">
      {{ note.isEncrypted ? '这篇笔记已加密，输入密码后查看内容。' : `${note.contentType === 'markdown' ? 'Markdown' : '富文本'} 文档` }}
    </p>
    <div class="note-card-foot">
      <div class="note-tags">
        <span
          v-for="tag in note.tags.slice(0, 2)"
          :key="tag.id"
        >
          {{ tag.name }}
        </span>
      </div>
      <time>{{ formatDate(note.updatedAt) }}</time>
    </div>
  </div>
</template>

<script setup lang="ts">
import { Lock, Star, Trash2, Undo2 } from 'lucide-vue-next'
import type { NoteList } from '../types'

defineProps<{
  note: NoteList
  trashMode?: boolean
}>()

defineEmits<{
  togglePin: []
  delete: []
  restore: []
}>()

function formatDate(value?: string | null) {
  if (!value) return ''
  const date = parseApiDate(value)
  const now = new Date()
  if (date.toDateString() === now.toDateString()) return date.toLocaleTimeString('zh-CN', { hour: '2-digit', minute: '2-digit' })
  const yesterday = new Date(now)
  yesterday.setDate(now.getDate() - 1)
  if (date.toDateString() === yesterday.toDateString()) return '昨天'
  return date.toLocaleDateString('zh-CN', { month: 'short', day: 'numeric' })
}

function parseApiDate(value: string) {
  const hasTimezone = /(?:z|[+-]\d{2}:?\d{2})$/i.test(value)
  return new Date(hasTimezone ? value : `${value}Z`)
}
</script>

<style scoped>
.note-card-title {
  display: flex;
  align-items: center;
  gap: 6px;
  min-width: 0;
  color: #374151;
}

.note-card-title strong {
  min-width: 0;
  overflow: hidden;
  color: #111827;
  text-overflow: ellipsis;
  white-space: nowrap;
  font-size: 15px;
  font-weight: 700;
}

.pin-mark {
  color: #d7a000;
}

.note-card-actions {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  margin-left: auto;
  opacity: 0;
  transition: opacity 0.18s ease;
}

:global(.note-card:hover) .note-card-actions,
:global(.note-card.is-current) .note-card-actions {
  opacity: 1;
}

.note-action {
  width: 26px;
  height: 26px;
  display: grid;
  place-items: center;
  border: 0;
  border-radius: 8px;
  background: transparent;
  color: #6b7280;
  cursor: pointer;
}

.note-action:hover,
.note-action.active {
  background: var(--nv-primary-soft, #eef0ff);
  color: var(--nv-primary, #4f46e5);
}

.note-action.danger:hover {
  background: #fee2e2;
  color: #ef4444;
}

.note-card-desc {
  margin: 6px 0 8px;
  overflow: hidden;
  color: #6b7280;
  text-overflow: ellipsis;
  white-space: nowrap;
  font-size: 13px;
}

.note-card-foot {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  color: #9ca3af;
  font-size: 12px;
}

.note-tags {
  min-width: 0;
  display: flex;
  gap: 5px;
}

.note-tags span {
  height: 22px;
  max-width: 92px;
  display: inline-flex;
  align-items: center;
  padding: 0 8px;
  overflow: hidden;
  border-radius: 999px;
  text-overflow: ellipsis;
  white-space: nowrap;
  font-size: 12px;
  font-weight: 650;
  color: var(--nv-primary, #4f46e5) !important;
  background: var(--nv-primary-soft, #eef0ff) !important;
}

.note-card-foot time {
  flex: 0 0 auto;
  margin-left: auto;
}

:global(.workspace.is-dark) .note-card-title {
  color: #cbd5e1;
}

:global(.workspace.is-dark) .note-card-title strong {
  color: #f8fafc;
}

:global(.workspace.is-dark) .note-card-desc,
:global(.workspace.is-dark) .note-card-foot,
:global(.workspace.is-dark) .note-action {
  color: #cbd5e1;
}

:global(.workspace.is-dark) .note-action:hover,
:global(.workspace.is-dark) .note-action.active {
  background: color-mix(in srgb, var(--nv-primary, #635bff) 22%, transparent);
  color: color-mix(in srgb, var(--nv-primary, #635bff) 48%, white);
}

:global(.workspace.is-dark) .note-tags span {
  color: color-mix(in srgb, var(--nv-primary, #635bff) 48%, white) !important;
  background: color-mix(in srgb, var(--nv-primary, #635bff) 20%, transparent) !important;
}
</style>
