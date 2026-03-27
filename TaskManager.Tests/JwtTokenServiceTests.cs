using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using TaskManager.Api.Auth;
using TaskManager.Api.Models;

namespace TaskManager.Tests;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _sut;

    public JwtTokenServiceTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "ThisIsATestSigningKeyThatIsLongEnoughForHmacSha256!",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpiryInMinutes"] = "60"
            })
            .Build();

        _sut = new JwtTokenService(config);
    }

    [Fact]
    public void GenerateToken_ReturnsValidJwt()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Smith",
            PasswordHash = "irrelevant"
        };

        var token = _sut.GenerateToken(user);

        Assert.False(string.IsNullOrEmpty(token));

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.Equal("TestIssuer", jwt.Issuer);
        Assert.Contains("TestAudience", jwt.Audiences);
    }

    [Fact]
    public void GenerateToken_ContainsUserClaims()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Smith",
            PasswordHash = "irrelevant"
        };

        var token = _sut.GenerateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.Equal(userId.ToString(), jwt.Subject);
        Assert.Contains(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "test@example.com");
        Assert.Contains(jwt.Claims, c => c.Type == "firstName" && c.Value == "John");
        Assert.Contains(jwt.Claims, c => c.Type == "lastName" && c.Value == "Smith");
    }

    [Fact]
    public void GenerateToken_ExpiresAtConfiguredTime()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Smith",
            PasswordHash = "irrelevant"
        };

        var before = DateTime.UtcNow;
        var token = _sut.GenerateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        // config says 60 minutes, so expiry should be roughly 60 min from now
        Assert.True(jwt.ValidTo > before.AddMinutes(59));
        Assert.True(jwt.ValidTo < before.AddMinutes(61));
    }
}
