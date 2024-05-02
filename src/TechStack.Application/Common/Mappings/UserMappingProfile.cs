namespace TechStack.Application.Mappings;

using AutoMapper;
using TechStack.Application.Users.Commands;
using TechStack.Application.Users.Queries;
using TechStack.Domain.Entities;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, GetUserByIdQueryResult>().
            ForMember(dest => dest.Id, opt => opt.Ignore()).
            ReverseMap();

        CreateMap<AddUserCommand, User>().
            ForMember(dest => dest.Id, opt => opt.Ignore()).
            ForMember(dest => dest.Created, opt => opt.Ignore()).
            ForMember(dest => dest.CreatedBy, opt => opt.Ignore()).
            ForMember(dest => dest.LastModified, opt => opt.Ignore()).
            ForMember(dest => dest.LastModifiedBy, opt => opt.Ignore()).
            ForMember(dest => dest.DomainEvents, opt => opt.Ignore());

        CreateMap<AlterUserCommandPart, User>().
            ForMember(dest => dest.Id, opt => opt.Ignore()).
            ForMember(dest => dest.Created, opt => opt.Ignore()).
            ForMember(dest => dest.CreatedBy, opt => opt.Ignore()).
            ForMember(dest => dest.LastModified, opt => opt.Ignore()).
            ForMember(dest => dest.LastModifiedBy, opt => opt.Ignore()).
            ForMember(dest => dest.DomainEvents, opt => opt.Ignore());
    }
}