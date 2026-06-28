using AspNetCoreRateLimit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NaarNoor.Application.Common.Interfaces;
using NaarNoor.Infrastructure.Data;
using NaarNoor.Infrastructure.Repositories;
using NaarNoor.Infrastructure.Services;

namespace NaarNoor.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = BuildConnectionString(configuration);

        // Database Context - PostgreSQL via Npgsql
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Rate Limiting (via built-in AspNetCoreRateLimit)
        services.AddMemoryCache();
        services.Configure<IpRateLimitOptions>(options =>
        {
            options.EnableEndpointRateLimiting = true;
            options.StackBlockedRequests = false;
            options.RealIpHeader = "X-Real-IP";
            options.HttpStatusCode = 429;
            options.GeneralRules = new List<RateLimitRule>
            {
                new() { Endpoint = "*/auth/register", Period = "1m", Limit = 5 },
                new() { Endpoint = "*/auth/login", Period = "1m", Limit = 10 },
                new() { Endpoint = "*", Period = "1m", Limit = 100 },
            };
        });
        services.AddInMemoryRateLimiting();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

        // Supabase Services - REST API based implementation
        var supabaseUrl = configuration["Supabase:Url"] ?? throw new InvalidOperationException("Supabase URL not configured");
        var supabaseAnonKey = configuration["Supabase:AnonKey"] ?? throw new InvalidOperationException("Supabase Anonymous Key not configured");
        
        services.AddHttpClient<ISupabaseAuthService, SupabaseAuthService>();
        services.AddHttpClient<ISupabaseStorageService, SupabaseStorageService>();
        services.AddHttpClient<ISupabaseRealtimeService, SupabaseRealtimeService>();

        var serviceRoleKey = Environment.GetEnvironmentVariable("SUPABASE_SERVICE_ROLE_KEY")
            ?? configuration["Supabase:ServiceRoleKey"]
            ?? "";

        services.AddSingleton(new SupabaseConfig 
        { 
            Url = supabaseUrl, 
            AnonKey = supabaseAnonKey,
            ServiceRoleKey = serviceRoleKey
        });

        return services;
    }

    private static string BuildConnectionString(IConfiguration configuration)
    {
        // 1. Replit built-in PostgreSQL — build from individual PG* env vars (always reliable)
        var pgHost = Environment.GetEnvironmentVariable("PGHOST");
        var pgPort = Environment.GetEnvironmentVariable("PGPORT") ?? "5432";
        var pgUser = Environment.GetEnvironmentVariable("PGUSER");
        var pgPassword = Environment.GetEnvironmentVariable("PGPASSWORD");
        var pgDatabase = Environment.GetEnvironmentVariable("PGDATABASE");

        if (!string.IsNullOrWhiteSpace(pgHost) && !string.IsNullOrWhiteSpace(pgUser))
            return $"Host={pgHost};Port={pgPort};Database={pgDatabase};Username={pgUser};Password={pgPassword};SSL Mode=Disable;";

        // 2. Explicit full connection string override
        var envConnectionString = Environment.GetEnvironmentVariable("POSTGRESQL_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(envConnectionString))
            return envConnectionString;

        // 3. Fall back to appsettings (for external dev environments with real values)
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Contains("YOUR_PASSWORD_HERE"))
            throw new InvalidOperationException(
                "Database connection string not found. Set PGHOST/PGUSER/PGPASSWORD/PGDATABASE environment variables.");

        return connectionString;
    }
}

public class SupabaseConfig
{
    public string Url { get; set; } = "";
    public string AnonKey { get; set; } = "";
    public string ServiceRoleKey { get; set; } = "";
}
