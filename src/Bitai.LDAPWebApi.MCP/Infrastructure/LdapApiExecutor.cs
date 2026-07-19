using Bitai.LDAPWebApi.Clients;
using Bitai.WebApi.Client;

namespace Bitai.LDAPWebApi.MCP;

public sealed class LdapApiExecutor
{
    private readonly LdapClientFactory _clientFactory;

    public LdapApiExecutor(LdapClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task<TResponse> ExecuteAsync<TClient, TResponse>(
        TClient client,
        Func<TClient, CancellationToken, Task<IHttpResponse>> operation,
        Func<TClient, IHttpResponse, Task<TResponse>> successSelector,
        CancellationToken cancellationToken = default)
        where TClient : LDAPWebApiBaseClient
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(_clientFactory.RequestTimeout);

        for (var attempt = 1; attempt <= _clientFactory.MaxRetries; attempt++)
        {
            try
            {
                var httpResponse = await operation(client, timeoutCts.Token);
                if (!httpResponse.IsSuccessResponse)
                {
                    client.ThrowClientRequestException("LDAP API call failed.", httpResponse);
                }

                return await successSelector(client, httpResponse);
            }
            catch (WebApiRequestException ex) when (attempt < _clientFactory.MaxRetries && IsTransient(ex))
            {
                await Task.Delay(_clientFactory.RetryDelay, cancellationToken);
            }
            catch (HttpRequestException) when (attempt < _clientFactory.MaxRetries)
            {
                await Task.Delay(_clientFactory.RetryDelay, cancellationToken);
            }
        }

        throw new InvalidOperationException("Unreachable code path while executing LDAP API operation.");
    }

    private static bool IsTransient(WebApiRequestException ex)
    {
        var statusCode = LdapError.TryGetHttpStatusCode(ex.NoSuccessResponse);
        return statusCode is 408 or 429 or >= 500;
    }
}
