using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpoMatic3000.Infrastructure;
using OpoMatic3000.Infrastructure.Persistence;

namespace OpoMatic3000.IntegrationTests.Persistence;

public sealed class InfrastructureConfigurationTests
{
    [Fact]
    public void Missing_connection_string_fails_with_a_diagnostic_message()
    {
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddInfrastructure(configuration));

        Assert.Contains(DependencyInjection.DatabaseConnectionStringName, exception.Message);
        Assert.Contains("User Secrets", exception.Message);
    }

    [Fact]
    public void Valid_configuration_registers_sql_server_without_sensitive_logging()
    {
        const string connectionString =
            "Server=localhost;Database=OpoMatic3000;Trusted_Connection=True;TrustServerCertificate=True";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"ConnectionStrings:{DependencyInjection.DatabaseConnectionStringName}"] = connectionString
            })
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(configuration);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OpoMatic3000DbContext>();

        Assert.True(context.Database.IsSqlServer());
        Assert.Equal("localhost", context.Database.GetDbConnection().DataSource);
        Assert.Equal("OpoMatic3000", context.Database.GetDbConnection().Database);
        Assert.False(context.GetService<Microsoft.EntityFrameworkCore.Diagnostics.IDiagnosticsLogger<
            Microsoft.EntityFrameworkCore.DbLoggerCategory.Infrastructure>>().Options.IsSensitiveDataLoggingEnabled);
    }
}
