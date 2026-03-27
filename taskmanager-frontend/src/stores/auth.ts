import { ref, computed } from 'vue'
import { defineStore } from 'pinia'
import { login as apiLogin, register as apiRegister } from '../api/authService'
import type { UserDto, LoginRequest, RegisterRequest } from '../types'
import router from '../router'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem('token'))
  const user = ref<UserDto | null>(loadUser())

  const isAuthenticated = computed(() => !!token.value)
  const fullName = computed(() =>
    user.value ? `${user.value.firstName} ${user.value.lastName}` : ''
  )

  function loadUser(): UserDto | null {
    const stored = localStorage.getItem('user')
    if (!stored) return null
    try {
      return JSON.parse(stored) as UserDto
    } catch {
      return null
    }
  }

  function setAuth(newToken: string, newUser: UserDto) {
    token.value = newToken
    user.value = newUser
    localStorage.setItem('token', newToken)
    localStorage.setItem('user', JSON.stringify(newUser))
  }

  function clearAuth() {
    token.value = null
    user.value = null
    localStorage.removeItem('token')
    localStorage.removeItem('user')
  }

  async function login(request: LoginRequest) {
    const response = await apiLogin(request)
    setAuth(response.token, response.user)
    router.push('/')
  }

  async function register(request: RegisterRequest) {
    const response = await apiRegister(request)
    setAuth(response.token, response.user)
    router.push('/')
  }

  function logout() {
    clearAuth()
    router.push('/login')
  }

  return { token, user, isAuthenticated, fullName, login, register, logout }
})
