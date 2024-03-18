namespace TechStack.Web.IntegrationTests.Mocks;

using AutoBogus;
using Bogus.Extensions;
using TechStack.Domain.Entities;
using TechStack.Web.IntegrationTests.Fixtures;

public class UserFaker : AutoFaker<User>
{
    public UserFaker() : this(Constants.EmailProvider)
    {
    }

    public UserFaker(string domain) : base(Constants.Locale)
    {
        RuleFor(x => x.AboutMe, faker => faker.Name.JobDescriptor());
        RuleFor(x => x.Age, faker => faker.Random.Int(min: 10, max: 120));
        RuleFor(x => x.DisplayName, faker => faker.Name.FullName());
        RuleFor(x => x.EmailHash, faker => faker.Internet.Email(provider: domain).ClampLength(1, 40));
        RuleFor(x => x.Location, faker => faker.Address.City());
        RuleFor(x => x.WebsiteUrl, faker => faker.Internet.Url());
        RuleFor(x => x.CreatedBy, faker => faker.Name.FirstName().ClampLength(max: 8));
        RuleFor(x => x.LastModifiedBy, faker => faker.Name.FirstName().ClampLength(max: 8));
    }
}
