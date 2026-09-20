using AutoMapper;
using Comercio.Api.DTOs.User;
using Comercio.Api.Models;

namespace Comercio.Api.Mappings
{
    public class UserProfile:Profile
    {
        public UserProfile()
        {
            CreateMap<RegisterDto, User>().ForMember(dest => dest.PasswordHash, opt => opt.Ignore());
        }
    }
}
