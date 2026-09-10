using AutoMapper;
using Data.Entities;
using Services.DTOs.Users;

namespace Services.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserRole, UserRoleDto>()
                .ForMember(destination => destination.Id,
                    options => options.MapFrom(source => source.RoleId))
                .ForMember(destination => destination.Name,
                    options => options.MapFrom(source => source.Role != null ? source.Role.Name : string.Empty));


            CreateMap<User, UserDto>()
                .ForMember(destination => destination.Roles,
                    options => options.MapFrom(source => source.UserRoles.OrderBy(ur => ur.Role != null ? ur.Role.Name : string.Empty)));
        }
    }
}
