using System.ComponentModel;
using Bitai.LDAPHelper.DTO;
using Bitai.LDAPWebApi.DTO;
using Bitai.WebApi.Client;
using ModelContextProtocol.Server;

namespace Bitai.LDAPWebApi.MCP;

[McpServerToolType]
public sealed class LdapTools
{
    private readonly LdapClientFactory _clientFactory;
    private readonly LdapApiExecutor _executor;

    public LdapTools(LdapClientFactory clientFactory, LdapApiExecutor executor)
    {
        _clientFactory = clientFactory;
        _executor = executor;
    }

    [McpServerTool(Name = "ldap_get_server_profile_ids", ReadOnly = true, Idempotent = true, Destructive = false)]
    [Description("Gets all LDAP server profile IDs.")]
    public Task<LdapToolResponse<IReadOnlyList<string>>> GetServerProfileIdsAsync(CancellationToken cancellationToken = default)
    {
        return ExecuteToolAsync(
            "ldap_get_server_profile_ids",
            async ct =>
            {
                var client = _clientFactory.CreateServerProfilesClient();
                var profileIds = await _executor.ExecuteAsync(
                    client,
                    (c, token) => c.GetProfileIdsAsync(_clientFactory.UseBearerToken, token),
                    async (c, response) => (await c.GetEnumerableDTOFromResponseAsync<string>(response)).ToList(),
                    ct);

                return (IReadOnlyList<string>)profileIds;
            },
            cancellationToken);
    }

    [McpServerTool(Name = "ldap_get_catalog_types", ReadOnly = true, Idempotent = true, Destructive = false)]
    [Description("Gets LDAP catalog type names for local and global catalogs.")]
    public Task<LdapToolResponse<LDAPServerCatalogTypes>> GetCatalogTypesAsync(CancellationToken cancellationToken = default)
    {
        return ExecuteToolAsync(
            "ldap_get_catalog_types",
            async ct =>
            {
                var client = _clientFactory.CreateCatalogTypesClient();
                return await _executor.ExecuteAsync(
                    client,
                    (c, token) => c.GetAllAsync(_clientFactory.UseBearerToken, token),
                    (c, response) => c.GetDTOFromResponseAsync<LDAPServerCatalogTypes>(response),
                    ct);
            },
            cancellationToken);
    }

    [McpServerTool(Name = "ldap_authenticate_domain_account", ReadOnly = true, Idempotent = true, Destructive = false)]
    [Description("Authenticates an LDAP domain account against the configured LDAP server profile.")]
    public Task<LdapToolResponse<LWADomainAccountAuthenticationResult>> AuthenticateDomainAccountAsync(
        [Description("LDAP domain name, for example HOLDING.")] string domain,
        [Description("LDAP account name, for example victor.bastidas.")] string userAccount,
        [Description("Password for the LDAP account.")] string password,
        [Description("Optional LDAP server profile id. If omitted, DefaultLdapServerProfile is used.")] string? ldapServerProfile = null,
        [Description("Use global catalog for this request. Null uses server default.")] bool? useGlobalCatalog = null,
        [Description("Optional request label for correlation.")] string? requestLabel = null,
        CancellationToken cancellationToken = default)
    {
        return ExecuteToolAsync(
            "ldap_authenticate_domain_account",
            async ct =>
            {
                var client = _clientFactory.CreateAuthenticationsClient(ldapServerProfile, useGlobalCatalog);
                var credential = new LDAPDomainAccountCredential(domain, userAccount, password);

                return await _executor.ExecuteAsync(
                    client,
                    (c, token) => c.AuthenticateAsync(credential, requestLabel ?? _clientFactory.DefaultRequestLabel, _clientFactory.UseBearerToken, token),
                    (c, response) => c.GetDTOFromResponseAsync<LWADomainAccountAuthenticationResult>(response),
                    ct);
            },
            cancellationToken);
    }

