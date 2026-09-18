using ConferenceBooking.Application.Abstractions;
using ConferenceBooking.Domain.Pricing;
using ConferenceBooking.Infrastructure.Persistence;
using ConferenceBooking.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceBooking.Infrastructure;

/// <summary>
/// Реєстрація інфраструктури одним викликом -Program.cs не знає про EF Core
/// і конкретні реалізації.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Default")));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.Configure<PricingOptions>(configuration.GetSection(PricingOptions.SectionName));
        services.AddSingleton(sp =>
            sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<PricingOptions>>().Value);
        services.AddSingleton<IPricingPolicy, TimeBasedPricingPolicy>();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtService, JwtService>();

        return services;
    }
}