using AutoMapper;
using Chime_ASPNET.Models.Entities;
using Chime_ASPNET.Models.Dtos.Auth;

namespace Chime_ASPNET.MappingProfiles;

public class AuthProfile : Profile
{
    public AuthProfile()
    {
        CreateMap<RegisterDto, User>();
        CreateMap<LoginDto, User>();
        CreateMap<OAuthDto, User>();
    }
}
