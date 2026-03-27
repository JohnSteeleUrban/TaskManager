using TaskManager.Api.Data;
using TaskManager.Api.DTOs;
using TaskManager.Api.Models;

namespace TaskManager.Api.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly ILogger<TaskService> _logger;

    public TaskService(ITaskRepository taskRepository, ILogger<TaskService> logger)
    {
        _taskRepository = taskRepository;
        _logger = logger;
    }

    public async Task<TaskResponse?> GetByIdAsync(Guid id, Guid userId)
    {
        var task = await _taskRepository.GetByIdAsync(id, userId);
        return task == null ? null : (TaskResponse)task;
    }

    public async Task<PagedResponse<TaskResponse>> GetPagedAsync(
        Guid userId, string? status, string? priority,
        string sortBy, string sortDirection,
        int pageNumber, int pageSize)
    {
        // parse the string filter values to enums, null if invalid or not provided
        TaskItemStatus? statusFilter = Enum.TryParse<TaskItemStatus>(status, true, out var s) ? s : null;
        Priority? priorityFilter = Enum.TryParse<Priority>(priority, true, out var p) ? p : null;

        var (items, totalCount) = await _taskRepository.GetPagedAsync(
            userId, statusFilter, priorityFilter,
            sortBy, sortDirection, pageNumber, pageSize);

        return new PagedResponse<TaskResponse>
        {
            Items = items.Select(t => (TaskResponse)t).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request, Guid userId)
    {
        var task = new TaskItem
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Priority = request.Priority,
            Status = TaskItemStatus.Pending,
            DueDate = request.DueDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _taskRepository.CreateAsync(task);
        _logger.LogInformation("Task {TaskId} created for user {UserId}", task.Id, userId);

        return task;
    }

    public async Task<TaskResponse?> UpdateAsync(Guid id, UpdateTaskRequest request, Guid userId)
    {
        var task = await _taskRepository.GetByIdAsync(id, userId);
        if (task == null)
            return null;

        task.Title = request.Title.Trim();
        task.Description = request.Description?.Trim();
        task.Priority = request.Priority;
        task.Status = request.Status;
        task.DueDate = request.DueDate;
        task.UpdatedAt = DateTime.UtcNow;

        await _taskRepository.UpdateAsync(task);
        _logger.LogInformation("Task {TaskId} updated by user {UserId}", id, userId);

        return task;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var deleted = await _taskRepository.DeleteAsync(id, userId);

        if (deleted)
            _logger.LogInformation("Task {TaskId} deleted by user {UserId}", id, userId);

        return deleted;
    }
}
