namespace TechStack.Web.IntegrationTests;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SqlServer.Dac;
using TechStack.Web.IntegrationTests.Extensions;
using Testcontainers.MsSql;
using Xunit;

public class IntegrationTestFactory<TProgram, TDbContext> : WebApplicationFactory<TProgram>, IAsyncLifetime
    where TProgram : class where TDbContext : DbContext
{
    // private const string PathToMigrations = "../../../../Database/Migrations";
    // private const string PathToTestData = "../../../../Database/SeedData";
    // private readonly string deployScriptPath = "../../../../Database/data/deployment_script.temp.sql";
    private const string PathToInitialDacPac = "../../../../Assets/Database/StackOverflow2010.dacpac";
    private readonly MsSqlContainer _container = new MsSqlBuilder().WithImage("mcr.microsoft.com/mssql/server:2022-latest").Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var connectionStringBuilder = new SqlConnectionStringBuilder(_container.GetConnectionString()); // get ip, port
        connectionStringBuilder.InitialCatalog = "TechStackDatabase";
        var connectionString = connectionStringBuilder.ConnectionString;

        builder.ConfigureTestServices(services =>
        {
            services.RemoveDbContext<TDbContext>();
            services.AddDbContext<TDbContext>(options => options.UseSqlServer(connectionString));
            services.EnsureDbCreated<TDbContext>();

            // services.AddMassTransitTestHarness();
        });
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        MigrateDatabase(_container.GetConnectionString());
    }

    private void MigrateDatabase(string sqlConnectionString)
    {
        var builder = new SqlConnectionStringBuilder(sqlConnectionString);
        var fileInfo = new FileInfo(PathToInitialDacPac);
        var initialCatalog = "TechStackDatabase";
        FillFromDacFx(builder, fileInfo, initialCatalog);
    }

    public new async Task DisposeAsync() => await _container.DisposeAsync();

    // private void FillDatabase(bool useDacPac)
    // {
    //     var connectionString = _container.GetConnectionString();

    //     if (useDacPac)
    //     {
    //         var builder = new SqlConnectionStringBuilder(connectionString);
    //         builder.InitialCatalog = "TestDB";
    //         // FillDbFromDacpac(builder);
    //         FillFromDacFx(builder);
    //         var evolveDacPac = new EvolveDb.Evolve(new SqlConnection(builder.ConnectionString), msg => Debug.WriteLine(msg))
    //         {
    //             // Locations = new[] { PathToTestData, }
    //         };
    //         evolveDacPac.Migrate();
    //         return;
    //     }

    //     var evolve = new EvolveDb.Evolve(new SqlConnection(connectionString), msg => Debug.WriteLine(msg))
    //     {
    //         Locations = new[] { PathToMigrations, PathToTestData }
    //     };
    //     evolve.Migrate();
    // }

    private void FillFromDacFx(SqlConnectionStringBuilder connectionStringBuilder, FileInfo dacpacFile, string catalog)
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

    // private void FillDbFromDacpac(SqlConnectionStringBuilder connectionStringBuilder, string databaseName = "master")
    // {
    //     var arguments = new[] {
    //         "SqlPackage",
    //         "/Action:Publish",
    //         "/SourceFile:\"../../../../Database/StackOverflow2010.dacpac\"",
    //         $"/TargetUser:{connectionStringBuilder.UserID}",
    //         $"/TargetPassword:{connectionStringBuilder.Password}",
    //         $"/TargetServerName:{connectionStringBuilder.DataSource}",
    //         $"/TargetTrustServerCertificate:{connectionStringBuilder.TrustServerCertificate}",
    //         $"/TargetDatabaseName:{databaseName}",
    //     };

    //     var startInfo = new ProcessStartInfo()
    //     {
    //         FileName = "dotnet",
    //         Arguments = string.Join(" ", arguments),
    //         UseShellExecute = false
    //     };

    //     using var process = Process.Start(startInfo);
    //     process?.WaitForExit();
    // }

    // private async Task FillDbFromDacpac2()
    // {
    //     using var dacpacStream = System.IO.File.OpenRead("../../../../Database/StackOverflow2010.dacpac");
    //     using DacPackage dacPackage = DacPackage.Load(dacpacStream);

    //     var dacService = new DacServices(_container.GetConnectionString());

    //     try
    //     {
    //         var deployScriptContent = dacService.GenerateDeployScript(dacPackage, "DACDB", new DacDeployOptions { IgnoreDefaultSchema = true, });
    //         System.IO.File.WriteAllText(deployScriptPath, deployScriptContent);
    //         await ReadSqlDeployScriptCopyAndExecInDockerContainerAsync(deployScriptContent);
    //     }
    //     catch
    //     {
    //         throw;
    //     }
    // }

    // private async Task ReadSqlDeployScriptCopyAndExecInDockerContainerAsync(string? deployScript = null)
    // {
    //     if (deployScript == null)
    //     {
    //         deployScript = System.IO.File.ReadAllText(deployScriptPath);
    //     }

    //     var execResult = await this.CopyAndExecSqlDbCreateScriptContainerAsync(deployScript);

    //     const int successExitCode = 0;
    //     if (execResult.ExitCode != successExitCode)
    //     {
    //         throw new System.Exception(execResult.Stderr);
    //     }
    // }

    // public async Task<ExecResult> CopyAndExecSqlDbCreateScriptContainerAsync(string scriptContent, CancellationToken ct = default)
    // {
    //     var constants = new
    //     {
    //         DefaultDataPathLinux = "/var/opt/mssql/data/",
    //         DefaultLogPathLinux = "/var/opt/mssql/log/",
    //         DefaultDataPathSqlEnvVar = "data",
    //         DefaultLogPathSqlEnvVar = "log",
    //         DatabaseName = "StackOverflow2010_TC",
    //         DefaultFilePrefix = "SOTC",
    //         DockerSqlDeployScriptPath = "/var/opt/mssql/script.sql",
    //         SqlServerDefaultConnection = "127.0.0.1,1433",
    //     };

    //     await this._container.CopyFileAsync(constants.DockerSqlDeployScriptPath, System.Text.Encoding.Default.GetBytes(scriptContent), 493, 0, 0, ct).ConfigureAwait(false);

    //     var sqlConnection = new SqlConnectionStringBuilder(this._container.GetConnectionString());

    //     string[] sqlCmds = new[]
    //     {
    //         "/opt/mssql-tools/bin/sqlcmd", "-b", "-r", "1",
    //         // "-S", sqlConnection.DataSource,
    //         "-S", constants.SqlServerDefaultConnection,
    //         "-U", sqlConnection.UserID, "-P", sqlConnection.Password,
    //         "-i", constants.DockerSqlDeployScriptPath,
    //         "-v", $"{constants.DefaultDataPathSqlEnvVar}={constants.DefaultDataPathLinux} {constants.DefaultLogPathSqlEnvVar}={constants.DefaultLogPathLinux}"
    //     };

    //     ExecResult execResult = await this._container.ExecAsync(sqlCmds, ct).ConfigureAwait(false);

    //     return execResult;
    // }
}