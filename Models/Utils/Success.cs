namespace Chime_ASPNET.Models.Utils;

public static class Success
{
    public const string IS_AUTHENTICATED = "User has been successfully authenticated.";
    public const string PASSWORD_RESET_LINK_SENT = "A password reset link was sent to the email address you provided. Please check your inbox (and spam folder, just in case) and follow the instructions to reset your password.";

    public static string RESOURCE_CREATED(string resource) => $"{resource} has been successfully created.";
    public static string RESOURCE_FETCHED(string resource) => $"{resource} has been successfully fetched.";
    public static string RESOURCE_UPDATED(string resource) => $"{resource} has been successfully updated.";
    public static string RESOURCE_DELETED(string resource) => $"{resource} has been successfully deleted.";
}