    [McpServerTool(Name = "ldap_authenticate_domain_account_with_user_lookup", ReadOnly = true, Idempotent = true, Destructive = false)]
    [Description("Authenticates an LDAP domain account against the configured LDAP server profile.")]
    public Task<LdapToolResponse<LWADomainAccountAuthenticationResult>> AuthenticateDomainAccountWithUserLookupAsync(
        [Description("LDAP domain name, for example HOLDING.")] string domain,
        [Description("LDAP account name, for example victor.bastidas.")] string userAccount,
        [Description("Password for the LDAP account.")] string password,
        [Description("Optional LDAP server profile id. If omitted, DefaultLdapServerProfile is used.")] string? ldapServerProfile = null,
        [Description("Use global catalog for this request. Null uses server default.")] bool? useGlobalCatalog = null,
        [Description("Optional request label for correlation.")] string? requestLabel = null,
        CancellationToken cancellationToken = default)
    {
        return ExecuteToolAsync(
            "ldap_authenticate_domain_account_with_user_lookup",
            async ct =>
            {
                var client = _clientFactory.CreateAuthenticationsClient(ldapServerProfile, useGlobalCatalog);
                var credential = new LDAPDomainAccountCredential(domain, userAccount, password);

                return await _executor.ExecuteAsync(
                    client,
                    (c, token) => c.AuthenticateWithoutUserLookupAsync(credential, requestLabel ?? _clientFactory.DefaultRequestLabel, _clientFactory.UseBearerToken, token),
                    (c, response) => c.GetDTOFromResponseAsync<LWADomainAccountAuthenticationResult>(response),
                    ct);
            },
            cancellationToken);
    }

    [McpServerTool(Name = "ldap_search_directory_by_identifier", ReadOnly = true, Idempotent = true, Destructive = false)]
    [Description("Searches LDAP directory entries by account identifier.")]
    public Task<LdapToolResponse<LWASearchResult>> SearchDirectoryByIdentifierAsync(
        [Description("Identifier value, for example sAMAccountName.")] string identifier,
        [Description("Optional LDAP server profile id. If omitted, DefaultLdapServerProfile is used.")] string? ldapServerProfile = null,
        [Description("Optional identifier attribute type.")] EntryAttribute? identifierAttribute = null,
        [Description("Optional required attribute set.")] RequiredEntryAttributes? requiredAttributes = null,
        [Description("Use global catalog for this request. Null uses server default.")] bool? useGlobalCatalog = null,
        [Description("Optional request label for correlation.")] string? requestLabel = null,
        CancellationToken cancellationToken = default)
    {
        return ExecuteToolAsync(
            "ldap_search_directory_by_identifier",
            async ct =>
            {
                var client = _clientFactory.CreateDirectoryClient(ldapServerProfile, useGlobalCatalog);
                return await _executor.ExecuteAsync(
                    client,
                    (c, token) => c.SearchByIdentifierAsync(identifier, identifierAttribute, requiredAttributes, requestLabel ?? _clientFactory.DefaultRequestLabel, _clientFactory.UseBearerToken, token),
                    (c, response) => c.GetDTOFromResponseAsync<LWASearchResult>(response),
                    ct);
            },
            cancellationToken);
    }

    [McpServerTool(Name = "ldap_search_users", ReadOnly = true, Idempotent = true, Destructive = false)]
    [Description("Searches LDAP user entries using one or two filters.")]
    public Task<LdapToolResponse<LWASearchResult>> SearchUsersAsync(
        [Description("Main filter value.")] string filterValue,
        [Description("Optional primary filter attribute. Null defaults to user account name behavior.")] EntryAttribute? filterAttribute = null,
        [Description("Optional secondary filter attribute.")] EntryAttribute? secondFilterAttribute = null,
        [Description("Optional secondary filter value.")] string? secondFilterValue = null,
        [Description("Optional flag to combine filters.")] bool? combineFilters = null,
        [Description("Optional required attribute set.")] RequiredEntryAttributes? requiredAttributes = null,
        [Description("Optional LDAP server profile id. If omitted, DefaultLdapServerProfile is used.")] string? ldapServerProfile = null,
        [Description("Use global catalog for this request. Null uses server default.")] bool? useGlobalCatalog = null,
        [Description("Optional request label for correlation.")] string? requestLabel = null,
        CancellationToken cancellationToken = default)
    {
        return ExecuteToolAsync(
            "ldap_search_users",
            async ct =>
            {
                var client = _clientFactory.CreateUserDirectoryClient(ldapServerProfile, useGlobalCatalog);
                return await _executor.ExecuteAsync(
                    client,
                    (c, token) => c.SearchFilteringByAsync(filterAttribute, filterValue, secondFilterAttribute, secondFilterValue, combineFilters, requiredAttributes, requestLabel ?? _clientFactory.DefaultRequestLabel, _clientFactory.UseBearerToken, token),
                    (c, response) => c.GetDTOFromResponseAsync<LWASearchResult>(response),
                    ct);
            },
            cancellationToken);
    }

