<template>
  <n-config-provider :theme="theme">
    <n-dialog-provider>
      <n-message-provider>
        <router-view />
      </n-message-provider>
    </n-dialog-provider>
  </n-config-provider>
</template>

<script setup lang="ts">
import { computed, watchEffect } from 'vue'
import { darkTheme } from 'naive-ui'
import { useUiStore } from './stores/ui'

const ui = useUiStore()
const theme = computed(() => (ui.dark ? darkTheme : null))

watchEffect(() => {
  document.documentElement.classList.toggle('dark', ui.dark)
})
</script>
