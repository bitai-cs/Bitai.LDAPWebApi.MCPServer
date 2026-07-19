using System.ComponentModel;
using System.Text.Json;
using Bitai.LDAPWebApi.MCP.Configuration;
using Bitai.LDAPWebApi.DTO;
using Bitai.WebApi.Client;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;

namespace Bitai.LDAPWebApi.MCP;

[McpServerResourceType]
public sealed class LdapResources
{
    private readonly LdapClientFactory _clientFactory;
    private readonly LdapApiExecutor _executor;
    private readonly LdapMcpServerOptions _options;

    public LdapResources(
        LdapClientFactory clientFactory,
        LdapApiExecutor executor,
        IOptions<LdapMcpServerOptions> options)
    {
        _clientFactory = clientFactory;
        _executor = executor;
        _options = options.Value;
    }

    [McpServerResource(Name = "ldap_catalog_types", UriTemplate = "ldap://catalog/types", MimeType = "application/json")]
    [Description("Gets catalog types from LDAP Web API as a resource.")]
    public async Task<string> GetCatalogTypesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _clientFactory.CreateCatalogTypesClient();
            var dto = await _executor.ExecuteAsync(
                client,
                (c, token) => c.GetAllAsync(_clientFactory.UseBearerToken, token),
                (c, response) => c.GetDTOFromResponseAsync<LDAPServerCatalogTypes>(response),
                cancellationToken);

            return JsonSerializer.Serialize(dto);
        }
        catch (WebApiRequestException ex)
        {
            return JsonSerializer.Serialize(LdapError.FromWebApiRequestException(ex));
        }
    }

    [McpServerResource(Name = "ldap_server_profile", UriTemplate = "ldap://profiles/{profileId}", MimeType = "application/json")]
    [Description("Gets a single LDAP server profile by profile id.")]
    public async Task<string> GetServerProfileByIdAsync(string profileId, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _clientFactory.CreateServerProfilesClient();
            var dto = await _executor.ExecuteAsync(
                client,
                (c, token) => c.GetByProfileIdAsync(profileId, _clientFactory.UseBearerToken, token),
                (c, response) => c.GetDTOFromResponseAsync<LDAPServerProfile>(response),
                cancellationToken);

            return JsonSerializer.Serialize(dto);
        }
        catch (WebApiRequestException ex)
        {
            return JsonSerializer.Serialize(LdapError.FromWebApiRequestException(ex));
        }
    }

    // [McpServerResource(Name = "ldap_server_configuration", UriTemplate = "ldap://config/server", MimeType = "application/json")]
    // [Description("Provides the active LDAP MCP server configuration (without secrets).")]
    // public string GetServerConfiguration()
    // {
    //     var model = new
    //     {
    //         _options.ApiBaseUrl,
    //         _options.DefaultLdapServerProfile,
    //         _options.UseGlobalCatalog,
    //         _options.UseBearerToken,
    //         _options.DefaultRequestLabel,
    //         _options.RequestTimeoutSeconds,
    //         _options.MaxRetries,
    //         _options.RetryDelayMilliseconds,
    //         OAuthConfigured = _options.OAuth.HasCompleteCredentials
    //     };

    //     return JsonSerializer.Serialize(model);
    // }
}
