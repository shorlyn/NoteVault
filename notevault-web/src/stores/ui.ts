import { defineStore } from 'pinia'

export type AccentTheme = 'violet' | 'blue' | 'emerald' | 'rose'
export type DefaultContentType = 'markdown' | 'html'

const themes: AccentTheme[] = ['violet', 'blue', 'emerald', 'rose']

export const useUiStore = defineStore('ui', {
  state: () => ({
    dark: localStorage.getItem('nv_dark') === 'true',
    theme: (localStorage.getItem('nv_theme') as AccentTheme) || 'violet',
    defaultContentType: (localStorage.getItem('nv_default_content_type') as DefaultContentType) || 'markdown',
  }),
  actions: {
    setDark(dark: boolean) {
      this.dark = dark
      localStorage.setItem('nv_dark', String(dark))
    },
    toggleDark() {
      this.setDark(!this.dark)
    },
    cycleTheme() {
      const index = themes.indexOf(this.theme)
      this.theme = themes[(index + 1) % themes.length]
      localStorage.setItem('nv_theme', this.theme)
    },
    setTheme(theme: AccentTheme) {
      this.theme = theme
      localStorage.setItem('nv_theme', theme)
    },
    setDefaultContentType(type: DefaultContentType) {
      this.defaultContentType = type
      localStorage.setItem('nv_default_content_type', type)
    },
  },
})
