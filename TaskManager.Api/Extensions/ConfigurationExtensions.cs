namespace TaskManager.Api.Extensions;

public static class ConfigurationExtensions
{
    public static string GetJwtKey(this IConfiguration configuration)
    {
        return configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured");
    }

    public static string GetJwtIssuer(this IConfiguration configuration)
    {
        return configuration["Jwt:Issuer"] ?? "TaskManager.Api";
    }

    public static string GetJwtAudience(this IConfiguration configuration)
    {
        return configuration["Jwt:Audience"] ?? "TaskManager.Client";
    }

    public static int GetJwtExpiryInMinutes(this IConfiguration configuration)
    {
        return int.Parse(configuration["Jwt:ExpiryInMinutes"] ?? "480");
    }
}
