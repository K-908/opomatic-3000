using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpoMatic3000.Application.Topics;
using OpoMatic3000.Application.Questions;
using OpoMatic3000.Infrastructure.Persistence;
using OpoMatic3000.Infrastructure.Questions;
using OpoMatic3000.Infrastructure.Topics;

namespace OpoMatic3000.Infrastructure;

public static class DependencyInjection
{
    public const string DatabaseConnectionStringName = "OpoMatic3000";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(DatabaseConnectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{DatabaseConnectionStringName}' is not configured. " +
                "In development, set it with .NET User Secrets as described in README.md.");
        }

        services.AddDbContext<OpoMatic3000DbContext>(options =>
            options
                .UseSqlServer(connectionString)
                .EnableSensitiveDataLogging(false));
        services.AddScoped<ITopicRepository, TopicRepository>();
        services.AddScoped<IQuestionRepository, QuestionRepository>();

        return services;
    }
}
