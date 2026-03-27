<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useTaskStore } from '../stores/tasks'
import { useToast } from 'primevue/usetoast'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import Tag from 'primevue/tag'
import Dialog from 'primevue/dialog'
import Select from 'primevue/select'
import ConfirmDialog from 'primevue/confirmdialog'
import { useConfirm } from 'primevue/useconfirm'
import TaskForm from '../components/TaskForm.vue'
import { Priority, TaskItemStatus, type TaskResponse } from '../types'
import { AxiosError } from 'axios'

const taskStore = useTaskStore()
const toast = useToast()
const confirm = useConfirm()

const showDialog = ref(false)
const editingTask = ref<TaskResponse | null>(null)
const formLoading = ref(false)
const formError = ref('')

// filter dropdown options, "All" is undefined which means don't filter
const statusFilterOptions = [
  { label: 'All Statuses', value: undefined },
  { label: 'Pending', value: TaskItemStatus.Pending },
  { label: 'In Progress', value: TaskItemStatus.InProgress },
  { label: 'Completed', value: TaskItemStatus.Completed }
]

const priorityFilterOptions = [
  { label: 'All Priorities', value: undefined },
  { label: 'Low', value: Priority.Low },
  { label: 'Medium', value: Priority.Medium },
  { label: 'High', value: Priority.High },
  { label: 'Critical', value: Priority.Critical }
]

onMounted(() => {
  taskStore.fetchTasks()
})

function openCreate() {
  editingTask.value = null
  formError.value = ''
  showDialog.value = true
}

function openEdit(task: TaskResponse) {
  editingTask.value = task
  formError.value = ''
  showDialog.value = true
}

async function handleSubmit(data: any) {
  formLoading.value = true
  formError.value = ''

  try {
    if (editingTask.value) {
      await taskStore.editTask(editingTask.value.id, data)
      toast.add({ severity: 'success', summary: 'Task updated', life: 3000 })
    } else {
      await taskStore.addTask(data)
      toast.add({ severity: 'success', summary: 'Task created', life: 3000 })
    }
    showDialog.value = false
  } catch (err) {
    if (err instanceof AxiosError && err.response?.data?.errors) {
      const errors = err.response.data.errors
      const messages = Object.values(errors).flat()
      formError.value = messages.join(' ')
    } else {
      formError.value = 'Something went wrong.'
    }
  } finally {
    formLoading.value = false
  }
}

function confirmDelete(task: TaskResponse) {
  confirm.require({
    message: `Delete "${task.title}"? This can't be undone.`,
    header: 'Delete Task',
    icon: 'pi pi-trash',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: async () => {
      try {
        await taskStore.removeTask(task.id)
        toast.add({ severity: 'success', summary: 'Task deleted', life: 3000 })
      } catch {
        toast.add({ severity: 'error', summary: 'Failed to delete task', life: 3000 })
      }
    }
  })
}

function getPriorityLabel(value: Priority): string {
  return ['Low', 'Medium', 'High', 'Critical'][value]
}

function getPrioritySeverity(value: Priority): string {
  return ['info', 'info', 'warn', 'danger'][value]
}

function getStatusLabel(value: TaskItemStatus): string {
  return ['Pending', 'In Progress', 'Completed'][value]
}

function getStatusSeverity(value: TaskItemStatus): string {
  return ['warn', 'info', 'success'][value]
}

function formatDate(dateStr: string | null): string {
  if (!dateStr) return ''
  return new Date(dateStr).toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric'
  })
}

function onSort(event: any) {
  const fieldMap: Record<string, string> = {
    title: 'title',
    priority: 'priority',
    status: 'status',
    dueDate: 'dueDate',
    createdAt: 'createdAt'
  }
  const field = fieldMap[event.sortField] || 'createdAt'
  const direction = event.sortOrder === 1 ? 'asc' : 'desc'
  taskStore.setSorting(field, direction)
}

function onPage(event: any) {
  taskStore.setPage(event.page + 1)
}
</script>

