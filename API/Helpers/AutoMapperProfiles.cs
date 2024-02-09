using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;
using CloudinaryDotNet.Actions;

namespace API.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<AppUser, MemberDto>()
            .ForMember(dest => dest.PhotoUrl, opt =>
                opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url))
            .ForMember(dest => dest.Age, opt =>
                opt.MapFrom(src => src.DateOfBirth.CalculateAge()));

        CreateMap<Photo, PhotoDto>();

        CreateMap<MemberUpdateDto, AppUser>();

        CreateMap<RegisterDto, AppUser>()
            .ForMember(dest => dest.DateOfBirth, opt =>
                opt.MapFrom(scr => new DateTime(scr.DateOfBirth.Year, scr.DateOfBirth.Month, scr.DateOfBirth.Day, 0, 0, 0, DateTimeKind.Utc)));

        CreateMap<Message, MessageDto>()
            .ForMember(dest => dest.SenderPhotoUrl, opt =>
                opt.MapFrom(src => src.Sender.Photos.FirstOrDefault(p => p.IsMain).Url))
            .ForMember(dest => dest.RecipientPhotoUrl, opt =>
                opt.MapFrom(src => src.Recipient.Photos.FirstOrDefault(p => p.IsMain).Url));

        CreateMap<DeletionResult, PhotoDeletionResultDto>();

        CreateMap<ImageUploadResult, ImageUploadDto>()
            //.ForMember(dest => dest.ErrorMessage, opt =>
            //    opt.MapFrom(src => src.Error.Message))
            .ForMember(dest => dest.AbsoluteUri, opt =>
                opt.MapFrom(src => src.SecureUrl.AbsoluteUri));
    }
}
