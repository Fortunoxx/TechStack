namespace TechStack.Application.Common.Mappings;

using AutoMapper;
using TechStack.Application.Users.Commands;
using TechStack.Application.Users.Queries;
using TechStack.Domain.Entities;

public class UserMapping : Profile
{
    public UserMapping()
    {
        CreateMap<User, GetUserByIdQueryResult>().
            ForMember(dest => dest.Id, opt => opt.Ignore()).
            ReverseMap();

        CreateMap<AddUserCommand, User>().
            ForMember(dest => dest.Id, opt => opt.Ignore()).
            ForMember(dest => dest.Created, opt => opt.Ignore()).
            ForMember(dest => dest.CreatedBy, opt => opt.Ignore()).
            ForMember(dest => dest.DomainEvents, opt => opt.Ignore()).
            ForMember(dest => dest.LastModified, opt => opt.Ignore()).
            ForMember(dest => dest.LastModifiedBy, opt => opt.Ignore()).
            ForMember(dest => dest.MetaData, opt => opt.Ignore());
    }
}