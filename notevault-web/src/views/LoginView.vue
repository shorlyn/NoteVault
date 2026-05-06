<template>
  <main class="login-page">
    <section class="intro-panel">
      <div class="logo-card">N</div>
      <h1>NoteVault</h1>
      <h2>Your notes, your keys, your vault.</h2>
      <p>一个支持 Markdown、富文本和单篇加密的私有云笔记系统，让你的知识安全、可控、随时可用。</p>

      <div class="feature-grid">
        <div class="feature-item">
          <PackageCheck :size="22" />
          <strong>私有部署</strong>
          <span>数据只属于你，完全掌控</span>
        </div>
        <div class="feature-item">
          <ShieldCheck :size="22" />
          <strong>单篇加密</strong>
          <span>前端加密，安全可靠</span>
        </div>
        <div class="feature-item">
          <FileText :size="22" />
          <strong>Markdown & 富文本</strong>
          <span>随心记录，灵活高效</span>
        </div>
      </div>

    </section>

    <section class="login-card">
      <h3>欢迎回来</h3>
      <p>登录你的 NoteVault</p>
      <form class="login-form" @submit.prevent="submit">
        <label>
          <User :size="19" />
          <span>账号</span>
          <input v-model="username" placeholder="请输入账号" autocomplete="username" />
        </label>
        <label>
          <LockKeyhole :size="19" />
          <span>密码</span>
          <input v-model="password" type="password" placeholder="请输入密码" autocomplete="current-password" />
        </label>
        <div class="form-row">
          <label class="remember">
            <input type="checkbox" />
            记住登录状态
          </label>
          <button type="button">忘记密码?</button>
        </div>
        <button class="submit-button" :disabled="loading" type="submit">{{ loading ? '登录中...' : '登录' }}</button>
      </form>
    </section>

    <a class="github-link" href="https://github.com/shorlyn/NoteVault.git" target="_blank" rel="noreferrer">
      <Github :size="22" />
      <span>
        <strong>GitHub</strong>
        github.com/shorlyn/NoteVault
      </span>
    </a>
  </main>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useMessage } from 'naive-ui'
import { FileText, Github, LockKeyhole, PackageCheck, ShieldCheck, User } from 'lucide-vue-next'
import { useAuthStore } from '../stores/auth'

const username = ref('')
const password = ref('')
const loading = ref(false)
const auth = useAuthStore()
const router = useRouter()
const message = useMessage()

