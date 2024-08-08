namespace TechStack.Application.UnitTests;

using AutoMapper;
using TechStack.Application.Mappings;

[Trait("Category", "UnitTest")]

public class MappingUnitTests
{
    [Fact]
    public void Application_Mapping_ShouldWorkAsync()
    {
        // Arrange 
        var cut = new MapperConfiguration(cfg => cfg.AddProfile<UserMapping>());

        // Act & Assert
        cut.AssertConfigurationIsValid();
    }
}