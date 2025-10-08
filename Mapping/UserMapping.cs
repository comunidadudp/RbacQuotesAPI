using Mapster;
using RbacApi.Data.Entities;
using RbacApi.DTOs;

namespace RbacApi.Mapping;

public class UserMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        MapToGetUserInfoDTO(config);
    }

    private static void MapToGetUserInfoDTO(TypeAdapterConfig config)
    {
        config.NewConfig<User, GetUserInfoDTO>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Fullname, src => src.Fullname)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.Role, src => src.RoleId.ToString())
            .Map(dest => dest.Permissions, src => src.Permissions)
            .Map(dest => dest.Active, src => src.Active)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt.ToUniversalTime().ToString("o"));
    }
}