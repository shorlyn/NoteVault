<template>
  <header class="editor-topbar" :class="{ 'is-dark': dark }">
    <div class="save-state" :class="{ saving: saveState === '正在保存', failed: saveState === '保存失败' }">
      <Check v-if="saveState === '已保存'" :size="15" />
      <span v-else class="status-dot" />
      {{ saveState }}
    </div>
    <button class="topbar-button" type="button" title="保存" @click="$emit('save')"><Undo2 :size="17" /></button>
    <button class="topbar-button" type="button" title="重做"><Redo2 :size="17" /></button>
    <button
      v-if="!deleted"
      class="topbar-button"
      :class="{ active: isPinned }"
      type="button"
      :title="isPinned ? '取消收藏' : '收藏'"
      @click="$emit('togglePin')"
    >
      <Star :size="17" />
    </button>
    <button v-if="!deleted" class="topbar-button" type="button" title="加密" @click="$emit('encrypt')">
      <Lock :size="17" />
    </button>
    <button class="topbar-button" type="button" title="全屏" @click="$emit('toggleFullscreen')">
      <Maximize2 :size="17" />
    </button>
    <button v-if="deleted" class="topbar-button active" type="button" title="放回原处" @click="$emit('restore')">
      <Undo2 :size="17" />
    </button>
    <button class="topbar-button danger" type="button" :title="deleted ? '彻底删除' : '删除'" @click="$emit('delete')">
      <Trash2 :size="17" />
    </button>
    <div class="avatar" :title="username" :aria-label="`当前用户：${username}`">{{ userInitial }}</div>
  </header>
</template>

<script setup lang="ts">
import { Check, Lock, Maximize2, Redo2, Star, Trash2, Undo2 } from 'lucide-vue-next'

defineProps<{
  dark: boolean
  saveState: string
  deleted: boolean
  isPinned: boolean
  username: string
  userInitial: string
}>()

defineEmits<{
  save: []
  togglePin: []
  encrypt: []
  toggleFullscreen: []
  restore: []
  delete: []
}>()
</script>

<style scoped>
.editor-topbar {
  height: 56px;
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 2px;
  padding: 0 24px;
  border-bottom: 1px solid #e5e7eb;
  background: rgba(255, 255, 255, 0.86);
  backdrop-filter: blur(12px);
}

.save-state {
  display: flex;
  align-items: center;
  gap: 7px;
  margin-right: 12px;
  color: #10b981;
  font-size: 13px;
}

.save-state.saving {
  color: #6b7280;
}

.save-state.failed {
  color: #ef4444;
}

.save-state svg {
  color: #10b981;
}

.status-dot {
  width: 6px;
  height: 6px;
  border-radius: 999px;
  background: #2fbd83;
}

.save-state.saving .status-dot {
  animation: pulse 1s infinite;
}

.topbar-button {
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

.topbar-button:hover {
  background: #f3f4f6;
  transform: translateY(-1px);
}

.topbar-button.active {
  background: var(--nv-primary-soft, #eef0ff);
  color: var(--nv-primary, #4f46e5);
}

.topbar-button.danger:hover {
  background: #fee2e2;
  color: #ef4444;
}

.avatar {
  width: 32px;
  height: 32px;
  display: grid;
  place-items: center;
  margin-left: 10px;
  border-radius: 999px;
  background: linear-gradient(135deg, var(--nv-primary, #635bff), var(--nv-primary-2, #7c3aed));
  color: #fff;
  box-shadow: 0 8px 18px var(--nv-primary-glow, rgba(99, 91, 255, 0.24));
  font-weight: 700;
  transition: transform 0.18s ease, background 0.22s ease, box-shadow 0.22s ease;
}

.avatar:hover {
  transform: translateY(-1px) scale(1.04);
}

@keyframes pulse {
  0%, 100% { opacity: 0.45; }
  50% { opacity: 1; }
}

.editor-topbar.is-dark {
  border-bottom-color: #252b38 !important;
  background: #171a22 !important;
}

.editor-topbar.is-dark .topbar-button {
  color: #cbd5e1 !important;
}

.editor-topbar.is-dark .topbar-button:hover {
  background: #242b38 !important;
}

.editor-topbar.is-dark .topbar-button.active {
  border-color: color-mix(in srgb, var(--nv-primary, #635bff) 62%, white) !important;
  background: color-mix(in srgb, var(--nv-primary, #635bff) 22%, transparent) !important;
  color: color-mix(in srgb, var(--nv-primary, #635bff) 48%, white) !important;
}
</style>
