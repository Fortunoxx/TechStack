namespace TechStack.Application.Mappings;

using AutoMapper;
using TechStack.Application.Users.Commands;
using TechStack.Application.Users.Queries;
using TechStack.Domain.Entities;

public class UserMapping : Profile
{
    public UserMapping()
    {
        CreateMap<User, GetUserByIdQueryResult>().ReverseMap();
        CreateMap<AddUserCommand, User>();
    }
}