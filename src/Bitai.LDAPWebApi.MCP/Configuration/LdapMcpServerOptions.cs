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

    public string ApiBaseUrl { get; init; } = string.Empty;

    public string? DefaultLdapServerProfile { get; init; }

    public bool UseGlobalCatalog { get; init; }

    public bool UseBearerToken { get; init; } = true;

    public string? DefaultRequestLabel { get; init; }

    public int RequestTimeoutSeconds { get; init; } = 10;

    public int MaxRetries { get; init; } = 3;

    public int RetryDelayMilliseconds { get; init; } = 1000;

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
    public string? AuthorityUrl { get; init; }

    public string? ApiScope { get; init; }

    public string? ClientId { get; init; }

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
    public static bool TryValidate(LdapMcpServerOptions options, out string validationError)
    {
        if (string.IsNullOrWhiteSpace(options.ApiBaseUrl))
        {
            validationError = $"{LdapMcpServerOptions.SectionName}:ApiBaseUrl is required.";
            return false;
        }

        if (!Uri.TryCreate(options.ApiBaseUrl, UriKind.Absolute, out _))
        {
            validationError = $"{LdapMcpServerOptions.SectionName}:ApiBaseUrl must be an absolute URI.";
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

        if (options.OAuth.HasAnyCredential && !options.OAuth.HasCompleteCredentials)
        {
            validationError =
                $"{LdapMcpServerOptions.SectionName}:OAuth requires AuthorityUrl, ApiScope, ClientId and ClientSecret together.";
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
