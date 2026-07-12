namespace TechStack.Application.Common.Mappings;

using Riok.Mapperly.Abstractions;
using TechStack.Application.Users.Commands;
using TechStack.Application.Users.Queries;
using TechStack.Domain.Entities;

[Mapper]
public partial class UserMapper
{
    [MapProperty(nameof(User.AboutMe), nameof(GetUserByIdQueryResult.AboutMe))]
    public partial GetUserByIdQueryResult UserToGetUserByIdQueryResult(User user);

    [MapProperty(nameof(AddUserCommand.AboutMe), nameof(User.AboutMe))]
    public partial User AddUserCommandToUser(AddUserCommand command);

    [MapProperty(nameof(GetUserByIdQueryResult.AboutMe), nameof(User.AboutMe))]
    public partial User GetUserByIdQueryResultToUser(GetUserByIdQueryResult queryResult);
}