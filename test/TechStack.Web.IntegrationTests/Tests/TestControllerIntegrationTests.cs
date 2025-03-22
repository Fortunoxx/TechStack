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

    // private readonly AuthIntegrationTestFactory<Program> _factory = factory;

    // public Task InitializeAsync() => Task.CompletedTask; // await SeedDatabaseAsync();

    // public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    internal async Task TestsApi_GetSomeDataWithAuthorization_ShouldWorkAsync()
    {
        // Arrange 
        // var cut = _factory.CreateClient();

        //The realm and the client configured in the Keycloak server
        var realm = "myrealm";
        var client = "myclient";

        //Keycloak server token endpoint
        var url = $"{_baseAddress}/realms/{realm}/protocol/openid-connect/token";
        //Api secure endpoint 
        var apiUrl = "api/authenticate";

        //Create the url encoded body
        var data = new Dictionary<string, string>
        {
            { "grant_type", "password" },
            { "client_id", $"{client}" },
            { "username", "myuser" },
            { "password", "mypassword" }
        };

        //Get the access token from the Keycloak server
        var response = await _client.PostAsync(url, new FormUrlEncodedContent(data));
        var content = await response.Content.ReadFromJsonAsync<JsonObject>();
        var token = content?["access_token"]?.ToString();

        // Act

        //Add the access token to request header
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        //Call the Api secure endpoint
        var result = await _httpClient.GetAsync(apiUrl);

        var act = await cut.GetAsync("api/test/1");

        // Assert
        act.StatusCode.Should().Be(System.Net.HttpStatusCode.OK, "there should be some users");
        act.EnsureSuccessStatusCode();
    }

    // private async Task SeedDatabaseAsync()
    // {
    //     // Create a new instance of the Faker class
    //     AutoFaker.Configure(builder =>
    //     {
    //         builder.WithLocale("de");
    //         builder.WithConventions(cfg =>
    //         {
    //             cfg.StreetName.Aliases("Street", "Strasse", "Straße");
    //             cfg.PhoneNumber.Aliases("Phone", "Mobile", "Tel", "Telefon", "Fax", "Mobil", "Rufnummer");
    //             cfg.ZipCode.Aliases("PostalCode", "PLZ", "Postleitzahl");
    //         });

    //         // exclude all the auto-generated Ids
    //         builder.WithSkip<User>(x => x.Id);
    //         builder.WithSkip<UserMetaData>(x => x.Id);
    //         builder.WithSkip<UserMetaData>(x => x.User);

    //         // specify the custom faker for all objects that need special handling
    //         builder.WithOverride<UserMetaData>(_ => new UserMetaDataFaker());
    //     });

    //     // Generate fake data for a list of customers
    //     var userFaker = new UserFaker(Constants.EmailProvider);
    //     var users = userFaker.Generate(100);

    //     // Add the customers to the context and save changes
    //     _context.Users.AddRange(users);
    //     _ = await _context.SaveChangesAsync(new CancellationTokenSource().Token);
    // }
}