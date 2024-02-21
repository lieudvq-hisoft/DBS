using AutoMapper;
using Data.Entities;
using Data.Model;
using Data.Models;

namespace Services.Mapping
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<RegisterModel, User>();
            CreateMap<User, UserModel>();
            CreateMap<User, ProfileModel>();
        }
    }
}
