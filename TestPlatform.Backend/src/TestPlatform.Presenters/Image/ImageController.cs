using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Abstractions.Enums;

namespace TestPlatform.Presenters.Image;

[ApiController]
[Route("images")]
public class ImageController : ControllerBase
{
    private readonly IImageStorageService _imageStorage;

    public ImageController(IImageStorageService imageStorage) => _imageStorage = imageStorage;

    [HttpPost("temp")]
    [SwaggerOperation(
        OperationId = "UploadTempImage",
        Summary = "Загрузить изображение во временное хранилище.",
        Description = "Сохраняет изображение во временную папку и возвращает имя файла.")]
    public async Task<IActionResult> UploadTemp(IFormFile file)
    {
        var result = await _imageStorage.SaveTempAsync(file, CancellationToken.None);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(new { tempFileName = result.Value });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("temp/{fileName}")]
    [SwaggerOperation(
        OperationId = "DeleteTempImage",
        Summary = "Удалить временное изображение.",
        Description = "Удаляет изображение из временного хранилища.")]
    public async Task<IActionResult> DeleteTemp(string fileName)
    {
        var result = await _imageStorage.DeleteTempAsync(fileName);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("permanent/{folder}/{fileName}")]
    [SwaggerOperation(
        OperationId = "DeletePermanentImage",
        Summary = "Удалить постоянное изображение.",
        Description = "Удаляет изображение из постоянного хранилища.")]
    public async Task<IActionResult> DeletePermanent(ImageFolder folder, string fileName)
    {
        var result = await _imageStorage.DeletePermanentAsync(folder, fileName);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return NoContent();
    }

    [HttpGet("permanent/{folder}/{fileName}")]
    [SwaggerOperation(
        OperationId = "GetPermanentImage",
        Summary = "Получить изображение.",
        Description = "Возвращает изображение из постоянного хранилища.")]
    public async Task<IActionResult> GetPermanent(ImageFolder folder, string fileName)
    {
        var result = await _imageStorage.GetPermanentImageStreamAsync(folder, fileName, CancellationToken.None);

        if (!result.IsSuccess)
        {
            if (result.Error == "file.not_found")
                return NotFound();
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Error });
        }

        var stream = result.Value;
        return File(stream, "image/webp", fileName);
    }

    [HttpGet("url/permanent/{folder}/{fileName}")]
    [SwaggerOperation(
        OperationId = "GetPermanentImageUrl",
        Summary = "Получить URL изображения.",
        Description = "Возвращает публичный URL изображения.")]
    public IActionResult GetPermanentUrl(ImageFolder folder, string fileName)
    {
        var result = _imageStorage.GetPermanentImageUrl(folder, fileName);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(new { url = result.Value });
    }
}