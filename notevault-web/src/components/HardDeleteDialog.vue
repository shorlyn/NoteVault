<template>
  <div class="delete-dialog-layer" @mousedown.self="$emit('cancel')">
    <section class="delete-dialog" @keydown.stop>
      <button class="delete-dialog-close" type="button" @click="$emit('cancel')">
        <X :size="24" />
      </button>
      <header class="delete-dialog-header">
        <div class="delete-dialog-icon">
          <Trash2 :size="24" />
        </div>
        <div>
          <h2>彻底删除笔记</h2>
          <p>确定要彻底删除「{{ title }}」吗？此操作不能撤销。</p>
        </div>
      </header>
      <footer class="delete-dialog-actions">
        <button class="delete-dialog-cancel" type="button" @click="$emit('cancel')">取消</button>
        <button class="delete-dialog-confirm" type="button" @click="$emit('confirm')">彻底删除</button>
      </footer>
    </section>
  </div>
</template>

<script setup lang="ts">
import { Trash2, X } from 'lucide-vue-next'

defineProps<{
  title?: string
}>()

defineEmits<{
  cancel: []
  confirm: []
}>()
</script>

<style scoped>
.delete-dialog-layer {
  position: fixed;
  inset: 0;
  z-index: 3000;
  display: grid;
  place-items: center;
  padding: 24px;
  background: rgba(17, 24, 39, 0.42);
  backdrop-filter: blur(10px);
}

.delete-dialog {
  position: relative;
  width: min(460px, calc(100vw - 32px));
  overflow: hidden;
  border-radius: 18px;
  background: rgba(255, 255, 255, 0.98);
  box-shadow: 0 18px 48px rgba(17, 24, 39, 0.18);
  color: #111827;
  font-family: Inter, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
}

.delete-dialog-close {
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
}

.delete-dialog-close:hover {
  background: #f3f4f6;
  color: #111827;
}

.delete-dialog-header {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 28px 28px 22px;
}

.delete-dialog-icon {
  width: 52px;
  height: 52px;
  display: grid;
  place-items: center;
  border-radius: 999px;
  color: #fff;
  background: rgba(239, 68, 68, 0.12);
}

.delete-dialog-icon svg {
  width: 28px;
  height: 28px;
  padding: 6px;
  border-radius: 8px;
  background: linear-gradient(135deg, #ef4444, #dc2626);
  box-shadow: 0 8px 18px rgba(239, 68, 68, 0.24);
}

.delete-dialog h2 {
  margin: 0 0 6px;
  color: #111827;
  font-size: 24px;
  line-height: 1.2;
  font-weight: 800;
}

.delete-dialog p {
  margin: 0;
  color: #6b7280;
  font-size: 15px;
  line-height: 1.6;
}

.delete-dialog-actions {
  height: 86px;
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 12px;
  padding: 0 28px;
  border-top: 1px solid #e5e7eb;
  background: rgba(250, 251, 255, 0.72);
}

.delete-dialog-cancel,
.delete-dialog-confirm {
  height: 40px;
  min-width: 84px;
  padding: 0 20px;
  border-radius: 12px;
  cursor: pointer;
  font-size: 15px;
  font-weight: 700;
}

.delete-dialog-cancel {
  border: 1px solid #e5e7eb;
  color: #111827;
  background: #fff;
}

.delete-dialog-cancel:hover {
  background: #f3f4f6;
}

.delete-dialog-confirm {
  border: 0;
  color: #fff;
  background: linear-gradient(135deg, #ef4444, #dc2626);
  box-shadow: 0 14px 30px rgba(239, 68, 68, 0.22);
}

.delete-dialog-confirm:hover {
  transform: translateY(-1px);
  box-shadow: 0 18px 34px rgba(239, 68, 68, 0.30);
}

:global(.dark) .delete-dialog {
  border: 1px solid #2b303b;
  background: #1a1f2b;
  color: #e5e7eb;
}

:global(.dark) .delete-dialog h2 {
  color: #f8fafc;
}

:global(.dark) .delete-dialog p {
  color: #cbd5e1;
}

:global(.dark) .delete-dialog-actions {
  border-top-color: #2b303b;
  background: rgba(18, 22, 33, 0.72);
}

:global(.dark) .delete-dialog-cancel {
  border-color: #2b303b;
  background: #242b38;
  color: #f8fafc;
}
</style>
