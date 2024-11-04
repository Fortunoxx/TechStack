namespace TechStack.Web.IntegrationTests.Mocks;

using AutoBogus;
using Bogus.Extensions;
using TechStack.Domain.Entities;
using TechStack.Web.IntegrationTests.Fixtures;

public class UserMetaDataFaker : AutoFaker<UserMetaData>
{
    public UserMetaDataFaker() : base(Constants.Locale)
    {
        RuleFor(x => x.MetaKey, faker => faker.Lorem.Word().ClampLength(max: 128));
        RuleFor(x => x.MetaValue, faker => faker.Lorem.Word());
        RuleFor(x => x.CreatedBy, faker => faker.Name.FirstName().ClampLength(max: 8));
        RuleFor(x => x.LastModifiedBy, faker => faker.Name.FirstName().ClampLength(max: 8));
    }
}