async function submit() {
  if (!username.value.trim() || !password.value) {
    message.warning('请输入账号和密码')
    return
  }
  loading.value = true
  try {
    await auth.login(username.value.trim(), password.value)
    router.push('/')
  } catch {
    message.error('账号或密码不正确')
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.login-page {
  min-height: 100vh;
  position: relative;
  display: grid;
  grid-template-columns: minmax(420px, 1fr) 520px;
  align-items: center;
  gap: 72px;
  padding: 56px 12vw;
  overflow: hidden;
  background:
    radial-gradient(circle at 78% 15%, rgba(106, 83, 242, 0.12), transparent 30%),
    radial-gradient(circle at 24% 35%, rgba(106, 83, 242, 0.10), transparent 38%),
    linear-gradient(135deg, #f7f8ff 0%, #ffffff 58%, #f7f7ff 100%);
}

.login-page::before {
  content: "";
  position: absolute;
  inset: -20%;
  background:
    repeating-radial-gradient(circle at 78% 23%, rgba(107, 93, 245, 0.16) 0 1px, transparent 1px 18px),
    radial-gradient(circle at 50% 0, transparent 0 270px, rgba(99, 86, 242, 0.10) 271px 286px, transparent 287px);
  opacity: 0.75;
  pointer-events: none;
}

.intro-panel,
.login-card {
  position: relative;
  z-index: 1;
}

.intro-panel {
  max-width: 580px;
}

.logo-card {
  width: 70px;
  height: 70px;
  display: grid;
  place-items: center;
  border-radius: 13px;
  color: #fff;
  background: linear-gradient(135deg, #6866f0, #513ee7);
  box-shadow: 0 18px 38px rgba(84, 72, 232, 0.28);
  font-size: 36px;
  font-weight: 900;
}

.intro-panel h1 {
  margin: 24px 0 8px;
  font-size: 42px;
  line-height: 1.1;
  color: #121827;
}

.intro-panel h2 {
  margin: 0 0 16px;
  color: #5147f0;
  font-size: 18px;
}

.intro-panel p {
  max-width: 560px;
  color: #5f6b7c;
  line-height: 1.9;
}

.feature-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 42px;
  margin: 34px 0 44px;
}

.feature-item {
  display: grid;
  gap: 9px;
  color: #1f2937;
}

.feature-item svg {
  width: 48px;
  height: 48px;
  padding: 12px;
  border: 1px solid rgba(91, 88, 244, 0.24);
  border-radius: 11px;
  color: #5147f0;
  background: rgba(255, 255, 255, 0.58);
}

.feature-item span {
  color: #7b8494;
  font-size: 13px;
}

.github-link {
  display: inline-flex;
  align-items: center;
  gap: 12px;
  color: #3f46d5;
  text-decoration: none;
}

.github-link svg {
  color: #111827;
}

.github-link span {
  display: grid;
  gap: 2px;
  font-size: 13px;
}

.github-link strong {
  font-size: 15px;
}

.login-card {
  width: 520px;
  padding: 50px 42px 38px;
  border: 1px solid rgba(196, 202, 218, 0.72);
  border-radius: 22px;
  background: rgba(255, 255, 255, 0.82);
  box-shadow: 0 26px 70px rgba(25, 32, 56, 0.12);
  backdrop-filter: blur(14px);
}

.login-card h3 {
  margin: 0;
  text-align: center;
  color: #111827;
  font-size: 30px;
}

.login-card > p {
  margin: 10px 0 32px;
  text-align: center;
  color: #697386;
  font-size: 16px;
}

.login-form {
  display: grid;
  gap: 16px;
}

.login-form label:not(.remember) {
  height: 76px;
  display: grid;
  grid-template-columns: 24px 1fr;
  grid-template-rows: 22px 1fr;
  column-gap: 13px;
  align-items: center;
  padding: 12px 18px;
  border: 1px solid #dfe4ee;
  border-radius: 9px;
  background: #fff;
}

.login-form label svg {
  grid-row: 1 / 3;
  color: #536073;
}

.login-form label span {
  color: #7b8494;
  font-size: 13px;
}

.login-form input {
  width: 100%;
  border: 0;
  outline: 0;
  background: transparent;
  color: #121827;
  font-size: 16px;
}

.form-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  color: #5f6b7c;
  font-size: 14px;
}

.remember {
  display: inline-flex;
  align-items: center;
  gap: 8px;
}

.remember input {
  width: 16px;
  height: 16px;
}

.form-row button {
  border: 0;
  background: transparent;
  color: #5147f0;
  cursor: pointer;
  font-weight: 650;
}

.submit-button {
  height: 56px;
  border: 0;
  border-radius: 7px;
  color: #fff;
  background: linear-gradient(135deg, #756df4, #4238e7);
  box-shadow: 0 16px 30px rgba(80, 68, 235, 0.25);
  cursor: pointer;
  font-size: 16px;
  font-weight: 760;
}

@media (max-width: 980px) {
  .login-page {
    grid-template-columns: 1fr;
    gap: 32px;
    padding: 72px 24px 32px;
  }

  .login-card {
    width: 100%;
  }
}

/* UI spec overrides */
.login-page {
  min-height: 100vh;
  display: grid;
  grid-template-columns: minmax(0, 1fr) 420px;
  gap: 82px;
  align-items: center;
  max-width: 1120px;
  margin: 0 auto;
  padding: 48px 40px;
  background:
    radial-gradient(circle at 15% 20%, rgba(99, 91, 255, 0.18), transparent 32%),
    radial-gradient(circle at 85% 80%, rgba(139, 92, 246, 0.16), transparent 30%),
    #f7f8fc;
  font-family: Inter, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
}

.login-page::before {
  display: none;
}

.intro-panel {
  max-width: 560px;
}

.logo-card {
  width: 52px;
  height: 52px;
  border-radius: 13px;
  background: linear-gradient(135deg, #635bff, #8b5cf6);
  box-shadow: 0 12px 32px rgba(17, 24, 39, 0.08);
  font-size: 28px;
}

.intro-panel h1 {
  margin: 22px 0 10px;
  color: #111827;
  font-size: 42px;
  font-weight: 800;
}

.intro-panel h2 {
  color: #4f46e5;
  font-size: 20px;
  font-weight: 700;
}

.intro-panel p {
  max-width: 520px;
  color: #6b7280;
  font-size: 15px;
  line-height: 1.8;
}

.feature-grid {
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 16px;
  margin: 32px 0 42px;
}

.feature-item {
  min-height: 112px;
  padding: 16px 14px;
  border: 1px solid rgba(229, 231, 235, 0.72);
  border-radius: 14px;
  background: #fff;
  box-shadow: 0 10px 28px rgba(17, 24, 39, 0.045);
}

.feature-item svg {
  width: 36px;
  height: 36px;
  padding: 8px;
  border: 0;
  border-radius: 14px;
  color: #635bff;
  background: #eef0ff;
}

.feature-item strong {
  color: #111827;
  font-size: 14px;
}

.feature-item span {
  color: #6b7280;
}

.github-link {
  position: fixed;
  left: 48px;
  bottom: 42px;
  z-index: 2;
  color: #111827;
  transition: color 0.18s ease;
}

.github-link:hover {
  color: #635bff;
}

.login-card {
  width: 420px;
  padding: 42px 38px 36px;
  border: 1px solid rgba(255, 255, 255, 0.7);
  border-radius: 20px;
  background: rgba(255, 255, 255, 0.92);
  box-shadow: 0 18px 44px rgba(17, 24, 39, 0.09);
  backdrop-filter: blur(14px);
}

.login-card h3 {
  font-size: 26px;
  font-weight: 800;
}

.login-card > p {
  margin: 10px 0 36px;
  color: #6b7280;
  font-size: 14px;
}

.login-form {
  gap: 18px;
}

.login-form label:not(.remember) {
  height: 52px;
  grid-template-columns: 22px 1fr;
  grid-template-rows: 1fr;
  padding: 0 14px;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  transition: border-color 0.18s ease, box-shadow 0.18s ease;
}

.login-form label:not(.remember):focus-within {
  border-color: #635bff;
  box-shadow: 0 0 0 3px rgba(99, 91, 255, 0.10);
}

.login-form label:not(.remember) span {
  display: none;
}

.login-form label svg {
  grid-row: auto;
}

.login-form input {
  color: #111827;
  font-size: 14px;
}

.form-row {
  margin: 0 0 2px;
}

.submit-button {
  height: 52px;
  border-radius: 12px;
  background: linear-gradient(135deg, #635bff, #7c3aed);
  font-weight: 700;
  transition: transform 0.18s ease, box-shadow 0.18s ease;
}

.submit-button:hover {
  transform: translateY(-1px);
  box-shadow: 0 12px 32px rgba(99, 91, 255, 0.28);
}

@media (max-width: 900px) {
  .login-page {
    grid-template-columns: 1fr;
    justify-items: center;
    padding: 32px 20px;
  }

  .intro-panel {
    display: none;
  }

  .github-link {
    position: relative;
    left: auto;
    bottom: auto;
  }
}

@media (max-width: 640px) {
  .login-card {
    width: 100%;
    padding: 28px;
  }
}
</style>
