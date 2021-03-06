using System.Linq;
using AutoMapper;
using DatingApp.WebAPI.DTO;
using DatingApp.WebAPI.Models;

namespace DatingApp.WebAPI.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<User, UserForListDto>()
                .ForMember(d => d.PhotoUrl, opt => opt.MapFrom(
                            src => src.Photos.FirstOrDefault(p => p.IsMain).UrlPhoto))
                .ForMember(d => d.Age, opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge()));
            CreateMap<User, UserForDetailDto>()
                .ForMember(d => d.PhotoUrl, opt => opt.MapFrom(
                            src => src.Photos.FirstOrDefault(p => p.IsMain).UrlPhoto))
                .ForMember(d => d.Age, opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge()));
            CreateMap<Photo, PhotosForDetailDto>();
            CreateMap<UserForUpdateDto, User>();
            CreateMap<PhotoForCreationDto, Photo>();
            CreateMap<Photo, PhotoForReturnDto>();
            CreateMap<UserForRegisterDto, User>();
            CreateMap<MessageForCreationDto, Message>().ReverseMap();
            CreateMap<Message, MessageToReturnDto>()
                .ForMember(x => x.SenderPhotoUrl, opt => 
                    opt.MapFrom(a => a.Sender.Photos.FirstOrDefault(p => p.IsMain).UrlPhoto))
                .ForMember(x => x.RecipientPhotoUrl, opt => 
                    opt.MapFrom(a => a.Recipient.Photos.FirstOrDefault(p => p.IsMain).UrlPhoto));
        }
    }
}