using TaskManager.Api.DTOs;

namespace TaskManager.Api.Services;

public interface ITaskService
{
    Task<TaskResponse?> GetByIdAsync(Guid id, Guid userId);
    Task<PagedResponse<TaskResponse>> GetPagedAsync(
        Guid userId, string? status, string? priority,
        string sortBy, string sortDirection,
        int pageNumber, int pageSize);
    Task<TaskResponse> CreateAsync(CreateTaskRequest request, Guid userId);
    Task<TaskResponse?> UpdateAsync(Guid id, UpdateTaskRequest request, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid userId);
}
