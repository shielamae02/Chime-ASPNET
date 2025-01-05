using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Chime_ASPNET.Models.Response;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static Chime_ASPNET.Models.Utils.Error;

namespace Chime_ASPNET.Controllers.Utils;

public static class ControllerUtil
{
    public static int GetUserId(ClaimsPrincipal user)
    {
        var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(userIdString, out var userId) ? userId : -1;
    }

    public static IActionResult GetResultFromError<T>(ApiResponse<T> apiResponse)
    {
        var errorType = apiResponse.ErrorType;

        return errorType switch
        {
            ErrorType.NotFound => new NotFoundObjectResult(apiResponse),
            ErrorType.BadRequest => new BadRequestObjectResult(apiResponse),
            ErrorType.ValidationError => new BadRequestObjectResult(apiResponse),
            ErrorType.Unauthorized => new UnauthorizedObjectResult(apiResponse),
            ErrorType.InternalServerError => new StatusCodeResult(500),
            _ => new BadRequestObjectResult(apiResponse)
        };
    }

    public static ApiResponse<object> GenerateValidationError(ModelStateDictionary modelState)
    {
        var validationErrors = modelState
            .Where(ms => ms.Value.Errors.Count > 0)
            .ToDictionary(
                kvp => char.ToLower(kvp.Key[0]) + kvp.Key[1..],
                kvp => string.Join("; ", kvp.Value!.Errors.Select(e => e.ErrorMessage))
            // kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).FirstOrDefault()
            );

        return ApiResponse<object>.ErrorResponse(
            "Validation failed.",
            ErrorType.ValidationError,
            validationErrors
        );
    }
}
