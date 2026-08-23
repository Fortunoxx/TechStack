namespace TechStack.Application.Common.Mappings;

using Mapster;
using TechStack.Application.Users.Commands;
using TechStack.Application.Users.Queries;
using TechStack.Domain.Entities;

public class UserMappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, GetUserByIdQueryResult>()
            .Ignore(dest => dest.Id);

        config.NewConfig<GetUserByIdQueryResult, User>()
            .Ignore(dest => dest.Id);

        config.NewConfig<AddUserCommand, User>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Created)
            .Ignore(dest => dest.CreatedBy)
            .Ignore(dest => dest.DomainEvents)
            .Ignore(dest => dest.LastModified)
            .Ignore(dest => dest.LastModifiedBy)
            .Ignore(dest => dest.MetaData);
    }
}