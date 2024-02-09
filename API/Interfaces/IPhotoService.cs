using API.DTOs;
using CloudinaryDotNet.Actions;

namespace API.Interfaces;

public interface IPhotoService
{
    Task<ImageUploadDto> AddPhotoAsync(IFormFile file);
    Task<PhotoDeletionResultDto> DeletePhotoAsync(string publicId);
}
