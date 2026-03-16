using Microsoft.AspNetCore.Http;
using TestPlatform.Application.Abstractions.Enums;

namespace TestPlatform.Application.Abstractions;

public interface IImageStorageService
{
    Task<string> SaveTempAsync(IFormFile file, CancellationToken cancellationToken);

    Task<string> MoveToPermanentAsync(string tempFileName, ImageFolder folder, CancellationToken cancellationToken);

    Task DeleteTempAsync(string fileName);

    Task DeletePermanentAsync(ImageFolder folder, string fileName);

    Task<Stream> GetPermanentImageStreamAsync(ImageFolder folder, string fileName, CancellationToken cancellationToken);

    string GetPermanentImageUrl(ImageFolder folder, string fileName);
}