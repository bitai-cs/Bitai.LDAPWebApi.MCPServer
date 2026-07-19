namespace Bitai.LDAPWebApi.MCP.Configuration;

public sealed class LdapMcpServerOptions
{
    public const string EnvironmentVariablePrefix = "BITAI_LDAP_MCP__";

    public const string SectionName = "LdapMcp";

    public const string TransportModeStdio = "Stdio";

    public const string TransportModeStreamableHttp = "StreamableHttp";

    public string ApiBaseUrl { get; init; } = string.Empty;

    public string? DefaultLdapServerProfile { get; init; }

    public bool UseGlobalCatalog { get; init; }

    public bool UseBearerToken { get; init; } = true;

    public string? DefaultRequestLabel { get; init; }

    public int RequestTimeoutSeconds { get; init; } = 30;

    public int MaxRetries { get; init; } = 2;

    public int RetryDelayMilliseconds { get; init; } = 400;

    public LdapMcpOAuthOptions OAuth { get; init; } = new();

    public string TransportMode { get; init; } = TransportModeStdio;
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

        validationError = string.Empty;
        return true;
    }
}
