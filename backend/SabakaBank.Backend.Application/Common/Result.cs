using SabakaBank.Backend.Application.Common.Errors;

namespace SabakaBank.Backend.Application.Common;

public class Result<TValue>
{
    public TValue?   Value    { get; }
    public AppError? Error    { get; }
    public bool      IsSuccess => Error is null;

    private Result(TValue value)  { Value = value; }
    private Result(AppError error) { Error = error; }

    public static Result<TValue> Ok(TValue value)      => new(value);
    public static Result<TValue> Fail(AppError error)  => new(error);

    public static implicit operator Result<TValue>(TValue value)    => Ok(value);
    public static implicit operator Result<TValue>(AppError error)  => Fail(error);
}