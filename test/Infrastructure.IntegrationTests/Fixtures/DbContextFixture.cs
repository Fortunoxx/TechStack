namespace Infrastructure.IntegrationTests.Fixtures;

using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Dac;
using System.Threading.Tasks;
using Testcontainers.MsSql;

public class DbContextFixture : IAsyncLifetime
{
    private const string DatabaseName = "TechStackDatabase";

    private const string PathToDacPac = "../../../../Assets/Database/DatabaseProjectStackOverflow2010.dacpac";

    private readonly MsSqlContainer _container = new MsSqlBuilder().
        WithImage("mcr.microsoft.com/mssql/server:2022-latest").
        Build();

    internal string SqlConnectionString => _container.GetConnectionString();

    internal static string InitialCatalog => DatabaseName;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        MigrateDatabase(_container.GetConnectionString());
    }

    public async Task DisposeAsync()
        => await _container.DisposeAsync();

    // TODO: share this logic across integration tests
    private static void MigrateDatabase(string sqlConnectionString)
    {
        var builder = new SqlConnectionStringBuilder(sqlConnectionString);
        FillFromDacFx(builder, new FileInfo(PathToDacPac), DatabaseName);
    }

    // TODO: share this logic across integration tests
    private static void FillFromDacFx(SqlConnectionStringBuilder connectionStringBuilder, FileInfo dacpacFile, string catalog)
    {
        using var dacpacStream = dacpacFile.OpenRead();
        using var dacPackage = DacPackage.Load(dacpacStream);
        connectionStringBuilder.InitialCatalog = catalog;

        var dacpacService = new DacServices(connectionStringBuilder.ConnectionString);

        try
        {
            var dacDeployOptions = new DacDeployOptions { IgnorePermissions = true, };
            dacpacService.Deploy(dacPackage, catalog, true, dacDeployOptions);
        }
        catch
        {
            throw;
        }
    }
}