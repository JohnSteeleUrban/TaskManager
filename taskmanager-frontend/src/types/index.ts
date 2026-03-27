export interface UserDto {
  id: string
  email: string
  firstName: string
  lastName: string
}

export interface AuthResponse {
  token: string
  user: UserDto
}

export interface LoginRequest {
  email: string
  password: string
}

export interface RegisterRequest {
  email: string
  password: string
  firstName: string
  lastName: string
}

export enum Priority {
  Low = 0,
  Medium = 1,
  High = 2,
  Critical = 3
}

export enum TaskItemStatus {
  Pending = 0,
  InProgress = 1,
  Completed = 2
}

export interface TaskResponse {
  id: string
  title: string
  description: string | null
  priority: Priority
  status: TaskItemStatus
  dueDate: string | null
  createdAt: string
  updatedAt: string
}

export interface CreateTaskRequest {
  title: string
  description?: string
  priority: Priority
  dueDate?: string
}

export interface UpdateTaskRequest {
  title: string
  description?: string
  priority: Priority
  status: TaskItemStatus
  dueDate?: string
}

export interface PagedResponse<T> {
  items: T[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
}

export interface ValidationErrors {
  errors: Record<string, string[]>
}

export interface ApiError {
  error: string
}
