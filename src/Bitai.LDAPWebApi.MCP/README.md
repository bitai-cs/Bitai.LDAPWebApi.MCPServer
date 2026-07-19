# Bitai.LDAPWebApi.MCP

MCP server for `Bitai.LDAPWebApi` built with the official `ModelContextProtocol` C# SDK with configurable `stdio` or Streamable HTTP transport.

## What it exposes

- **Tools** for LDAP profile IDs discovery, authentication, directory lookups, and MS AD user lifecycle operations.
- **Resources** for current server configuration, catalog types, and profile lookup.
- **Prompts** for guided LDAP lookup and safe account lifecycle workflows.

All API calls are executed through `Bitai.LDAPWebApi.Clients` (no direct `HttpClient` endpoint wiring).

## Configuration

Configure `LdapMcp` in `appsettings.json` or via environment variables:

- `TransportMode` (`Stdio` or `StreamableHttp`)
- `LDAPWebApiServer.ApiBaseUrl`
- `LDAPWebApiServer.DefaultLdapServerProfile`
- `LDAPWebApiServer.UseGlobalCatalog`
- `LDAPWebApiServer.UseBearerToken`
- `DefaultRequestLabel`
- `RequestTimeoutSeconds`
- `MaxRetries`
- `RetryDelayMilliseconds`
- `LDAPWebApiServer.OAuth.AuthorityUrl`
- `LDAPWebApiServer.OAuth.ApiScope`
- `LDAPWebApiServer.OAuth.ClientId`
- `LDAPWebApiServer.OAuth.ClientSecret`

OAuth values must be provided together when bearer token mode is enabled.

When `TransportMode` is `StreamableHttp`, the server hosts MCP over HTTP using `MapMcp()`.

## Run

```bash
dotnet run --project mcp-server/Bitai.LDAPWebApi.MCP
```
