using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TestPlatform.Application.Files;
using TestPlatform.Application.Users;

namespace TestPlatform.Presenters.Image;

[ApiController]
[Route("images")]
public class ImageController : ControllerBase
{
    private readonly IFileAssetService _fileAssetService;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public ImageController(
        IFileAssetService fileAssetService,
        ICurrentUserAccessor currentUserAccessor)
    {
        _fileAssetService = fileAssetService;
        _currentUserAccessor = currentUserAccessor;
    }

    [HttpPost]
    [SwaggerOperation(
        OperationId = "UploadImage",
        Summary = "Загрузить изображение",
        Description = "Загружает изображение в объектное хранилище и возвращает идентификатор файла.")]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        var currentUser = _currentUserAccessor.User;
        if (currentUser is null)
            return Unauthorized();

        await using var content = file.OpenReadStream();
        var result = await _fileAssetService.UploadImageAsync(
            new FileUploadRequest(
                file.FileName,
                file.ContentType,
                file.Length,
                content),
            currentUser.Id,
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error });
    }

    [HttpGet("{fileId:guid}")]
    [SwaggerOperation(
        OperationId = "GetImage",
        Summary = "Получить изображение",
        Description = "Возвращает изображение по идентификатору файла.")]
    public async Task<IActionResult> Get(Guid fileId, CancellationToken cancellationToken)
    {
        var result = await _fileAssetService.GetStreamAsync(fileId, cancellationToken);

        if (result.IsFailure)
            return result.Error == "file.not_found"
                ? NotFound()
                : BadRequest(new { error = result.Error });

        return File(result.Value, "image/webp");
    }

    [HttpGet("{fileId:guid}/url")]
    [SwaggerOperation(
        OperationId = "GetImageUrl",
        Summary = "Получить URL изображения",
        Description = "Возвращает URL изображения по идентификатору файла.")]
    public async Task<IActionResult> GetUrl(Guid fileId, CancellationToken cancellationToken)
    {
        var result = await _fileAssetService.GetUrlAsync(fileId, cancellationToken);

        return result.IsSuccess
            ? Ok(new { url = result.Value })
            : NotFound(new { error = result.Error });
    }

    [HttpDelete("{fileId:guid}")]
    [SwaggerOperation(
        OperationId = "DeleteImage",
        Summary = "Удалить изображение",
        Description = "Помечает файл удалённым и удаляет объект из хранилища.")]
    public async Task<IActionResult> Delete(Guid fileId, CancellationToken cancellationToken)
    {
        var currentUser = _currentUserAccessor.User;
        if (currentUser is null)
            return Unauthorized();

        var result = await _fileAssetService.DeleteAsync(
            fileId,
            currentUser.Id,
            cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(new { error = result.Error });
    }
}
