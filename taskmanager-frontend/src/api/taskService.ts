import apiClient from './client'
import type {
  TaskResponse,
  CreateTaskRequest,
  UpdateTaskRequest,
  PagedResponse
} from '../types'

export interface TaskQueryParams {
  status?: number
  priority?: number
  sortBy?: string
  sortDirection?: string
  pageNumber?: number
  pageSize?: number
}

export async function getTasks(params: TaskQueryParams = {}): Promise<PagedResponse<TaskResponse>> {
  const response = await apiClient.get<PagedResponse<TaskResponse>>('/tasks', { params })
  return response.data
}

export async function getTask(id: string): Promise<TaskResponse> {
  const response = await apiClient.get<TaskResponse>(`/tasks/${id}`)
  return response.data
}

export async function createTask(request: CreateTaskRequest): Promise<TaskResponse> {
  const response = await apiClient.post<TaskResponse>('/tasks', request)
  return response.data
}

export async function updateTask(id: string, request: UpdateTaskRequest): Promise<TaskResponse> {
  const response = await apiClient.put<TaskResponse>(`/tasks/${id}`, request)
  return response.data
}

export async function deleteTask(id: string): Promise<void> {
  await apiClient.delete(`/tasks/${id}`)
}
