using Microsoft.Extensions.Logging;
using Moq;
using TaskManager.Api.Auth;
using TaskManager.Api.Data;
using TaskManager.Api.DTOs;
using TaskManager.Api.Models;
using TaskManager.Api.Services;

namespace TaskManager.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepo;
    private readonly Mock<IPasswordHasher> _hasher;
    private readonly Mock<IJwtTokenService> _jwt;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _userRepo = new Mock<IUserRepository>();
        _hasher = new Mock<IPasswordHasher>();
        _jwt = new Mock<IJwtTokenService>();
        var logger = new Mock<ILogger<AuthService>>();

        _sut = new AuthService(_userRepo.Object, _hasher.Object, _jwt.Object, logger.Object);
    }

    [Fact]
    public async Task RegisterAsync_NewUser_ReturnsTokenAndUser()
    {
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "password123",
            FirstName = "John",
            LastName = "Smith"
        };

        _userRepo.Setup(x => x.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _userRepo.Setup(x => x.CreateAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);
        _hasher.Setup(x => x.Hash(request.Password)).Returns("hashed_pw");
        _jwt.Setup(x => x.GenerateToken(It.IsAny<User>())).Returns("test_token");

        var (response, error, statusCode) = await _sut.RegisterAsync(request);

        Assert.Equal(201, statusCode);
        Assert.Null(error);
        Assert.NotNull(response);
        Assert.Equal("test_token", response.Token);
        Assert.Equal("test@example.com", response.User.Email);
        Assert.Equal("John", response.User.FirstName);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_Returns409()
    {
        var existing = new User { Id = Guid.NewGuid(), Email = "test@example.com" };
        _userRepo.Setup(x => x.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync(existing);

        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "password123",
            FirstName = "John",
            LastName = "Smith"
        };

        var (response, error, statusCode) = await _sut.RegisterAsync(request);

        Assert.Equal(409, statusCode);
        Assert.Null(response);
        Assert.Contains("already exists", error);
        _userRepo.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hashed_pw",
            FirstName = "John",
            LastName = "Smith"
        };

        _userRepo.Setup(x => x.GetByEmailAsync("test@example.com")).ReturnsAsync(user);
        _hasher.Setup(x => x.Verify("password123", "hashed_pw")).Returns(true);
        _jwt.Setup(x => x.GenerateToken(user)).Returns("test_token");

        var request = new LoginRequest { Email = "test@example.com", Password = "password123" };

        var (response, error, statusCode) = await _sut.LoginAsync(request);

        Assert.Equal(200, statusCode);
        Assert.Null(error);
        Assert.NotNull(response);
        Assert.Equal("test_token", response.Token);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_Returns401()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hashed_pw"
        };

        _userRepo.Setup(x => x.GetByEmailAsync("test@example.com")).ReturnsAsync(user);
        _hasher.Setup(x => x.Verify("wrong_password", "hashed_pw")).Returns(false);

        var request = new LoginRequest { Email = "test@example.com", Password = "wrong_password" };

        var (response, error, statusCode) = await _sut.LoginAsync(request);

        Assert.Equal(401, statusCode);
        Assert.Null(response);
        Assert.Contains("Invalid", error);
    }

    [Fact]
    public async Task LoginAsync_NonExistentEmail_Returns401()
    {
        _userRepo.Setup(x => x.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        var request = new LoginRequest { Email = "nobody@example.com", Password = "password123" };

        var (response, error, statusCode) = await _sut.LoginAsync(request);

        Assert.Equal(401, statusCode);
        Assert.Null(response);
        _hasher.Verify(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
