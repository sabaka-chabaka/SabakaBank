using SabakaBank.Backend.Application.Common.Errors;

namespace SabakaBank.Backend.API.Extensions;

public static class ApiResponse
{
    public static IResult FromError(AppError error) => error.Code switch
    {
        "NotFound"         => Results.NotFound(new { error.Code, error.Message }),
        "Conflict"         => Results.Conflict(new { error.Code, error.Message }),
        "Forbidden"        => Results.Json(new { error.Code, error.Message }, statusCode: 403),
        "InvalidOperation" => Results.BadRequest(new { error.Code, error.Message }),
        _                  => Results.BadRequest(new { error.Code, error.Message })
    };
}
