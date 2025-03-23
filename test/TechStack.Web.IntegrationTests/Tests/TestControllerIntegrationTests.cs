namespace TechStack.Web.IntegrationTests.Tests;

using System.Net.Http.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using Xunit;

[Collection(nameof(AuthIntegrationTestFactoryCollection))]
public sealed class TestControllerIntegrationTests(AuthIntegrationTestFactory<Program> factory)
{
    private readonly HttpClient _httpClient = factory.CreateClient();
    private readonly HttpClient _client = new();
    private readonly string _baseAddress = factory.BaseAddress ?? string.Empty;

    [Fact]
    internal async Task TestsApi_GetSomeDataWithAuthorization_ShouldWorkAsync()
    {
        // Arrange 
        //The realm and the client configured in the Keycloak server
        var realm = "myrealm";
        var client = "myclient";

        //Keycloak server token endpoint
        var url = $"{_baseAddress}/realms/{realm}/protocol/openid-connect/token";

        //Create the url encoded body
        var data = new Dictionary<string, string>
        {
            { "grant_type", "password" },
            { "client_id", $"{client}" },
            { "username", "myuser" },
            { "password", "1234" }
        };

        //Get the access token from the Keycloak server
        var response = await _client.PostAsync(url, new FormUrlEncodedContent(data));
        var content = await response.Content.ReadFromJsonAsync<JsonObject>();
        var token = content?["access_token"]?.ToString();

        //Add the access token to request header
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        //Api secure endpoint 
        var apiUrl = "api/test/1";
        
        // Act
        var act = await _httpClient.GetAsync(apiUrl);

        // Assert
        act.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        act.EnsureSuccessStatusCode();
    }
}