using TaskManager.Api.Models;

namespace TaskManager.Api.Data;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(Guid id, Guid userId);
    Task<(List<TaskItem> Items, int TotalCount)> GetPagedAsync(
        Guid userId,
        TaskItemStatus? status,
        Priority? priority,
        string sortBy,
        string sortDirection,
        int pageNumber,
        int pageSize);
    Task<TaskItem> CreateAsync(TaskItem task);
    Task<TaskItem> UpdateAsync(TaskItem task);
    Task<bool> DeleteAsync(Guid id, Guid userId);
}
