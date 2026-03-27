<script setup lang="ts">
import { ref, watch } from 'vue'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import Select from 'primevue/select'
import DatePicker from 'primevue/datepicker'
import Button from 'primevue/button'
import { Priority, TaskItemStatus, type TaskResponse } from '../types'

const props = defineProps<{
  task?: TaskResponse | null
  loading?: boolean
}>()

const emit = defineEmits<{
  submit: [data: {
    title: string
    description?: string
    priority: Priority
    status?: TaskItemStatus
    dueDate?: string
  }]
  cancel: []
}>()

const isEditing = !!props.task

const title = ref(props.task?.title ?? '')
const description = ref(props.task?.description ?? '')
const priority = ref(props.task?.priority ?? Priority.Medium)
const status = ref(props.task?.status ?? TaskItemStatus.Pending)
const dueDate = ref<Date | null>(props.task?.dueDate ? new Date(props.task.dueDate) : null)

const priorityOptions = [
  { label: 'Low', value: Priority.Low },
  { label: 'Medium', value: Priority.Medium },
  { label: 'High', value: Priority.High },
  { label: 'Critical', value: Priority.Critical }
]

const statusOptions = [
  { label: 'Pending', value: TaskItemStatus.Pending },
  { label: 'In Progress', value: TaskItemStatus.InProgress },
  { label: 'Completed', value: TaskItemStatus.Completed }
]

// reset form when the task prop changes (opening dialog for a different task)
watch(() => props.task, (newTask) => {
  title.value = newTask?.title ?? ''
  description.value = newTask?.description ?? ''
  priority.value = newTask?.priority ?? Priority.Medium
  status.value = newTask?.status ?? TaskItemStatus.Pending
  dueDate.value = newTask?.dueDate ? new Date(newTask.dueDate) : null
})

function handleSubmit() {
  const data: any = {
    title: title.value,
    description: description.value || undefined,
    priority: priority.value,
    dueDate: dueDate.value?.toISOString() ?? undefined
  }

  if (isEditing) {
    data.status = status.value
  }

  emit('submit', data)
}
</script>

<template>
  <form @submit.prevent="handleSubmit" class="task-form">
    <div class="field">
      <label for="title">Title</label>
      <InputText
        id="title"
        v-model="title"
        placeholder="What needs to be done?"
        class="w-full"
      />
    </div>

    <div class="field">
      <label for="description">Description</label>
      <Textarea
        id="description"
        v-model="description"
        placeholder="Optional details..."
        rows="3"
        class="w-full"
        autoResize
      />
    </div>

    <div class="form-row">
      <div class="field">
        <label for="priority">Priority</label>
        <Select
          id="priority"
          v-model="priority"
          :options="priorityOptions"
          optionLabel="label"
          optionValue="value"
          class="w-full"
        />
      </div>

      <div v-if="isEditing" class="field">
        <label for="status">Status</label>
        <Select
          id="status"
          v-model="status"
          :options="statusOptions"
          optionLabel="label"
          optionValue="value"
          class="w-full"
        />
      </div>
    </div>

    <div class="field">
      <label for="dueDate">Due Date</label>
      <DatePicker
        id="dueDate"
        v-model="dueDate"
        showTime
        hourFormat="12"
        :minDate="new Date()"
        showIcon
        class="w-full"
        placeholder="Optional"
      />
    </div>

    <div class="form-actions">
      <Button
        type="button"
        label="Cancel"
        severity="secondary"
        text
        @click="emit('cancel')"
      />
      <Button
        type="submit"
        :label="isEditing ? 'Save Changes' : 'Create Task'"
        :loading="loading"
      />
    </div>
  </form>
</template>

<style scoped>
.task-form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.form-row {
  display: flex;
  gap: 1rem;
}

.form-row .field {
  flex: 1;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.5rem;
  padding-top: 0.5rem;
}
</style>
