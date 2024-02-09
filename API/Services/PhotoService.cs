using API.DTOs;
using API.Interfaces;

namespace API.Services;

public class PhotoService : IPhotoService
{
    private readonly string _host = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
    private readonly string _uploadDirectory = "wwwroot";
    private readonly string _uploadFolder = "uploads/test";

    public async Task<ImageUploadDto> AddPhotoAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("The file was not transferred or is empty.");
        }

        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), _uploadDirectory, _uploadFolder);
        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }
        var publicId = Guid.NewGuid().ToString();
        var fileName = publicId + Path.GetExtension(file.FileName);
        var filePath = Path.Combine(uploadPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var absoluteUri = new Uri(new Uri(_host), Path.Combine(_uploadFolder, fileName)).AbsoluteUri;

        return new ImageUploadDto
        {
            AbsoluteUri = absoluteUri,
            PublicId = publicId,
        };
    }

    public async Task<PhotoDeletionResultDto> DeletePhotoAsync(string Url)
    {
        var uri = new Uri(Url);
        if (!uri.AbsoluteUri.StartsWith(_host))
        {
            return new PhotoDeletionResultDto { ErrorMessage = "Unknown host. Cannot delete this photo" };
        }
        
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), _uploadDirectory, _uploadFolder , uri.Segments.Last());
        
        File.Delete(filePath);

        return new PhotoDeletionResultDto();
    }
}
