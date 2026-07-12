namespace TechStack.Application.Common.Mappings;

using Riok.Mapperly.Abstractions;
using TechStack.Application.Users.Commands;
using TechStack.Application.Users.Queries;
using TechStack.Domain.Entities;

[Mapper]
public partial class UserMapper
{
    [MapperIgnoreSource(nameof(User.DomainEvents))]
    [MapperIgnoreSource(nameof(User.MetaData))]
    [MapProperty(nameof(User.AboutMe), nameof(GetUserByIdQueryResult.AboutMe))]
    public partial GetUserByIdQueryResult UserToGetUserByIdQueryResult(User user);

    [MapperIgnoreTarget(nameof(User.MetaData))]
    [MapperIgnoreTarget(nameof(User.Created))]
    [MapperIgnoreTarget(nameof(User.CreatedBy))]
    [MapperIgnoreTarget(nameof(User.LastModified))]
    [MapperIgnoreTarget(nameof(User.LastModifiedBy))]
    [MapperIgnoreTarget(nameof(User.Id))]
    [MapProperty(nameof(AddUserCommand.AboutMe), nameof(User.AboutMe))]
    public partial User AddUserCommandToUser(AddUserCommand command);

    [MapperIgnoreTarget(nameof(User.MetaData))]
    [MapProperty(nameof(GetUserByIdQueryResult.AboutMe), nameof(User.AboutMe))]
    public partial User GetUserByIdQueryResultToUser(GetUserByIdQueryResult queryResult);
}