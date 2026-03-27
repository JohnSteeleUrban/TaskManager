<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '../stores/auth'
import InputText from 'primevue/inputtext'
import Password from 'primevue/password'
import Button from 'primevue/button'
import Message from 'primevue/message'
import { AxiosError } from 'axios'

const authStore = useAuthStore()

const email = ref('')
const password = ref('')
const firstName = ref('')
const lastName = ref('')
const error = ref('')
const fieldErrors = ref<Record<string, string[]>>({})
const loading = ref(false)

async function handleRegister() {
  error.value = ''
  fieldErrors.value = {}
  loading.value = true

  try {
    await authStore.register({
      email: email.value,
      password: password.value,
      firstName: firstName.value,
      lastName: lastName.value
    })
  } catch (err) {
    if (err instanceof AxiosError && err.response?.data) {
      const data = err.response.data
      if (data.errors) {
        fieldErrors.value = data.errors
      } else {
        error.value = data.error || 'Registration failed.'
      }
    } else {
      error.value = 'Something went wrong. Please try again.'
    }
  } finally {
    loading.value = false
  }
}

function getFieldError(field: string): string {
  const errors = fieldErrors.value[field]
  return errors?.length ? errors[0] : ''
}
</script>

<template>
  <div class="auth-container">
    <div class="auth-card">
      <h1>Create Account</h1>

      <Message v-if="error" severity="error" :closable="false">
        {{ error }}
      </Message>

      <form @submit.prevent="handleRegister" class="auth-form">
        <div class="field">
          <label for="firstName">First Name</label>
          <InputText
            id="firstName"
            v-model="firstName"
            placeholder="First name"
            class="w-full"
            :invalid="!!getFieldError('FirstName')"
          />
          <small v-if="getFieldError('FirstName')" class="p-error">
            {{ getFieldError('FirstName') }}
          </small>
        </div>

        <div class="field">
          <label for="lastName">Last Name</label>
          <InputText
            id="lastName"
            v-model="lastName"
            placeholder="Last name"
            class="w-full"
            :invalid="!!getFieldError('LastName')"
          />
          <small v-if="getFieldError('LastName')" class="p-error">
            {{ getFieldError('LastName') }}
          </small>
        </div>

        <div class="field">
          <label for="email">Email</label>
          <InputText
            id="email"
            v-model="email"
            type="email"
            placeholder="you@example.com"
            class="w-full"
            :invalid="!!getFieldError('Email')"
          />
          <small v-if="getFieldError('Email')" class="p-error">
            {{ getFieldError('Email') }}
          </small>
        </div>

        <div class="field">
          <label for="password">Password</label>
          <Password
            id="password"
            v-model="password"
            :feedback="false"
            toggleMask
            placeholder="Min. 8 characters"
            class="w-full"
            inputClass="w-full"
            :invalid="!!getFieldError('Password')"
          />
          <small v-if="getFieldError('Password')" class="p-error">
            {{ getFieldError('Password') }}
          </small>
        </div>

        <Button
          type="submit"
          label="Create Account"
          :loading="loading"
          class="w-full"
        />
      </form>

      <p class="auth-link">
        Already have an account?
        <router-link to="/login">Sign in</router-link>
      </p>
    </div>
  </div>
</template>

<style scoped>
.auth-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  padding: 2rem;
}

.auth-card {
  width: 100%;
  max-width: 400px;
}

.auth-card h1 {
  margin-bottom: 1.5rem;
  text-align: center;
}

.auth-form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.auth-link {
  text-align: center;
  margin-top: 1rem;
}
</style>
