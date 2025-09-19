namespace RbacApi.Responses;

public class ApiResponseBase(int status, string message, List<Error>? errors = null)
{
    public int Status { get; private set; } = status;
    public string Message { get; private set; } = message;
    public List<Error>? Errors { get; private set; } = errors;
}

public record Error(string PropertyName, string ErrorMessage);
