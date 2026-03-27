using TaskManager.Api.DTOs;
using TaskManager.Api.Models;
using TaskManager.Api.Validators;

namespace TaskManager.Tests;

public class CreateTaskRequestValidatorTests
{
    private readonly CreateTaskRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidRequest_Passes()
    {
        var request = new CreateTaskRequest
        {
            Title = "Do the thing",
            Priority = Priority.Medium
        };

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyTitle_Fails()
    {
        var request = new CreateTaskRequest
        {
            Title = "",
            Priority = Priority.Low
        };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Title");
    }

    [Fact]
    public void Validate_TitleTooLong_Fails()
    {
        var request = new CreateTaskRequest
        {
            Title = new string('a', 201),
            Priority = Priority.Low
        };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Title");
    }

    [Fact]
    public void Validate_DescriptionTooLong_Fails()
    {
        var request = new CreateTaskRequest
        {
            Title = "Valid",
            Description = new string('a', 2001),
            Priority = Priority.Low
        };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description");
    }

    [Fact]
    public void Validate_PastDueDate_Fails()
    {
        var request = new CreateTaskRequest
        {
            Title = "Valid",
            Priority = Priority.Low,
            DueDate = DateTime.UtcNow.AddDays(-1)
        };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "DueDate");
    }

    [Fact]
    public void Validate_FutureDueDate_Passes()
    {
        var request = new CreateTaskRequest
        {
            Title = "Valid",
            Priority = Priority.Low,
            DueDate = DateTime.UtcNow.AddDays(7)
        };

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_NullDueDate_Passes()
    {
        var request = new CreateTaskRequest
        {
            Title = "Valid",
            Priority = Priority.Low,
            DueDate = null
        };

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_InvalidPriorityValue_Fails()
    {
        var request = new CreateTaskRequest
        {
            Title = "Valid",
            Priority = (Priority)99
        };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Priority");
    }
}

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidRequest_Passes()
    {
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "password123",
            FirstName = "John",
            LastName = "Smith"
        };

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyEmail_Fails()
    {
        var request = new RegisterRequest
        {
            Email = "",
            Password = "password123",
            FirstName = "John",
            LastName = "Smith"
        };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validate_InvalidEmailFormat_Fails()
    {
        var request = new RegisterRequest
        {
            Email = "not-an-email",
            Password = "password123",
            FirstName = "John",
            LastName = "Smith"
        };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validate_ShortPassword_Fails()
    {
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "short",
            FirstName = "John",
            LastName = "Smith"
        };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Password");
    }

    [Fact]
    public void Validate_EmptyFirstName_Fails()
    {
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "password123",
            FirstName = "",
            LastName = "Smith"
        };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "FirstName");
    }
}
