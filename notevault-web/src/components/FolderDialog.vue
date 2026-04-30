<template>
  <div class="modal-layer" @mousedown.self="$emit('cancel')">
    <section class="folder-modal" @keydown.stop>
      <button class="folder-modal-close" type="button" @click="$emit('cancel')">
        <X :size="30" />
      </button>
      <header class="folder-modal-header">
        <div class="folder-modal-icon">
          <FolderPlus :size="32" />
        </div>
        <div>
          <h2>新建文件夹</h2>
          <p>为你的笔记创建一个新文件夹</p>
        </div>
      </header>
      <label class="folder-modal-label" for="folder-name-input">文件夹名称</label>
      <div class="folder-input-shell" @click="folderNameInput?.focus()">
        <Files :size="28" />
        <input
          id="folder-name-input"
          ref="folderNameInput"
          :value="name"
          maxlength="50"
          placeholder="输入文件夹名称"
          @input="onInput"
          @keydown.enter="$emit('create')"
        />
        <span>{{ name.length }} / 50</span>
      </div>
      <footer class="folder-modal-actions">
        <button class="folder-cancel" type="button" @click="$emit('cancel')">取消</button>
        <button class="folder-confirm" type="button" :disabled="!name.trim()" @click="$emit('create')">创建</button>
      </footer>
    </section>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { Files, FolderPlus, X } from 'lucide-vue-next'

defineProps<{
  name: string
}>()

const emit = defineEmits<{
  'update:name': [value: string]
  cancel: []
  create: []
}>()

const folderNameInput = ref<HTMLInputElement | null>(null)

onMounted(() => {
  window.setTimeout(() => folderNameInput.value?.focus(), 80)
})

function onInput(event: Event) {
  emit('update:name', (event.target as HTMLInputElement).value)
}
</script>

<style scoped>
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

.folder-modal {
  position: relative;
  width: min(620px, calc(100vw - 32px));
  overflow: hidden;
  border-radius: 22px;
  background: rgba(255, 255, 255, 0.98);
  box-shadow: 0 24px 60px rgba(17, 24, 39, 0.18);
  color: #111827;
  font-family: Inter, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
}

.folder-modal-close {
  position: absolute;
  top: 22px;
  right: 22px;
  width: 38px;
  height: 38px;
  display: grid;
  place-items: center;
  border: 0;
  border-radius: 12px;
  background: transparent;
  color: #6b7280;
  cursor: pointer;
}

.folder-modal-close:hover {
  background: #f3f4f6;
  color: #111827;
}

.folder-modal-header {
  display: flex;
  align-items: center;
  gap: 22px;
  padding: 42px 48px 28px;
}

.folder-modal-icon {
  width: 74px;
  height: 74px;
  display: grid;
  place-items: center;
  border-radius: 999px;
  background: var(--nv-primary-soft, #eef0ff);
  color: #fff;
}

.folder-modal-icon svg {
  width: 48px;
  height: 48px;
  padding: 10px;
  border-radius: 12px;
  background: linear-gradient(135deg, var(--nv-primary, #635bff), var(--nv-primary-2, #7c3aed));
  box-shadow: 0 12px 26px var(--nv-primary-glow, rgba(99, 91, 255, 0.24));
}

.folder-modal h2 {
  margin: 0 0 8px;
  color: #111827;
  font-size: 28px;
  line-height: 1.2;
  font-weight: 800;
}

.folder-modal p {
  margin: 0;
  color: #6b7280;
  font-size: 16px;
}

.folder-modal-label {
  display: block;
  margin: 0 48px 12px;
  color: #111827;
  font-size: 17px;
  font-weight: 800;
}

.folder-input-shell {
  height: 58px;
  display: flex;
  align-items: center;
  gap: 14px;
  margin: 0 48px 42px;
  padding: 0 20px;
  border: 1px solid var(--nv-primary-border, #bdb7ff);
  border-radius: 14px;
  background: #fff;
  box-shadow:
    0 0 0 3px color-mix(in srgb, var(--nv-primary, #635bff) 10%, transparent),
    0 14px 32px color-mix(in srgb, var(--nv-primary, #635bff) 12%, transparent);
}

.folder-input-shell svg {
  color: #6b7280;
}

.folder-input-shell input {
  min-width: 0;
  flex: 1;
  border: 0;
  outline: 0;
  background: transparent;
  color: #111827;
  font: inherit;
  font-size: 18px;
  font-weight: 650;
}

.folder-input-shell input::placeholder {
  color: #9ca3af;
}

.folder-input-shell span {
  color: #9ca3af;
  font-size: 16px;
  font-weight: 700;
}

.folder-modal-actions {
  height: 92px;
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 14px;
  padding: 0 48px;
  border-top: 1px solid #e5e7eb;
  background: rgba(250, 251, 255, 0.72);
}

.folder-cancel,
.folder-confirm {
  height: 44px;
  min-width: 96px;
  padding: 0 24px;
  border-radius: 12px;
  cursor: pointer;
  font-size: 16px;
  font-weight: 800;
}

.folder-cancel {
  border: 1px solid #e5e7eb;
  color: #111827;
  background: #fff;
}

.folder-cancel:hover {
  background: #f3f4f6;
}

.folder-confirm {
  border: 0;
  color: #fff;
  background: linear-gradient(135deg, var(--nv-primary, #635bff), var(--nv-primary-2, #7c3aed));
  box-shadow: 0 14px 30px color-mix(in srgb, var(--nv-primary, #635bff) 22%, transparent);
}

.folder-confirm:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 18px 34px color-mix(in srgb, var(--nv-primary, #635bff) 30%, transparent);
}

.folder-confirm:disabled {
  cursor: not-allowed;
  opacity: 0.55;
}

:global(.workspace.is-dark) .folder-modal,
:global(.workspace.is-dark) .folder-input-shell {
  border-color: #2b303b;
  background: #1a1f2b;
}

:global(.workspace.is-dark) .folder-modal-actions {
  border-top-color: #2b303b;
  background: rgba(18, 22, 33, 0.72);
}

:global(.workspace.is-dark) .folder-modal h2,
:global(.workspace.is-dark) .folder-modal-label,
:global(.workspace.is-dark) .folder-input-shell input {
  color: #f8fafc;
}

:global(.workspace.is-dark) .folder-modal p {
  color: #cbd5e1;
}

:global(.workspace.is-dark) .folder-cancel {
  border-color: #2b303b;
  background: #242b38;
  color: #f8fafc;
}
</style>
