namespace Chime_ASPNET.Models.Utils;

public static class Error
{
    public enum ErrorType
    {
        ValidationError,
        BadRequest,
        NotFound,
        Unauthorized,
        InternalServerError
    }

    public const string ValidationError = "Validation failed.";
    public const string Unauthorized = "Access denied.";
    public const string NotFound = "Resource not found.";
    public const string AlreadyExists = "Resource already exists.";
    public const string ServerError = "Failed to process the request.";
    public const string EmailSendFailed = "Failed to send email";

    public static string FIELD_IS_REQUIRED(string field) => $"{field} is required.";
    public static string ERROR_CREATING_RESOURCE(string resource) => $"Failed to create {resource}.";
    public static string ERROR_FETCHING_RESOURCE(string resource) => $"Failed to fetch {resource}.";
    public static string ERROR_UPDATING_RESOURCE(string resource) => $"Failed to update {resource}.";
    public static string ERROR_DELETING_RESOURCE(string resource) => $"Failed to delete {resource}.";
}
