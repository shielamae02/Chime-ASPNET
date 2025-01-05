using AutoMapper;
using Chime_ASPNET.Models.Dtos.Auth;
using Chime_ASPNET.Models.Entities;

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