    [McpServerTool(Name = "ldap_search_groups", ReadOnly = true, Idempotent = true, Destructive = false)]
    [Description("Searches LDAP group entries by filter.")]
    public Task<LdapToolResponse<LWASearchResult>> SearchGroupsAsync(
        [Description("Primary filter attribute.")] EntryAttribute filterAttribute,
        [Description("Primary filter value.")] string filterValue,
        [Description("Optional secondary filter attribute.")] EntryAttribute? secondFilterAttribute = null,
        [Description("Optional secondary filter value.")] string? secondFilterValue = null,
        [Description("Optional flag to combine filters.")] bool? combineFilters = null,
        [Description("Optional required attribute set.")] RequiredEntryAttributes? requiredAttributes = null,
        [Description("Optional LDAP server profile id. If omitted, DefaultLdapServerProfile is used.")] string? ldapServerProfile = null,
        [Description("Use global catalog for this request. Null uses server default.")] bool? useGlobalCatalog = null,
        [Description("Optional request label for correlation.")] string? requestLabel = null,
        CancellationToken cancellationToken = default)
    {
        return ExecuteToolAsync(
            "ldap_search_groups",
            async ct =>
            {
                var client = _clientFactory.CreateGroupsDirectoryClient(ldapServerProfile, useGlobalCatalog);
                return await _executor.ExecuteAsync(
                    client,
                    (c, token) => c.SearchFilteringByAsync(filterAttribute, filterValue, secondFilterAttribute, secondFilterValue, combineFilters, requiredAttributes, requestLabel ?? _clientFactory.DefaultRequestLabel, _clientFactory.UseBearerToken, token),
                    (c, response) => c.GetDTOFromResponseAsync<LWASearchResult>(response),
                    ct);
            },
            cancellationToken);
    }

    [McpServerTool(Name = "ldap_create_msad_user", ReadOnly = false, Destructive = false)]
    [Description("Creates a new Microsoft Active Directory user account.")]
    public Task<LdapToolResponse<LWACreateMsADUserAccountResult>> CreateMsAdUserAsync(
        [Description("Payload for the new Active Directory user account.")] LDAPMsADUserAccount newUserAccount,
        [Description("Optional LDAP server profile id. If omitted, DefaultLdapServerProfile is used.")] string? ldapServerProfile = null,
        [Description("Use global catalog for this request. Null uses server default.")] bool? useGlobalCatalog = null,
        [Description("Optional request label for correlation.")] string? requestLabel = null,
        CancellationToken cancellationToken = default)
    {
        return ExecuteToolAsync(
            "ldap_create_msad_user",
            async ct =>
            {
                var client = _clientFactory.CreateUserDirectoryClient(ldapServerProfile, useGlobalCatalog);
                return await _executor.ExecuteAsync(
                    client,
                    (c, token) => c.CreateMsADUserAccountAsync(newUserAccount, requestLabel ?? _clientFactory.DefaultRequestLabel, _clientFactory.UseBearerToken, token),
                    (c, response) => c.GetDTOFromResponseAsync<LWACreateMsADUserAccountResult>(response),
                    ct);
            },
            cancellationToken);
    }

    [McpServerTool(Name = "ldap_set_msad_user_password", ReadOnly = false, Destructive = true)]
    [Description("Updates a Microsoft Active Directory user password.")]
    public Task<LdapToolResponse<LWAPasswordUpdateResult>> SetMsAdUserPasswordAsync(
        [Description("Target user account, for example HOLDING\\\\user or user.")] string userAccount,
        [Description("New password to set.")] string password,
        [Description("Identifier attribute used to locate account.")] EntryAttribute? identifierAttribute = EntryAttribute.sAMAccountName,
        [Description("Optional LDAP server profile id. If omitted, DefaultLdapServerProfile is used.")] string? ldapServerProfile = null,
        [Description("Use global catalog for this request. Null uses server default.")] bool? useGlobalCatalog = null,
        [Description("Optional request label for correlation.")] string? requestLabel = null,
        CancellationToken cancellationToken = default)
    {
        return ExecuteToolAsync(
            "ldap_set_msad_user_password",
            async ct =>
            {
                var client = _clientFactory.CreateUserDirectoryClient(ldapServerProfile, useGlobalCatalog);
                var credential = new LDAPCredential { UserAccount = userAccount, Password = password };

                return await _executor.ExecuteAsync(
                    client,
                    (c, token) => c.SetMsADUserAccountPasswordAsync(credential, identifierAttribute, requestLabel ?? _clientFactory.DefaultRequestLabel, _clientFactory.UseBearerToken, token),
                    (c, response) => c.GetDTOFromResponseAsync<LWAPasswordUpdateResult>(response),
                    ct);
            },
            cancellationToken);
    }

