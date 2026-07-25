using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TestPlatform.Api.Common;
using TestPlatform.Application.Files;
using TestPlatform.Application.Users;

namespace TestPlatform.Api.Files;

[ApiController]
[Route("images")]
public class ImageController(
    IFileAssetService fileAssetService,
    ICurrentUserAccessor currentUserAccessor) : ApiControllerBase
{
    [HttpPost]
    [Authorize]
    [SwaggerOperation(
        OperationId = "UploadImage",
        Summary = "Upload an image",
        Description = "Uploads a temporary image and returns its identifier and stable API URL.")]
    public async Task<IActionResult> Upload(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.User;
        if (currentUser is null)
        {
            return Unauthorized();
        }

        await using var content = file.OpenReadStream();
        var result = await fileAssetService.UploadImageAsync(
            new FileUploadRequest(
                file.FileName,
                file.ContentType,
                file.Length,
                content),
            currentUser.Id,
            cancellationToken);

        if (result.IsFailure)
        {
            return ToErrorResult(result.Error);
        }

        var response = new UploadImageResponse(
            result.Value.FileId,
            Url.ActionLink(nameof(Get), values: new { fileId = result.Value.FileId })
            ?? $"/images/{result.Value.FileId}");

        return CreatedAtAction(
            nameof(Get),
            new { fileId = result.Value.FileId },
            response);
    }

    [HttpGet("{fileId:guid}")]
    [SwaggerOperation(
        OperationId = "GetImage",
        Summary = "Get an image",
        Description = "Returns image content by its identifier.")]
    public async Task<IActionResult> Get(
        Guid fileId,
        CancellationToken cancellationToken)
    {
        var result = await fileAssetService.GetStreamAsync(fileId, cancellationToken);
        if (result.IsFailure)
        {
            return ToErrorResult(result.Error);
        }

        return File(result.Value, "image/webp");
    }

    [HttpGet("{fileId:guid}/url")]
    [SwaggerOperation(
        OperationId = "GetImageUrl",
        Summary = "Create a temporary image URL",
        Description = "Returns a short-lived presigned storage URL for an image.")]
    public async Task<IActionResult> GetUrl(
        Guid fileId,
        CancellationToken cancellationToken)
    {
        var result = await fileAssetService.GetUrlAsync(fileId, cancellationToken);

        return result.IsSuccess
            ? Ok(new { url = result.Value })
            : ToErrorResult(result.Error);
    }

    [HttpDelete("{fileId:guid}")]
    [Authorize]
    [SwaggerOperation(
        OperationId = "DeleteImage",
        Summary = "Delete an image",
        Description = "Deletes the storage object and marks its database record as deleted.")]
    public async Task<IActionResult> Delete(
        Guid fileId,
        CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.User;
        if (currentUser is null)
        {
            return Unauthorized();
        }

        var result = await fileAssetService.DeleteAsync(
            fileId,
            currentUser.Id,
            currentUser.IsAdmin,
            cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : ToErrorResult(result.Error);
    }
}
