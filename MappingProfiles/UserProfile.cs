using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Chime_ASPNET.Models.Dtos.Users;
using Chime_ASPNET.Models.Entities;

namespace Chime_ASPNET.MappingProfiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>();
    }
}
