using Bitai.LDAPWebApi.Clients;
using Bitai.LDAPWebApi.MCP.Configuration;
using Bitai.WebApi.Client;
using Microsoft.Extensions.Options;
using ModelContextProtocol;

namespace Bitai.LDAPWebApi.MCP;

public sealed class LdapClientFactory
{
    private readonly LdapMcpServerOptions _options;
    private readonly LdapWebApiServerOptions _ldapWebApiServerOptions;

    public LdapClientFactory(IOptions<LdapMcpServerOptions> options)
    {
        _options = options.Value;
        _ldapWebApiServerOptions = _options.LDAPWebApiServer;
    }

    public bool UseBearerToken => _ldapWebApiServerOptions.UseBearerToken;

    public bool UseGlobalCatalogByDefault => _ldapWebApiServerOptions.UseGlobalCatalog;

    public string? DefaultRequestLabel => _options.DefaultRequestLabel;

    public TimeSpan RequestTimeout => TimeSpan.FromSeconds(_options.RequestTimeoutSeconds);

    public int MaxRetries => _options.MaxRetries;

    public TimeSpan RetryDelay => TimeSpan.FromMilliseconds(_options.RetryDelayMilliseconds);

    public LDAPServerProfilesWebApiClient CreateServerProfilesClient()
    {
        var credential = BuildCredentialIfConfigured();
        return credential is null
            ? new LDAPServerProfilesWebApiClient(_ldapWebApiServerOptions.ApiBaseUrl)
            : new LDAPServerProfilesWebApiClient(_ldapWebApiServerOptions.ApiBaseUrl, credential);
    }

    public LDAPCatalogTypesWebApiClient CreateCatalogTypesClient()
    {
        var credential = BuildCredentialIfConfigured();
        return credential is null
            ? new LDAPCatalogTypesWebApiClient(_ldapWebApiServerOptions.ApiBaseUrl)
            : new LDAPCatalogTypesWebApiClient(_ldapWebApiServerOptions.ApiBaseUrl, credential);
    }

    public LDAPDirectoryWebApiClient CreateDirectoryClient(string? ldapServerProfile, bool? useGlobalCatalog = null)
    {
        var profile = ResolveProfile(ldapServerProfile);
        var credential = BuildCredentialIfConfigured();
        return credential is null
            ? new LDAPDirectoryWebApiClient(_ldapWebApiServerOptions.ApiBaseUrl, profile, useGlobalCatalog ?? _ldapWebApiServerOptions.UseGlobalCatalog)
            : new LDAPDirectoryWebApiClient(_ldapWebApiServerOptions.ApiBaseUrl, profile, useGlobalCatalog ?? _ldapWebApiServerOptions.UseGlobalCatalog, credential);
    }

    public LDAPUserDirectoryWebApiClient CreateUserDirectoryClient(string? ldapServerProfile, bool? useGlobalCatalog = null)
    {
        var profile = ResolveProfile(ldapServerProfile);
        var credential = BuildCredentialIfConfigured();
        return credential is null
            ? new LDAPUserDirectoryWebApiClient(_ldapWebApiServerOptions.ApiBaseUrl, profile, useGlobalCatalog ?? _ldapWebApiServerOptions.UseGlobalCatalog)
            : new LDAPUserDirectoryWebApiClient(_ldapWebApiServerOptions.ApiBaseUrl, profile, useGlobalCatalog ?? _ldapWebApiServerOptions.UseGlobalCatalog, credential);
    }

    public LDAPGroupsDirectoryWebApiClient CreateGroupsDirectoryClient(string? ldapServerProfile, bool? useGlobalCatalog = null)
    {
        var profile = ResolveProfile(ldapServerProfile);
        var credential = BuildCredentialIfConfigured();
        return credential is null
            ? new LDAPGroupsDirectoryWebApiClient(_ldapWebApiServerOptions.ApiBaseUrl, profile, useGlobalCatalog ?? _ldapWebApiServerOptions.UseGlobalCatalog)
            : new LDAPGroupsDirectoryWebApiClient(_ldapWebApiServerOptions.ApiBaseUrl, profile, useGlobalCatalog ?? _ldapWebApiServerOptions.UseGlobalCatalog, credential);
    }

    public LDAPAuthenticationsWebApiClient CreateAuthenticationsClient(string? ldapServerProfile, bool? useGlobalCatalog = null)
    {
        var profile = ResolveProfile(ldapServerProfile);
        var credential = BuildCredentialIfConfigured();
        return credential is null
            ? new LDAPAuthenticationsWebApiClient(_ldapWebApiServerOptions.ApiBaseUrl, profile, useGlobalCatalog ?? _ldapWebApiServerOptions.UseGlobalCatalog)
            : new LDAPAuthenticationsWebApiClient(_ldapWebApiServerOptions.ApiBaseUrl, profile, useGlobalCatalog ?? _ldapWebApiServerOptions.UseGlobalCatalog, credential);
    }

    public string ResolveProfile(string? requestedProfile)
    {
        var profile = string.IsNullOrWhiteSpace(requestedProfile) ? _ldapWebApiServerOptions.DefaultLdapServerProfile : requestedProfile;
        if (string.IsNullOrWhiteSpace(profile))
        {
            throw new McpException(
                $"No LDAP server profile was provided. Set {LdapMcpServerOptions.SectionName}:LDAPWebApiServer:DefaultLdapServerProfile or pass ldapServerProfile in the tool call.");
        }

        return profile;
    }

    private WebApiClientCredential? BuildCredentialIfConfigured()
    {
        if (!_ldapWebApiServerOptions.OAuth.HasAnyCredential)
        {
            return null;
        }

        return new WebApiClientCredential
        {
            AuthorityUrl = _ldapWebApiServerOptions.OAuth.AuthorityUrl ?? string.Empty,
            ApiScope = _ldapWebApiServerOptions.OAuth.ApiScope ?? string.Empty,
            ClientId = _ldapWebApiServerOptions.OAuth.ClientId ?? string.Empty,
            ClientSecret = _ldapWebApiServerOptions.OAuth.ClientSecret ?? string.Empty
        };
    }
}