    [McpServerTool(Name = "ldap_disable_msad_user", ReadOnly = false, Destructive = true)]
    [Description("Disables a Microsoft Active Directory user account.")]
    public Task<LdapToolResponse<LWADisableUserAccountOperationResult>> DisableMsAdUserAsync(
        [Description("Identifier for the user account to disable.")] string identifier,
        [Description("Identifier attribute used to locate account.")] EntryAttribute? identifierAttribute = EntryAttribute.sAMAccountName,
        [Description("Optional LDAP server profile id. If omitted, DefaultLdapServerProfile is used.")] string? ldapServerProfile = null,
        [Description("Use global catalog for this request. Null uses server default.")] bool? useGlobalCatalog = null,
        [Description("Optional request label for correlation.")] string? requestLabel = null,
        CancellationToken cancellationToken = default)
    {
        return ExecuteToolAsync(
            "ldap_disable_msad_user",
            async ct =>
            {
                var client = _clientFactory.CreateUserDirectoryClient(ldapServerProfile, useGlobalCatalog);
                return await _executor.ExecuteAsync(
                    client,
                    (c, token) => c.DisableMsADUserAccountAsync(identifier, identifierAttribute, requestLabel ?? _clientFactory.DefaultRequestLabel, _clientFactory.UseBearerToken, token),
                    (c, response) => c.GetDTOFromResponseAsync<LWADisableUserAccountOperationResult>(response),
                    ct);
            },
            cancellationToken);
    }

    [McpServerTool(Name = "ldap_remove_msad_user", ReadOnly = false, Destructive = true)]
    [Description("Removes a Microsoft Active Directory user account.")]
    public Task<LdapToolResponse<LWARemoveMsADUserAccountResult>> RemoveMsAdUserAsync(
        [Description("Identifier for the user account to remove.")] string identifier,
        [Description("Identifier attribute used to locate account.")] EntryAttribute? identifierAttribute = EntryAttribute.sAMAccountName,
        [Description("Optional LDAP server profile id. If omitted, DefaultLdapServerProfile is used.")] string? ldapServerProfile = null,
        [Description("Use global catalog for this request. Null uses server default.")] bool? useGlobalCatalog = null,
        [Description("Optional request label for correlation.")] string? requestLabel = null,
        CancellationToken cancellationToken = default)
    {
        return ExecuteToolAsync(
            "ldap_remove_msad_user",
            async ct =>
            {
                var client = _clientFactory.CreateUserDirectoryClient(ldapServerProfile, useGlobalCatalog);
                return await _executor.ExecuteAsync(
                    client,
                    (c, token) => c.RemoveMsADUserAccountAsync(identifier, identifierAttribute, requestLabel ?? _clientFactory.DefaultRequestLabel, _clientFactory.UseBearerToken, token),
                    (c, response) => c.GetDTOFromResponseAsync<LWARemoveMsADUserAccountResult>(response),
                    ct);
            },
            cancellationToken);
    }

    private static async Task<LdapToolResponse<T>> ExecuteToolAsync<T>(
        string operation,
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken)
    {
        try
        {
            var data = await action(cancellationToken);
            return LdapToolResponse<T>.Ok(operation, data);
        }
        catch (WebApiRequestException ex)
        {
            return LdapToolResponse<T>.ApiError(operation, ex);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return LdapToolResponse<T>.Timeout(operation);
        }
        catch (HttpRequestException ex)
        {
            return LdapToolResponse<T>.Transport(operation, ex.Message);
        }
    }
}
