using Bitai.LDAPWebApi.Clients;
using Bitai.LDAPWebApi.MCP.Configuration;
using Bitai.WebApi.Client;
using Microsoft.Extensions.Options;
using ModelContextProtocol;

namespace Bitai.LDAPWebApi.MCP;

public sealed class LdapClientFactory
{
    private readonly LdapMcpServerOptions _options;

    public LdapClientFactory(IOptions<LdapMcpServerOptions> options)
    {
        _options = options.Value;
    }

    public bool UseBearerToken => _options.UseBearerToken;

    public bool UseGlobalCatalogByDefault => _options.UseGlobalCatalog;

    public string? DefaultRequestLabel => _options.DefaultRequestLabel;

    public TimeSpan RequestTimeout => TimeSpan.FromSeconds(_options.RequestTimeoutSeconds);

    public int MaxRetries => _options.MaxRetries;

    public TimeSpan RetryDelay => TimeSpan.FromMilliseconds(_options.RetryDelayMilliseconds);

    public LDAPServerProfilesWebApiClient CreateServerProfilesClient()
    {
        var credential = BuildCredentialIfConfigured();
        return credential is null
            ? new LDAPServerProfilesWebApiClient(_options.ApiBaseUrl)
            : new LDAPServerProfilesWebApiClient(_options.ApiBaseUrl, credential);
    }

    public LDAPCatalogTypesWebApiClient CreateCatalogTypesClient()
    {
        var credential = BuildCredentialIfConfigured();
        return credential is null
            ? new LDAPCatalogTypesWebApiClient(_options.ApiBaseUrl)
            : new LDAPCatalogTypesWebApiClient(_options.ApiBaseUrl, credential);
    }

    public LDAPDirectoryWebApiClient CreateDirectoryClient(string? ldapServerProfile, bool? useGlobalCatalog = null)
    {
        var profile = ResolveProfile(ldapServerProfile);
        var credential = BuildCredentialIfConfigured();
        return credential is null
            ? new LDAPDirectoryWebApiClient(_options.ApiBaseUrl, profile, useGlobalCatalog ?? _options.UseGlobalCatalog)
            : new LDAPDirectoryWebApiClient(_options.ApiBaseUrl, profile, useGlobalCatalog ?? _options.UseGlobalCatalog, credential);
    }

    public LDAPUserDirectoryWebApiClient CreateUserDirectoryClient(string? ldapServerProfile, bool? useGlobalCatalog = null)
    {
        var profile = ResolveProfile(ldapServerProfile);
        var credential = BuildCredentialIfConfigured();
        return credential is null
            ? new LDAPUserDirectoryWebApiClient(_options.ApiBaseUrl, profile, useGlobalCatalog ?? _options.UseGlobalCatalog)
            : new LDAPUserDirectoryWebApiClient(_options.ApiBaseUrl, profile, useGlobalCatalog ?? _options.UseGlobalCatalog, credential);
    }

    public LDAPGroupsDirectoryWebApiClient CreateGroupsDirectoryClient(string? ldapServerProfile, bool? useGlobalCatalog = null)
    {
        var profile = ResolveProfile(ldapServerProfile);
        var credential = BuildCredentialIfConfigured();
        return credential is null
            ? new LDAPGroupsDirectoryWebApiClient(_options.ApiBaseUrl, profile, useGlobalCatalog ?? _options.UseGlobalCatalog)
            : new LDAPGroupsDirectoryWebApiClient(_options.ApiBaseUrl, profile, useGlobalCatalog ?? _options.UseGlobalCatalog, credential);
    }

    public LDAPAuthenticationsWebApiClient CreateAuthenticationsClient(string? ldapServerProfile, bool? useGlobalCatalog = null)
    {
        var profile = ResolveProfile(ldapServerProfile);
        var credential = BuildCredentialIfConfigured();
        return credential is null
            ? new LDAPAuthenticationsWebApiClient(_options.ApiBaseUrl, profile, useGlobalCatalog ?? _options.UseGlobalCatalog)
            : new LDAPAuthenticationsWebApiClient(_options.ApiBaseUrl, profile, useGlobalCatalog ?? _options.UseGlobalCatalog, credential);
    }

    public string ResolveProfile(string? requestedProfile)
    {
        var profile = string.IsNullOrWhiteSpace(requestedProfile) ? _options.DefaultLdapServerProfile : requestedProfile;
        if (string.IsNullOrWhiteSpace(profile))
        {
            throw new McpException(
                $"No LDAP server profile was provided. Set {LdapMcpServerOptions.SectionName}:DefaultLdapServerProfile or pass ldapServerProfile in the tool call.");
        }

        return profile;
    }

    private WebApiClientCredential? BuildCredentialIfConfigured()
    {
        if (!_options.OAuth.HasAnyCredential)
        {
            return null;
        }

        return new WebApiClientCredential
        {
            AuthorityUrl = _options.OAuth.AuthorityUrl ?? string.Empty,
            ApiScope = _options.OAuth.ApiScope ?? string.Empty,
            ClientId = _options.OAuth.ClientId ?? string.Empty,
            ClientSecret = _options.OAuth.ClientSecret ?? string.Empty
        };
    }
}
