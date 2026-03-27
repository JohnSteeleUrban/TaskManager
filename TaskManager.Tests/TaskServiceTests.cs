using Microsoft.Extensions.Logging;
using Moq;
using TaskManager.Api.Data;
using TaskManager.Api.DTOs;
using TaskManager.Api.Models;
using TaskManager.Api.Services;

namespace TaskManager.Tests;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _taskRepo;
    private readonly TaskService _sut;
    private readonly Guid _userId = Guid.NewGuid();

    public TaskServiceTests()
    {
        _taskRepo = new Mock<ITaskRepository>();
        var logger = new Mock<ILogger<TaskService>>();
        _sut = new TaskService(_taskRepo.Object, logger.Object);
    }

    [Fact]
    public async Task CreateAsync_SetsDefaultsCorrectly()
    {
        var request = new CreateTaskRequest
        {
            Title = "  Test task  ",
            Description = "  Some description  ",
            Priority = Priority.High
        };

        _taskRepo
            .Setup(x => x.CreateAsync(It.IsAny<TaskItem>()))
            .ReturnsAsync((TaskItem t) => t);

        var result = await _sut.CreateAsync(request, _userId);

        Assert.Equal("Test task", result.Title);
        Assert.Equal("Some description", result.Description);
        Assert.Equal(Priority.High, result.Priority);
        Assert.Equal(TaskItemStatus.Pending, result.Status);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task CreateAsync_AlwaysSetsStatusToPending()
    {
        var request = new CreateTaskRequest
        {
            Title = "Test",
            Priority = Priority.Low
        };

        _taskRepo
            .Setup(x => x.CreateAsync(It.IsAny<TaskItem>()))
            .ReturnsAsync((TaskItem t) => t);

        var result = await _sut.CreateAsync(request, _userId);

        Assert.Equal(TaskItemStatus.Pending, result.Status);
    }

    [Fact]
    public async Task CreateAsync_SetsUserIdFromParameter()
    {
        var request = new CreateTaskRequest { Title = "Test", Priority = Priority.Low };

        TaskItem? captured = null;
        _taskRepo
            .Setup(x => x.CreateAsync(It.IsAny<TaskItem>()))
            .Callback<TaskItem>(t => captured = t)
            .ReturnsAsync((TaskItem t) => t);

        await _sut.CreateAsync(request, _userId);

        Assert.NotNull(captured);
        Assert.Equal(_userId, captured.UserId);
    }

    [Fact]
    public async Task UpdateAsync_ExistingTask_UpdatesFields()
    {
        var existing = new TaskItem
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            Title = "Old title",
            Status = TaskItemStatus.Pending,
            Priority = Priority.Low,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _taskRepo.Setup(x => x.GetByIdAsync(existing.Id, _userId)).ReturnsAsync(existing);
        _taskRepo.Setup(x => x.UpdateAsync(It.IsAny<TaskItem>())).ReturnsAsync((TaskItem t) => t);

        var request = new UpdateTaskRequest
        {
            Title = "New title",
            Priority = Priority.Critical,
            Status = TaskItemStatus.InProgress
        };

        var result = await _sut.UpdateAsync(existing.Id, request, _userId);

        Assert.NotNull(result);
        Assert.Equal("New title", result.Title);
        Assert.Equal(Priority.Critical, result.Priority);
        Assert.Equal(TaskItemStatus.InProgress, result.Status);
    }

    [Fact]
    public async Task UpdateAsync_TaskNotFound_ReturnsNull()
    {
        var taskId = Guid.NewGuid();
        _taskRepo.Setup(x => x.GetByIdAsync(taskId, _userId)).ReturnsAsync((TaskItem?)null);

        var request = new UpdateTaskRequest
        {
            Title = "Doesn't matter",
            Priority = Priority.Low,
            Status = TaskItemStatus.Pending
        };

        var result = await _sut.UpdateAsync(taskId, request, _userId);

        Assert.Null(result);
        _taskRepo.Verify(x => x.UpdateAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ExistingTask_ReturnsTrue()
    {
        var taskId = Guid.NewGuid();
        _taskRepo.Setup(x => x.DeleteAsync(taskId, _userId)).ReturnsAsync(true);

        var result = await _sut.DeleteAsync(taskId, _userId);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_WrongUser_ReturnsFalse()
    {
        var taskId = Guid.NewGuid();
        var wrongUserId = Guid.NewGuid();
        _taskRepo.Setup(x => x.DeleteAsync(taskId, wrongUserId)).ReturnsAsync(false);

        var result = await _sut.DeleteAsync(taskId, wrongUserId);

        Assert.False(result);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingTask_ReturnsMappedResponse()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            Title = "Test",
            Priority = Priority.Medium,
            Status = TaskItemStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _taskRepo.Setup(x => x.GetByIdAsync(task.Id, _userId)).ReturnsAsync(task);

        var result = await _sut.GetByIdAsync(task.Id, _userId);

        Assert.NotNull(result);
        Assert.Equal(task.Id, result.Id);
        Assert.Equal("Test", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        var taskId = Guid.NewGuid();
        _taskRepo.Setup(x => x.GetByIdAsync(taskId, _userId)).ReturnsAsync((TaskItem?)null);

        var result = await _sut.GetByIdAsync(taskId, _userId);

        Assert.Null(result);
    }
}
