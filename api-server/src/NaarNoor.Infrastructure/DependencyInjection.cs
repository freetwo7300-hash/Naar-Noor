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

        services.AddSingleton(new SupabaseConfig 
        { 
            Url = supabaseUrl, 
            AnonKey = supabaseAnonKey,
            ServiceRoleKey = configuration["Supabase:ServiceRoleKey"] ?? ""
        });

        return services;
    }

    private static string BuildConnectionString(IConfiguration configuration)
    {
        // Try environment variable first
        var envConnectionString = Environment.GetEnvironmentVariable("POSTGRESQL_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(envConnectionString))
            return envConnectionString;

        // Try Supabase connection string
        var supabaseConnectionString = configuration["ConnectionStrings:SupabasePostgresql"];
        if (!string.IsNullOrWhiteSpace(supabaseConnectionString))
            return supabaseConnectionString;

        // Fall back to direct configuration
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "Database connection string not found. Set POSTGRESQL_CONNECTION_STRING environment variable or configure ConnectionStrings:SupabasePostgresql in appsettings.");

        return connectionString;
    }
}

public class SupabaseConfig
{
    public string Url { get; set; } = "";
    public string AnonKey { get; set; } = "";
    public string ServiceRoleKey { get; set; } = "";
}
