using API.DTOs;

namespace API.Interfaces;

public interface IPhotoService
{
    Task<ImageUploadDto> AddPhotoAsync(IFormFile file);
    Task<PhotoDeletionResultDto> DeletePhotoAsync(string Url);
}
