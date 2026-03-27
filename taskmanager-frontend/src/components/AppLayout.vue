<script setup lang="ts">
import { useAuthStore } from '../stores/auth'
import Button from 'primevue/button'

const authStore = useAuthStore()
</script>

<template>
  <div class="app-layout">
    <header class="app-header">
      <div class="header-left">
        <h2>TaskManager</h2>
      </div>
      <div class="header-right">
        <span class="user-name">{{ authStore.fullName }}</span>
        <Button
          icon="pi pi-sign-out"
          severity="secondary"
          text
          rounded
          @click="authStore.logout()"
          aria-label="Logout"
        />
      </div>
    </header>

    <main class="app-content">
      <slot />
    </main>
  </div>
</template>

<style scoped>
.app-layout {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.app-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.75rem 1.5rem;
  border-bottom: 1px solid var(--p-surface-200);
  background: var(--p-surface-0);
}

.app-header h2 {
  margin: 0;
  font-size: 1.25rem;
}

.header-right {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.user-name {
  font-size: 0.9rem;
  color: var(--p-text-muted-color);
}

.app-content {
  flex: 1;
  padding: 1.5rem;
  max-width: 1200px;
  margin: 0 auto;
  width: 100%;
}
</style>
