import axios from 'axios'
import router from '../router'

const apiClient = axios.create({
  baseURL: 'http://localhost:5225/api',
  headers: {
    'Content-Type': 'application/json'
  }
})

// attach jwt token to every request
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// if we get a 401 back, token is expired or invalid so kick to login
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token')
      localStorage.removeItem('user')
      router.push('/login')
    }
    return Promise.reject(error)
  }
)

export default apiClient