<template>
  <div>
    <div class="page-header">
      <h2>My Tasks</h2>
      <Button
        label="New Task"
        icon="pi pi-plus"
        @click="openCreate"
      />
    </div>

    <div class="filters">
      <Select
        :modelValue="taskStore.filters.status"
        :options="statusFilterOptions"
        optionLabel="label"
        optionValue="value"
        placeholder="All Statuses"
        @update:modelValue="(val: any) => taskStore.setFilter('status', val)"
      />
      <Select
        :modelValue="taskStore.filters.priority"
        :options="priorityFilterOptions"
        optionLabel="label"
        optionValue="value"
        placeholder="All Priorities"
        @update:modelValue="(val: any) => taskStore.setFilter('priority', val)"
      />
    </div>

    <DataTable
      :value="taskStore.tasks"
      :loading="taskStore.loading"
      lazy
      :totalRecords="taskStore.totalCount"
      :rows="taskStore.filters.pageSize"
      paginator
      :first="((taskStore.filters.pageNumber ?? 1) - 1) * (taskStore.filters.pageSize ?? 10)"
      @page="onPage"
      @sort="onSort"
      removableSort
      sortMode="single"
      :rowsPerPageOptions="[5, 10, 20]"
      @update:rows="(val: number) => { taskStore.filters.pageSize = val; taskStore.filters.pageNumber = 1; taskStore.fetchTasks() }"
      tableStyle="min-width: 50rem"
    >
      <template #empty>
        <div class="empty-state">
          <p>No tasks yet. Create one to get started!</p>
        </div>
      </template>

      <Column field="title" header="Title" sortable style="min-width: 14rem">
        <template #body="{ data }">
          <span class="task-title" :class="{ completed: data.status === TaskItemStatus.Completed }">
            {{ data.title }}
          </span>
        </template>
      </Column>

      <Column field="priority" header="Priority" sortable style="width: 8rem">
        <template #body="{ data }">
          <Tag :value="getPriorityLabel(data.priority)" :severity="getPrioritySeverity(data.priority)" />
        </template>
      </Column>

      <Column field="status" header="Status" sortable style="width: 9rem">
        <template #body="{ data }">
          <Tag :value="getStatusLabel(data.status)" :severity="getStatusSeverity(data.status)" />
        </template>
      </Column>

      <Column field="dueDate" header="Due Date" sortable style="width: 9rem">
        <template #body="{ data }">
          {{ formatDate(data.dueDate) }}
        </template>
      </Column>

      <Column field="createdAt" header="Created" sortable style="width: 9rem">
        <template #body="{ data }">
          {{ formatDate(data.createdAt) }}
        </template>
      </Column>

      <Column header="Actions" style="width: 8rem">
        <template #body="{ data }">
          <div class="action-buttons">
            <Button
              icon="pi pi-pencil"
              text
              rounded
              severity="info"
              @click="openEdit(data)"
              aria-label="Edit"
            />
            <Button
              icon="pi pi-trash"
              text
              rounded
              severity="danger"
              @click="confirmDelete(data)"
              aria-label="Delete"
            />
          </div>
        </template>
      </Column>
    </DataTable>

    <Dialog
      v-model:visible="showDialog"
      :header="editingTask ? 'Edit Task' : 'New Task'"
      modal
      :style="{ width: '500px' }"
    >
      <p v-if="formError" class="form-error">{{ formError }}</p>
      <TaskForm
        :task="editingTask"
        :loading="formLoading"
        @submit="handleSubmit"
        @cancel="showDialog = false"
      />
    </Dialog>

    <ConfirmDialog />
  </div>
</template>

<style scoped>
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.page-header h2 {
  margin: 0;
}

.filters {
  display: flex;
  gap: 0.75rem;
  margin-bottom: 1rem;
}

.empty-state {
  text-align: center;
  padding: 2rem;
  color: var(--p-text-muted-color);
}

.task-title.completed {
  text-decoration: line-through;
  opacity: 0.6;
}

.action-buttons {
  display: flex;
  gap: 0.25rem;
}

.form-error {
  color: var(--p-red-500);
  margin-bottom: 1rem;
  font-size: 0.9rem;
}
</style>
