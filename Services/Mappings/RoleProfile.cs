using AutoMapper;
using Data.Entities;
using Services.DTOs.Roles;

namespace Services.Mappings
{
    public class RoleProfile : Profile
    {
        public RoleProfile()
        {
            CreateMap<Permission, PermissionOptionDto>();

            CreateMap<RolePermission, PermissionOptionDto>()
                .ForMember(destination => destination.Id,
                    options => options.MapFrom(source => source.PermissionId))
                .ForMember(destination => destination.Name,
                    options => options.MapFrom(source => source.Permission != null ? source.Permission.Name : string.Empty))
                .ForMember(destination => destination.Description,
                    options => options.MapFrom(source => source.Permission != null ? source.Permission.Description : string.Empty))
                .ForMember(destination => destination.Module,
                    options => options.MapFrom(source => source.Permission != null ? source.Permission.Module ?? string.Empty : string.Empty))
                .ForMember(destination => destination.Submodule,
                    options => options.MapFrom(source => source.Permission != null ? source.Permission.Submodule ?? string.Empty : string.Empty))
                .ForMember(destination => destination.Label,
                    options => options.MapFrom(source => source.Permission != null ? source.Permission.Label ?? string.Empty : string.Empty))
                .ForMember(destination => destination.SortOrder,
                    options => options.MapFrom(source => source.Permission != null ? source.Permission.SortOrder : 0));

            CreateMap<Role, RoleDto>()
                .ForMember(destination => destination.Permissions,
                    options => options.MapFrom(source => source.RolePermissions.OrderBy(rp => rp.Permission.SortOrder)));
        }
    }
}
