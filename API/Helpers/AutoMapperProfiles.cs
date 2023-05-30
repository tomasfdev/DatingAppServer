using API.DTOs;
using API.Extensions;
using API.Models;
using AutoMapper;

namespace API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<AppUser, UserDto>()
                .ForMember(destination => destination.PhotoUrl, options => options.MapFrom(source => source.Photos.FirstOrDefault(photo => photo.IsMain).Url))
                .ForMember(destination => destination.Age, options => options.MapFrom(source => source.DateOfBirth.CalculateAge()))
                .ReverseMap();
            CreateMap<Photo, PhotoDto>().ReverseMap();
            CreateMap<UserUpdateDto, AppUser>().ReverseMap();   //como as props correspondem exatamente ñ é preciso acrescentar nenhuma config
        }
    }
}
