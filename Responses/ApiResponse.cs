
namespace RbacApi.Responses;

public class ApiResponse<T>(
    int status,
    string message,
    T? data = default,
    List<Error>? errors = null) : ApiResponseBase(status, message, errors)
{
    public T? Data { get; set; } = data;

    public static ApiResponse<T> Ok(T data)
        => new(StatusCodes.Status200OK, "Ok", data);
}

public static class ApiResponse
{
    // BadRequest
    public static ApiResponseBase BadRequest(string message, List<Error>? errors = null)
        => new(StatusCodes.Status400BadRequest, message, errors);

    // Unauthorized
    public static ApiResponseBase Unauthorized(string message)
        => new(StatusCodes.Status401Unauthorized, message);

    // NotFound
    public static ApiResponseBase NotFound(string message)
        => new(StatusCodes.Status404NotFound, message);

    // InternalServerError
    public static ApiResponseBase Error(string message)
        => new(StatusCodes.Status500InternalServerError, message);
}
