namespace Bitai.LDAPWebApi.MCP.Configuration;

public sealed class LdapMcpServerOptions
{
    #region Constants
    public const string EnvironmentVariablePrefix = "BITAI_LDAP_MCP__";

    public const string SectionName = "LdapMcp";

    public const string TransportModeStdio = "Stdio";

    public const string TransportModeStreamableHttp = "StreamableHttp";
    #endregion



    public string TransportMode { get; init; } = TransportModeStreamableHttp;

    /// <summary>
    /// Gets the CORS settings applied when the Streamable HTTP transport mode is enabled.
    /// </summary>
    public LdapMcpStreamableHttpCorsOptions StreamableHttpCors { get; init; } = new();

    /// <summary>
    /// Gets LDAP Web API connection and authentication options.
    /// </summary>
    public LdapWebApiServerOptions LDAPWebApiServer { get; init; } = new();

    public string? DefaultRequestLabel { get; init; }

    public int RequestTimeoutSeconds { get; init; } = 10;

    public int MaxRetries { get; init; } = 3;

    public int RetryDelayMilliseconds { get; init; } = 1000;

}

/// <summary>
/// Represents LDAP Web API server settings used by the MCP server.
/// </summary>
public sealed class LdapWebApiServerOptions
{
    /// <summary>
    /// Gets the base URL of the LDAP Web API.
    /// </summary>
    public string ApiBaseUrl { get; init; } = string.Empty;

    /// <summary>
    /// Gets the default LDAP server profile identifier.
    /// </summary>
    public string? DefaultLdapServerProfile { get; init; }

    /// <summary>
    /// Gets a value indicating whether global catalog is used by default.
    /// </summary>
    public bool UseGlobalCatalog { get; init; }

    /// <summary>
    /// Gets a value indicating whether the API client uses bearer token mode.
    /// </summary>
    public bool UseBearerToken { get; init; } = true;

    /// <summary>
    /// Gets OAuth client credential settings.
    /// </summary>
    public LdapMcpOAuthOptions OAuth { get; init; } = new();
}

/// <summary>
/// Represents CORS options for Streamable HTTP transport mode.
/// </summary>
public sealed class LdapMcpStreamableHttpCorsOptions
{
    /// <summary>
    /// Gets a value indicating whether any origin is allowed.
    /// </summary>
    public bool AllowAnyOrigin { get; init; } = true;

    /// <summary>
    /// Gets the list of allowed origins when <see cref="AllowAnyOrigin"/> is false.
    /// </summary>
    public IReadOnlyList<string> AllowedOrigins { get; init; } = Array.Empty<string>();
}

public sealed class LdapMcpOAuthOptions
{
    /// <summary>
    /// Gets the authority URL for OAuth token acquisition.
    /// </summary>
    public string? AuthorityUrl { get; init; }

    /// <summary>
    /// Gets the API scope used to request tokens.
    /// </summary>
    public string? ApiScope { get; init; }

    /// <summary>
    /// Gets the OAuth client identifier.
    /// </summary>
    public string? ClientId { get; init; }

    /// <summary>
    /// Gets the OAuth client secret.
    /// </summary>
    public string? ClientSecret { get; init; }

    public bool HasAnyCredential =>
        !string.IsNullOrWhiteSpace(AuthorityUrl) ||
        !string.IsNullOrWhiteSpace(ApiScope) ||
        !string.IsNullOrWhiteSpace(ClientId) ||
        !string.IsNullOrWhiteSpace(ClientSecret);

    public bool HasCompleteCredentials =>
        !string.IsNullOrWhiteSpace(AuthorityUrl) &&
        !string.IsNullOrWhiteSpace(ApiScope) &&
        !string.IsNullOrWhiteSpace(ClientId) &&
        !string.IsNullOrWhiteSpace(ClientSecret);
}

public static class LdapMcpServerOptionsValidator
{
    /// <summary>
    /// Validates MCP server options.
    /// </summary>
    /// <param name="options">Options instance to validate.</param>
    /// <param name="validationError">Validation error when invalid; otherwise empty string.</param>
    /// <returns><see langword="true"/> when valid; otherwise <see langword="false"/>.</returns>
    public static bool TryValidate(LdapMcpServerOptions options, out string validationError)
    {
        if (string.IsNullOrWhiteSpace(options.LDAPWebApiServer.ApiBaseUrl))
        {
            validationError = $"{LdapMcpServerOptions.SectionName}:LDAPWebApiServer:ApiBaseUrl is required.";
            return false;
        }

        if (!Uri.TryCreate(options.LDAPWebApiServer.ApiBaseUrl, UriKind.Absolute, out _))
        {
            validationError = $"{LdapMcpServerOptions.SectionName}:LDAPWebApiServer:ApiBaseUrl must be an absolute URI.";
            return false;
        }

        if (options.RequestTimeoutSeconds < 1 || options.RequestTimeoutSeconds > 300)
        {
            validationError = $"{LdapMcpServerOptions.SectionName}:RequestTimeoutSeconds must be between 1 and 300.";
            return false;
        }

        if (options.MaxRetries < 1 || options.MaxRetries > 5)
        {
            validationError = $"{LdapMcpServerOptions.SectionName}:MaxRetries must be between 1 and 5.";
            return false;
        }

        if (options.RetryDelayMilliseconds < 100 || options.RetryDelayMilliseconds > 5000)
        {
            validationError = $"{LdapMcpServerOptions.SectionName}:RetryDelayMilliseconds must be between 100 and 5000.";
            return false;
        }

        if (options.LDAPWebApiServer.OAuth.HasAnyCredential && !options.LDAPWebApiServer.OAuth.HasCompleteCredentials)
        {
            validationError =
                $"{LdapMcpServerOptions.SectionName}:LDAPWebApiServer:OAuth requires AuthorityUrl, ApiScope, ClientId and ClientSecret together.";
            return false;
        }

        if (!string.Equals(options.TransportMode, LdapMcpServerOptions.TransportModeStdio, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(options.TransportMode, LdapMcpServerOptions.TransportModeStreamableHttp, StringComparison.OrdinalIgnoreCase))
        {
            validationError =
                $"{LdapMcpServerOptions.SectionName}:TransportMode must be '{LdapMcpServerOptions.TransportModeStdio}' or '{LdapMcpServerOptions.TransportModeStreamableHttp}'.";
            return false;
        }

        if (string.Equals(options.TransportMode, LdapMcpServerOptions.TransportModeStreamableHttp, StringComparison.OrdinalIgnoreCase) &&
            !options.StreamableHttpCors.AllowAnyOrigin)
        {
            if (options.StreamableHttpCors.AllowedOrigins.Count is 0)
            {
                validationError =
                    $"{LdapMcpServerOptions.SectionName}:StreamableHttpCors:AllowedOrigins must contain at least one origin when AllowAnyOrigin is false.";
                return false;
            }

            foreach (var origin in options.StreamableHttpCors.AllowedOrigins)
            {
                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri) ||
                    (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                {
                    validationError =
                        $"{LdapMcpServerOptions.SectionName}:StreamableHttpCors:AllowedOrigins contains an invalid origin '{origin}'.";
                    return false;
                }
            }
        }

        validationError = string.Empty;
        return true;
    }
}
