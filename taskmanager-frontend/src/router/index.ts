import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: () => import('../views/LoginView.vue'),
      meta: { requiresAuth: false }
    },
    {
      path: '/register',
      name: 'register',
      component: () => import('../views/RegisterView.vue'),
      meta: { requiresAuth: false }
    },
    {
      path: '/',
      name: 'tasks',
      component: () => import('../views/TaskListView.vue'),
      meta: { requiresAuth: true }
    }
  ]
})

// route guard: kick unauthenticated users to login
router.beforeEach((to) => {
  const token = localStorage.getItem('token')

  if (to.meta.requiresAuth && !token) {
    return { name: 'login' }
  }

  // already logged in, don't show login/register pages
  if (!to.meta.requiresAuth && token && to.name !== 'tasks') {
    return { name: 'tasks' }
  }
})

export default router
