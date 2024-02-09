using API.DTOs;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace API.Services;

public class PhotoService : IPhotoService
{
    private readonly Cloudinary _cloudinary;
    private readonly IMapper _mapper;

    public PhotoService(IOptions<CloudinarySettings> config, IMapper mapper)
    {
        var account = new Account
        (
            config.Value.CloudName,
            config.Value.ApiKey,
            config.Value.ApiSecret
        );

        _cloudinary = new Cloudinary(account);
        _mapper = mapper;
    }

    public async Task<ImageUploadDto> AddPhotoAsync(IFormFile file)
    {
        var uploadResult = new ImageUploadResult();

        if (file.Length > 0)
        {
            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, stream),
                Transformation = new Transformation()
                    .Height(500)
                    .Width(500)
                    .Crop("fill")
                    .Gravity("face")
            };
            uploadResult = await _cloudinary.UploadAsync(uploadParams);
        }
        return _mapper.Map<ImageUploadDto>(uploadResult);
    }

    public async Task<PhotoDeletionResultDto> DeletePhotoAsync(string publicId)
    {
        var deleteParams = new DeletionParams(publicId);
        var result = await _cloudinary.DestroyAsync(deleteParams);

        return _mapper.Map<PhotoDeletionResultDto>(result);
    }
}
