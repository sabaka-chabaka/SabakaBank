namespace SabakaBank.Backend.Application.Common.Errors;

public record AppError(string Code, string Message)
{
    public static AppError NotFound(string message)         => new("NotFound", message);
    public static AppError Conflict(string message)         => new("Conflict", message);
    public static AppError Forbidden(string message)        => new("Forbidden", message);
    public static AppError InvalidOperation(string message) => new("InvalidOperation", message);
}