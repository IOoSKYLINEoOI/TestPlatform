using TestPlatform.Core.Files;

namespace TestPlatform.Application.Files;

public interface IFileAssetsReadDbContext
{
    IQueryable<FileAsset> ReadFileAssets { get; }
}