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
    <div class="account-menu" @keydown.escape="accountOpen = false">
      <button
        class="avatar"
        type="button"
        :class="{ active: accountOpen }"
        :title="username"
        :aria-label="`当前用户：${username}`"
        @click="accountOpen = !accountOpen"
      >
        {{ userInitial }}
      </button>
      <div v-if="accountOpen" class="account-popover">
        <div class="account-user">
          <span class="mini-avatar">{{ userInitial }}</span>
          <div>
            <strong>{{ username }}</strong>
            <small>当前账号</small>
          </div>
        </div>
        <button type="button" @click="selectAccountAction('changePassword')">
          <KeyRound :size="16" />
          修改密码
        </button>
        <button class="danger" type="button" @click="selectAccountAction('logout')">
          <LogOut :size="16" />
          退出登录
        </button>
      </div>
    </div>
  </header>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { Check, KeyRound, Lock, LogOut, Maximize2, Redo2, Star, Trash2, Undo2 } from 'lucide-vue-next'

defineProps<{
  dark: boolean
  saveState: string
  deleted: boolean
  isPinned: boolean
  username: string
  userInitial: string
}>()

const emit = defineEmits<{
  save: []
  togglePin: []
  encrypt: []
  toggleFullscreen: []
  restore: []
  delete: []
  changePassword: []
  logout: []
}>()

const accountOpen = ref(false)

function selectAccountAction(action: 'changePassword' | 'logout') {
  accountOpen.value = false
  if (action === 'changePassword') emit('changePassword')
  else emit('logout')
}
</script>

<style scoped>
.editor-topbar {
  position: relative;
  z-index: 40;
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
  position: relative;
  z-index: 1;
  display: flex;
  align-items: center;
  gap: 7px;
  margin-right: 12px;
  color: #10b981;
  font-size: 13px;
  white-space: nowrap;
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

.account-menu {
  position: relative;
  z-index: 60;
  margin-left: 10px;
}

.avatar {
  width: 32px;
  height: 32px;
  display: grid;
  place-items: center;
  border: 0;
  border-radius: 999px;
  background: linear-gradient(135deg, var(--nv-primary, #635bff), var(--nv-primary-2, #7c3aed));
  color: #fff;
  box-shadow: 0 8px 18px var(--nv-primary-glow, rgba(99, 91, 255, 0.24));
  cursor: pointer;
  font-weight: 700;
  transition: transform 0.18s ease, background 0.22s ease, box-shadow 0.22s ease;
}

.avatar:hover,
.avatar.active {
  transform: translateY(-1px) scale(1.04);
}

.account-popover {
  position: absolute;
  top: calc(100% + 12px);
  right: -2px;
  z-index: 100;
  width: 220px;
  padding: 8px;
  border: 1px solid #e5e7eb;
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.96);
  box-shadow: 0 18px 42px rgba(17, 24, 39, 0.14);
  backdrop-filter: blur(16px);
  animation: accountIn 0.16s ease both;
}

.account-popover::before {
  content: "";
  position: absolute;
  top: -6px;
  right: 13px;
  width: 12px;
  height: 12px;
  transform: rotate(45deg);
  border-left: 1px solid #e5e7eb;
  border-top: 1px solid #e5e7eb;
  background: rgba(255, 255, 255, 0.96);
}

.account-user {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px 8px 10px;
  border-bottom: 1px solid #eef0f5;
  margin-bottom: 6px;
}

.mini-avatar {
  width: 28px;
  height: 28px;
  display: grid;
  place-items: center;
  border-radius: 999px;
  background: linear-gradient(135deg, var(--nv-primary, #635bff), var(--nv-primary-2, #7c3aed));
  color: #fff;
  font-size: 13px;
  font-weight: 800;
}

.account-user strong {
  display: block;
  color: #111827;
  font-size: 14px;
  line-height: 1.2;
}

.account-user small {
  display: block;
  margin-top: 3px;
  color: #9ca3af;
  font-size: 12px;
}

.account-popover button:not(.avatar) {
  width: 100%;
  height: 36px;
  display: flex;
  align-items: center;
  gap: 9px;
  padding: 0 10px;
  border: 0;
  border-radius: 10px;
  background: transparent;
  color: #374151;
  cursor: pointer;
  font-size: 14px;
  font-weight: 700;
  text-align: left;
  white-space: nowrap;
  transition: background 0.18s ease, color 0.18s ease;
}

.account-popover button:hover {
  background: #f3f4f6;
  color: var(--nv-primary, #4f46e5);
}

.account-popover button.danger:hover {
  background: #fee2e2;
  color: #ef4444;
}

@keyframes pulse {
  0%, 100% { opacity: 0.45; }
  50% { opacity: 1; }
}

@keyframes accountIn {
  from {
    opacity: 0;
    transform: translateY(-4px) scale(0.98);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
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

.editor-topbar.is-dark .account-popover {
  border-color: #303747;
  background: rgba(26, 31, 43, 0.96);
  box-shadow: 0 18px 42px rgba(0, 0, 0, 0.32);
}

.editor-topbar.is-dark .account-popover::before {
  border-color: #303747;
  background: rgba(26, 31, 43, 0.96);
}

.editor-topbar.is-dark .account-user {
  border-bottom-color: #303747;
}

.editor-topbar.is-dark .account-user strong,
.editor-topbar.is-dark .account-popover button {
  color: #e5e7eb;
}

.editor-topbar.is-dark .account-user small {
  color: #94a3b8;
}

.editor-topbar.is-dark .account-popover button:hover {
  background: #242b38;
  color: color-mix(in srgb, var(--nv-primary, #635bff) 48%, white);
}

.editor-topbar.is-dark .account-popover button.danger:hover {
  background: rgba(239, 68, 68, 0.16);
  color: #fca5a5;
}
</style>
