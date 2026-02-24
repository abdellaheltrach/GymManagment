namespace GymManagement.Application.Common.Models;

/// <summary>
/// Typed result wrapper. Every handler returns Result&lt;T&gt; — never raw values.
/// Business failures (not found, conflict, validation) are returned as typed errors.
/// Exceptions are reserved for unexpected infrastructure failures only.
/// Controllers check IsSuccess and render accordingly — no try/catch in controllers.
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public T? Value { get; }
    public string? Error { get; }
    public ResultErrorType ErrorType { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value     = value;
        ErrorType = ResultErrorType.None;
    }

    private Result(string error, ResultErrorType errorType)
    {
        IsSuccess = false;
        Error     = error;
        ErrorType = errorType;
    }

    public static Result<T> Success(T value)
        => new(value);

    public static Result<T> Failure(string error)
        => new(error, ResultErrorType.BusinessRule);

    public static Result<T> NotFound(string entityName, Guid id)
        => new($"{entityName} with id '{id}' was not found.", ResultErrorType.NotFound);

    public static Result<T> NotFound(string message)
        => new(message, ResultErrorType.NotFound);

    public static Result<T> Conflict(string message)
        => new(message, ResultErrorType.Conflict);

    public static Result<T> Forbidden(string message)
        => new(message, ResultErrorType.Forbidden);

    public static Result<T> ValidationError(string message)
        => new(message, ResultErrorType.Validation);
}

/// <summary>Non-generic Result for commands that return no value.</summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public ResultErrorType ErrorType { get; }

    private Result(bool success, string? error = null, ResultErrorType errorType = ResultErrorType.None)
    {
        IsSuccess = success;
        Error     = error;
        ErrorType = errorType;
    }

    public static Result Success()
        => new(true);

    public static Result Failure(string error)
        => new(false, error, ResultErrorType.BusinessRule);

    public static Result NotFound(string entityName, Guid id)
        => new(false, $"{entityName} with id '{id}' was not found.", ResultErrorType.NotFound);

    public static Result NotFound(string message)
        => new(false, message, ResultErrorType.NotFound);

    public static Result Conflict(string message)
        => new(false, message, ResultErrorType.Conflict);

    public static Result Forbidden(string message)
        => new(false, message, ResultErrorType.Forbidden);
}

public enum ResultErrorType
{
    None,
    NotFound,
    Conflict,
    BusinessRule,
    Validation,
    Forbidden
}
