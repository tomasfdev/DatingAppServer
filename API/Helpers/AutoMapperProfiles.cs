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
            CreateMap<RegisterDto, AppUser>().ReverseMap(); //como as props correspondem exatamente ñ é preciso acrescentar nenhuma config
            CreateMap<Message, MessageDto>()
                //mapear SenderPhotoUrl e RecipientPhotoUrl porque são props que MessageDto tem e Message ñ tem, logo AutoMapper não consegue mapear... 
                //tou a ir buscar a photo ao Message.Sender que é uma nav prop de AppUser que contem Photos(AppUser.Photos) e aí está a prop da foto, Photo.Url !!
                .ForMember(desination => desination.SenderPhotoUrl, options => options.MapFrom(source => source.Sender.Photos.FirstOrDefault(photo => photo.IsMain).Url))
                .ForMember(desination => desination.RecipientPhotoUrl, options => options.MapFrom(source => source.Recipient.Photos.FirstOrDefault(photo => photo.IsMain).Url))
                .ReverseMap();
            CreateMap<MessageDto, Message>();
            CreateMap<DateTime, DateTime>().ConvertUsing(d => DateTime.SpecifyKind(d, DateTimeKind.Utc));
            CreateMap<DateTime?, DateTime?>().ConvertUsing(d => d.HasValue ? DateTime.SpecifyKind(d.Value, DateTimeKind.Utc) : null);
        }
    }
}
