using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)] // tests in different classes will run in parallel, but not in the same class
// [assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)] // all tests in the assembly will run in the same collection

namespace TechStack.Web.IntegrationTests;

using TechStack.Infrastructure.Data;

[CollectionDefinition(nameof(DatabaseCollectionSetup))]
public class DatabaseCollectionSetup : ICollectionFixture<IntegrationTestFactory<Program, ApplicationDbContext>>
{
   // just a marker class for xUnit
}