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
        RuleFor(fake => fake.AboutMe, x => x.Name.JobDescriptor());
        RuleFor(fake => fake.DisplayName, x => x.Name.FullName());
        RuleFor(fake => fake.EmailHash, x => x.Internet.Email(provider: domain).ClampLength(1,40));
        RuleFor(fake => fake.Location, x => x.Address.City());
        RuleFor(fake => fake.WebsiteUrl, x => x.Internet.Url());
    }
}
