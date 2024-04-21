namespace Infrastructure.IntegrationTests.Mocks;

using TechStack.Application.Common.Interfaces;

public class MockUser : IUser
{
    public string? Id => "MockUser";
}