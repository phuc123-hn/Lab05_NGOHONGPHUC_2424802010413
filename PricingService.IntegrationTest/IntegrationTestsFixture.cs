using Alba;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace PricingService.IntegrationTest;

public class IntegrationTestsFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer pgSqlContainer = new PostgreSqlBuilder()
        .WithDatabase("lab_netmicro_pricing")
        .WithCleanUp(true)
        .Build();

    public IAlbaHost SystemUnderTest { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await pgSqlContainer.StartAsync();

        SystemUnderTest = await AlbaHost.For<Program>(builder =>
        {
            builder.ConfigureAppConfiguration((ctx, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = pgSqlContainer.GetConnectionString(),
                    // khi chạy test không cần đăng ký Eureka
                    ["eureka:client:shouldRegisterWithEureka"] = "false"
                });
            });

            builder.ConfigureServices((ctx, services) => SetupServices(ctx, services));
        });
    }

    public async Task DisposeAsync()
    {
        await SystemUnderTest.DisposeAsync();
        await pgSqlContainer.DisposeAsync();
    }

    protected virtual void SetupServices(WebHostBuilderContext ctx, IServiceCollection services)
    {
    }

    protected Task SetupData()
    {
        return Task.CompletedTask;
    }
}
