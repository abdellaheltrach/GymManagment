namespace GymManagement.Application.Common.Exceptions;

/// <summary>
/// Thrown only for unexpected infrastructure failures — not business rule violations.
/// Business failures must use Result&lt;T&gt;.Failure() instead.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string entityName, Guid id)
        : base($"{entityName} with id '{id}' was not found.") { }

    public NotFoundException(string message) : base(message) { }
}

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}

public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) { }
}

/// <summary>
/// Thrown by ValidationBehaviour when FluentValidation fails.
/// Global exception middleware in the Web layer maps this to a 400 response.
/// </summary>
public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IEnumerable<FluentValidation.Results.ValidationFailure> failures)
        : base("One or more validation failures occurred.")
    {
        Errors = failures
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }
}
