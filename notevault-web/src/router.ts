import { createRouter, createWebHistory } from 'vue-router'
import WorkspaceView from './views/WorkspaceView.vue'
import LoginView from './views/LoginView.vue'
import { useAuthStore } from './stores/auth'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/login', component: LoginView },
    { path: '/', component: WorkspaceView },
  ],
})

router.beforeEach((to) => {
  const auth = useAuthStore()
  if (to.path !== '/login' && !auth.accessToken) return '/login'
  if (to.path === '/login' && auth.accessToken) return '/'
})

export default router
