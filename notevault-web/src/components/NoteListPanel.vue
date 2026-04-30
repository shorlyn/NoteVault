<template>
  <section class="note-column" :class="{ 'is-dark': dark }">
    <div class="search-shell">
      <Search :size="17" />
      <input :value="query" placeholder="搜索笔记 (Ctrl + K)" @input="onSearchInput" @keydown.enter="$emit('loadNotes')" />
      <kbd>/</kbd>
    </div>

    <div class="list-title-row">
      <button type="button" class="list-title" @click="$emit('showAll')">
        {{ listTitle }}
        <ChevronDown :size="16" />
      </button>
      <button class="icon-button" type="button" title="筛选" @click="$emit('loadNotes')">
        <Filter :size="17" />
      </button>
    </div>

    <n-skeleton v-if="loading" text :repeat="8" />
    <n-empty v-else-if="!notes.length" description="还没有笔记" class="empty-state" />
    <n-scrollbar v-else class="notes-scroll">
      <div v-if="pinnedNotes.length" class="group-label">置顶</div>
      <article
        v-for="note in pinnedNotes"
        :key="note.id"
        class="note-card"
        :class="{ 'is-current': currentId === note.id }"
        @click="$emit('openNote', note.id)"
      >
        <NoteCard
          :note="note"
          :trash-mode="trashMode"
          @toggle-pin="$emit('togglePin', note)"
          @delete="$emit('deleteNote', note)"
          @restore="$emit('restoreNote', note)"
        />
      </article>

      <div class="group-label">{{ pinnedNotes.length ? '今天' : '笔记' }}</div>
      <article
        v-for="note in regularNotes"
        :key="note.id"
        class="note-card"
        :class="{ 'is-current': currentId === note.id }"
        @click="$emit('openNote', note.id)"
      >
        <NoteCard
          :note="note"
          :trash-mode="trashMode"
          @toggle-pin="$emit('togglePin', note)"
          @delete="$emit('deleteNote', note)"
          @restore="$emit('restoreNote', note)"
        />
      </article>
    </n-scrollbar>
  </section>
</template>

<script setup lang="ts">
import { ChevronDown, Filter, Search } from 'lucide-vue-next'
import NoteCard from './NoteCard.vue'
import type { NoteList } from '../types'

defineProps<{
  query: string
  loading: boolean
  listTitle: string
  currentId?: string
  trashMode: boolean
  notes: NoteList[]
  pinnedNotes: NoteList[]
  regularNotes: NoteList[]
  dark: boolean
}>()

const emit = defineEmits<{
  'update:query': [value: string]
  loadNotes: []
  showAll: []
  openNote: [id: string]
  togglePin: [note: NoteList]
  deleteNote: [note: NoteList]
  restoreNote: [note: NoteList]
}>()

function onSearchInput(event: Event) {
  emit('update:query', (event.target as HTMLInputElement).value)
}
</script>

<style scoped>
.note-column {
  display: flex;
  flex-direction: column;
  min-width: 0;
  padding: 16px 12px 0;
  border-right: 1px solid #e5e7eb;
  background: #fafbff;
  transition: background 0.22s ease, border-color 0.22s ease;
}

.search-shell {
  height: 40px;
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 0 12px;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  background: #fff;
  color: #9ca3af;
  transition: border 0.18s ease, box-shadow 0.18s ease;
}

.search-shell:focus-within {
  border-color: var(--nv-primary, #635bff);
  box-shadow: 0 0 0 3px color-mix(in srgb, var(--nv-primary, #635bff) 12%, transparent);
}

.search-shell input {
  min-width: 0;
  flex: 1;
  border: 0;
  outline: 0;
  background: transparent;
  color: #111827;
  font: inherit;
  font-size: 14px;
}

.search-shell input::placeholder {
  color: #9ca3af;
}

.search-shell kbd {
  min-width: 24px;
  height: 24px;
  display: grid;
  place-items: center;
  border-radius: 8px;
  background: #f3f4f6;
  color: #6b7280;
  font-size: 13px;
  font-family: inherit;
}

.list-title-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-top: 16px;
  margin-bottom: 14px;
}

.list-title {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  border: 0;
  background: transparent;
  color: #111827;
  cursor: pointer;
  font: inherit;
  font-size: 16px;
  font-weight: 700;
}

.icon-button {
  width: 32px;
  height: 32px;
  display: grid;
  place-items: center;
  border: 0;
  border-radius: 10px;
  background: transparent;
  color: #4b5563;
  cursor: pointer;
}

.icon-button:hover {
  background: #f3f4f6;
}

.notes-scroll {
  min-height: 0;
}

.group-label {
  margin: 2px 0 8px;
  color: #9ca3af;
  font-size: 12px;
}

.note-card {
  position: relative;
  margin-bottom: 10px;
  padding: 14px 16px;
  border: 1px solid transparent;
  border-radius: 14px;
  background: #fff;
  cursor: pointer;
  animation: noteFadeIn 0.26s ease both;
  transition: border 0.18s ease, box-shadow 0.18s ease, background 0.18s ease, transform 0.18s ease;
}

.note-card:hover {
  border-color: color-mix(in srgb, var(--nv-primary-border, #bdb7ff) 72%, white);
  box-shadow: 0 8px 24px rgba(17, 24, 39, 0.06);
  transform: translateY(-1px);
}

.note-card.is-current {
  border-color: var(--nv-primary-border, #bdb7ff);
  background: linear-gradient(135deg, color-mix(in srgb, var(--nv-primary-soft, #eef0ff) 82%, white), #fff);
}

.note-card.is-current::before {
  content: "";
  position: absolute;
  left: 0;
  top: 16px;
  width: 4px;
  height: calc(100% - 32px);
  border-radius: 999px;
  background: var(--nv-primary, #635bff);
}

@keyframes noteFadeIn {
  from {
    opacity: 0;
    transform: translateY(6px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.empty-state {
  margin-top: 120px;
  color: #9ca3af;
}

.note-column.is-dark {
  border-color: #2b303b !important;
  background: #121621 !important;
}

.note-column.is-dark .search-shell,
.note-column.is-dark .note-card {
  background: #1a1f2b !important;
}

.note-column.is-dark .search-shell {
  border-color: #3a4254 !important;
  box-shadow: none !important;
}

.note-column.is-dark .search-shell:focus-within {
  border-color: color-mix(in srgb, var(--nv-primary, #635bff) 62%, white) !important;
  box-shadow: 0 0 0 3px color-mix(in srgb, var(--nv-primary, #635bff) 18%, transparent) !important;
}

.note-column.is-dark .search-shell input,
.note-column.is-dark .list-title {
  color: #f8fafc !important;
}

.note-column.is-dark .search-shell kbd {
  background: #f8fafc !important;
  color: #4b5563 !important;
}

.note-column.is-dark .note-card {
  border-color: transparent !important;
  box-shadow: none !important;
}

.note-column.is-dark .note-card:hover {
  border-color: #3a4254 !important;
  background: #1d2330 !important;
}

.note-column.is-dark .note-card.is-current {
  border-color: color-mix(in srgb, var(--nv-primary, #635bff) 62%, white) !important;
  background: linear-gradient(135deg, color-mix(in srgb, var(--nv-primary, #635bff) 22%, transparent), #1a1f2b) !important;
}
</style>
