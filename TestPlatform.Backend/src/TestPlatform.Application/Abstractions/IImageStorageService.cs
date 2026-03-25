using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using TestPlatform.Application.Abstractions.Enums;

namespace TestPlatform.Application.Abstractions;

public interface IImageStorageService
{
    Task<Result<string>> SaveTempAsync(IFormFile file, CancellationToken cancellationToken);

    Task<Result<string>> MoveToPermanent(string tempFileName, ImageFolder folder);

    Task<Result> DeleteTempAsync(string fileName);

    Task<Result> DeletePermanentAsync(ImageFolder folder, string fileName);

    Task<Result<Stream>> GetPermanentImageStreamAsync(ImageFolder folder, string fileName, CancellationToken cancellationToken);

    Result<string> GetPermanentImageUrl(ImageFolder folder, string fileName);

}
