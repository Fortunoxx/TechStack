namespace TechStack.Web.IntegrationTests;

using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.Keycloak;
using Xunit;

public class AuthIntegrationTestFactory<TProgram> : WebApplicationFactory<TProgram>, IAsyncLifetime
    where TProgram : class
{
    public string? BaseAddress { get; set; } = "https://localhost:8443";

    private readonly KeycloakContainer _container = new KeycloakBuilder()
        .WithImage("keycloak/keycloak:26.0")
        .WithPortBinding(8443, 8443)
        //map the realm configuration file import.json.
        .WithResourceMapping("./Tests/Import/import.json", "/opt/keycloak/data/import")
        //map the certificates
        .WithResourceMapping("./Tests/Certificates", "/opt/keycloak/certs")
        .WithCommand("--import-realm")
        .WithEnvironment("KC_HTTPS_CERTIFICATE_FILE", "/opt/keycloak/certs/certificate.pem")
        .WithEnvironment("KC_HTTPS_CERTIFICATE_KEY_FILE", "/opt/keycloak/certs/certificate.key")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8443))
        .WithCleanUp(true)
        .Build();

    public async Task InitializeAsync()
        => await _container.StartAsync();

    async Task IAsyncLifetime.DisposeAsync()
        => await _container.StopAsync();
}

[CollectionDefinition(nameof(AuthIntegrationTestFactoryCollection))]
public class AuthIntegrationTestFactoryCollection : ICollectionFixture<AuthIntegrationTestFactory<Program>>
{
}