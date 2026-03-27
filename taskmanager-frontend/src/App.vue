<script setup lang="ts">
import { useRoute } from 'vue-router'
import AppLayout from './components/AppLayout.vue'
import Toast from 'primevue/toast'

const route = useRoute()

const publicRoutes = ['login', 'register']
</script>

<template>
  <Toast />

  <AppLayout v-if="!publicRoutes.includes(route.name as string)">
    <router-view />
  </AppLayout>

  <router-view v-else />
</template>
