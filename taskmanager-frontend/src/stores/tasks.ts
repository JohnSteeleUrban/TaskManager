import { ref } from 'vue'
import { defineStore } from 'pinia'
import {
  getTasks,
  createTask,
  updateTask,
  deleteTask,
  type TaskQueryParams
} from '../api/taskService'
import type { TaskResponse, CreateTaskRequest, UpdateTaskRequest } from '../types'

export const useTaskStore = defineStore('tasks', () => {
  const tasks = ref<TaskResponse[]>([])
  const totalCount = ref(0)
  const totalPages = ref(0)
  const loading = ref(false)

  // filter/pagination state with defaults
  const filters = ref<TaskQueryParams>({
    pageNumber: 1,
    pageSize: 10,
    sortBy: 'createdAt',
    sortDirection: 'desc'
  })

  async function fetchTasks() {
    loading.value = true
    try {
      const result = await getTasks(filters.value)
      tasks.value = result.items
      totalCount.value = result.totalCount
      totalPages.value = result.totalPages
    } finally {
      loading.value = false
    }
  }

  async function addTask(request: CreateTaskRequest) {
    await createTask(request)
    await fetchTasks()
  }

  async function editTask(id: string, request: UpdateTaskRequest) {
    await updateTask(id, request)
    await fetchTasks()
  }

  async function removeTask(id: string) {
    await deleteTask(id)
    // if we deleted the last item on this page, go back one page
    if (tasks.value.length === 1 && filters.value.pageNumber! > 1) {
      filters.value.pageNumber!--
    }
    await fetchTasks()
  }

  function setFilter(key: keyof TaskQueryParams, value: number | string | undefined) {
    filters.value[key] = value as any
    filters.value.pageNumber = 1
    fetchTasks()
  }

  function setPage(page: number) {
    filters.value.pageNumber = page
    fetchTasks()
  }

  function setSorting(field: string, direction: string) {
    filters.value.sortBy = field
    filters.value.sortDirection = direction
    filters.value.pageNumber = 1
    fetchTasks()
  }

  return {
    tasks,
    totalCount,
    totalPages,
    loading,
    filters,
    fetchTasks,
    addTask,
    editTask,
    removeTask,
    setFilter,
    setPage,
    setSorting
  }
})
