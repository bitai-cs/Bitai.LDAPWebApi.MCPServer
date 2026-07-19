using System.Net;
using Bitai.WebApi.Client;

namespace Bitai.LDAPWebApi.MCP;

public sealed record LdapToolResponse<T>(
    bool Success,
    string Operation,
    T? Data,
    LdapError? Error)
{
    public static LdapToolResponse<T> Ok(string operation, T data) =>
        new(true, operation, data, null);

    public static LdapToolResponse<T> ApiError(string operation, WebApiRequestException exception) =>
        new(false, operation, default, LdapError.FromWebApiRequestException(exception));

    public static LdapToolResponse<T> Timeout(string operation) =>
        new(false, operation, default, new LdapError("timeout", "LDAP API call timed out."));

    public static LdapToolResponse<T> Transport(string operation, string message) =>
        new(false, operation, default, new LdapError("transport_error", message));
}

public sealed record LdapError(
    string Code,
    string Message,
    int? HttpStatusCode = null,
    string? ReasonPhrase = null,
    string? ContentType = null,
    string? Content = null)
{
    public static LdapError FromWebApiRequestException(WebApiRequestException exception)
    {
        var noSuccessResponse = exception.NoSuccessResponse;
        return new LdapError(
            Code: "api_error",
            Message: exception.Message,
            HttpStatusCode: TryGetHttpStatusCode(noSuccessResponse),
            ReasonPhrase: TryReadStringProperty(noSuccessResponse, "ReasonPhrase"),
            ContentType: TryReadStringProperty(noSuccessResponse, "ContentMediaType"),
            Content: TryReadStringProperty(noSuccessResponse, "Content"));
    }

    public static int? TryGetHttpStatusCode(object? noSuccessResponse)
    {
        if (noSuccessResponse is null)
        {
            return null;
        }

        var value = noSuccessResponse.GetType().GetProperty("HttpStatusCode")?.GetValue(noSuccessResponse);
        return value switch
        {
            HttpStatusCode statusCode => (int)statusCode,
            int number => number,
            _ => null
        };
    }

    private static string? TryReadStringProperty(object? source, string propertyName)
    {
        return source?.GetType().GetProperty(propertyName)?.GetValue(source)?.ToString();
    }
}
