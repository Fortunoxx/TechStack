namespace TechStack.Web.IntegrationTests;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.SqlServer.Dac;
using TechStack.Infrastructure;
using TechStack.Web.IntegrationTests.Extensions;
using Testcontainers.MsSql;
using Xunit;

public class IntegrationTestFactory<TProgram, TDbContext> : WebApplicationFactory<TProgram>, IAsyncLifetime
    where TProgram : class
    where TDbContext : DbContext
{
    private const string DatabaseName = "TechStackDatabase";

    private const string PathToInitialDacPac = "../../../../Assets/Database/StackOverflow2010.dacpac";

    private const string PathToUpgradedDacPac = "../../../../Assets/Database/DatabaseProjectStackOverflow2010.dacpac";

    private readonly MsSqlContainer _container = new MsSqlBuilder().
        WithImage("mcr.microsoft.com/mssql/server:2022-latest").
        Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        MigrateDatabase(_container.GetConnectionString());
    }

    public new async Task DisposeAsync()
        => await _container.DisposeAsync();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var connectionStringBuilder = new SqlConnectionStringBuilder(_container.GetConnectionString())
        {
            InitialCatalog = DatabaseName
        };

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveDbContext<TDbContext>();
            services.AddDbContext<TDbContext>(options => options.UseSqlServer(connectionStringBuilder.ConnectionString));
            services.EnsureDbCreated<TDbContext>();
        });
    }

    private static void MigrateDatabase(string sqlConnectionString)
    {
        var builder = new SqlConnectionStringBuilder(sqlConnectionString);
        FillFromDacFx(builder, new FileInfo(PathToInitialDacPac), DatabaseName);
        FillFromDacFx(builder, new FileInfo(PathToUpgradedDacPac), DatabaseName);
    }

    private static void FillFromDacFx(SqlConnectionStringBuilder connectionStringBuilder, FileInfo dacpacFile, string catalog)
    {
        using var dacpacStream = dacpacFile.OpenRead();
        using DacPackage dacPackage = DacPackage.Load(dacpacStream);
        connectionStringBuilder.InitialCatalog = catalog;

        var dacpacService = new DacServices(connectionStringBuilder.ConnectionString);

        try
        {
            var dacDeployOptions = new DacDeployOptions { IgnorePermissions = true, };
            dacpacService.Deploy(dacPackage, connectionStringBuilder.InitialCatalog, true, dacDeployOptions);
        }
        catch
        {
            throw;
        }
    }
}