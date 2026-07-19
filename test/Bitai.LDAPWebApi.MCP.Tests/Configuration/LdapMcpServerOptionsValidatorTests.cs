using Bitai.LDAPWebApi.MCP.Configuration;

namespace Bitai.LDAPWebApi.MCP.Tests.Configuration;

public class LdapMcpServerOptionsValidatorTests
{
    [Fact]
    public void TryValidate_ReturnsTrue_WhenConfigurationIsValid()
    {
        var options = new LdapMcpServerOptions
        {
            LDAPWebApiServer = new LdapWebApiServerOptions
            {
                ApiBaseUrl = "https://localhost:5101"
            },
            RequestTimeoutSeconds = 30,
            MaxRetries = 2,
            RetryDelayMilliseconds = 400
        };

        var isValid = LdapMcpServerOptionsValidator.TryValidate(options, out var error);

        Assert.True(isValid);
        Assert.Equal(string.Empty, error);
    }

    [Fact]
    public void TryValidate_ReturnsFalse_WhenApiBaseUrlIsMissing()
    {
        var options = new LdapMcpServerOptions
        {
            LDAPWebApiServer = new LdapWebApiServerOptions
            {
                ApiBaseUrl = ""
            },
            RequestTimeoutSeconds = 30,
            MaxRetries = 2,
            RetryDelayMilliseconds = 400
        };

        var isValid = LdapMcpServerOptionsValidator.TryValidate(options, out var error);

        Assert.False(isValid);
        Assert.Contains("ApiBaseUrl", error);
    }

    [Fact]
    public void TryValidate_ReturnsFalse_WhenOAuthIsPartiallyConfigured()
    {
        var options = new LdapMcpServerOptions
        {
            LDAPWebApiServer = new LdapWebApiServerOptions
            {
                ApiBaseUrl = "https://localhost:5101",
                OAuth = new LdapMcpOAuthOptions
                {
                    AuthorityUrl = "https://localhost/IsSts9",
                    ClientId = "client"
                }
            },
            RequestTimeoutSeconds = 30,
            MaxRetries = 2,
            RetryDelayMilliseconds = 400
        };

        var isValid = LdapMcpServerOptionsValidator.TryValidate(options, out var error);

        Assert.False(isValid);
        Assert.Contains("OAuth", error);
    }

    [Fact]
    public void TryValidate_ReturnsTrue_WhenTransportModeIsStreamableHttp()
    {
        var options = new LdapMcpServerOptions
        {
            LDAPWebApiServer = new LdapWebApiServerOptions
            {
                ApiBaseUrl = "https://localhost:5101"
            },
            TransportMode = LdapMcpServerOptions.TransportModeStreamableHttp,
            RequestTimeoutSeconds = 30,
            MaxRetries = 2,
            RetryDelayMilliseconds = 400
        };

        var isValid = LdapMcpServerOptionsValidator.TryValidate(options, out var error);

        Assert.True(isValid);
        Assert.Equal(string.Empty, error);
    }

    [Fact]
    public void TryValidate_ReturnsFalse_WhenTransportModeIsInvalid()
    {
        var options = new LdapMcpServerOptions
        {
            LDAPWebApiServer = new LdapWebApiServerOptions
            {
                ApiBaseUrl = "https://localhost:5101"
            },
            TransportMode = "Http",
            RequestTimeoutSeconds = 30,
            MaxRetries = 2,
            RetryDelayMilliseconds = 400
        };

        var isValid = LdapMcpServerOptionsValidator.TryValidate(options, out var error);

        Assert.False(isValid);
        Assert.Contains("TransportMode", error);
    }

    [Fact]
    public void TryValidate_ReturnsFalse_WhenStreamableHttpCorsOriginsAreMissing()
    {
        var options = new LdapMcpServerOptions
        {
            LDAPWebApiServer = new LdapWebApiServerOptions
            {
                ApiBaseUrl = "https://localhost:5101"
            },
            TransportMode = LdapMcpServerOptions.TransportModeStreamableHttp,
            RequestTimeoutSeconds = 30,
            MaxRetries = 2,
            RetryDelayMilliseconds = 400,
            StreamableHttpCors = new LdapMcpStreamableHttpCorsOptions
            {
                AllowAnyOrigin = false,
                AllowedOrigins = []
            }
        };

        var isValid = LdapMcpServerOptionsValidator.TryValidate(options, out var error);

        Assert.False(isValid);
        Assert.Contains("StreamableHttpCors:AllowedOrigins", error);
    }

    [Fact]
    public void TryValidate_ReturnsTrue_WhenStreamableHttpCorsAllowsAnyOrigin()
    {
        var options = new LdapMcpServerOptions
        {
            LDAPWebApiServer = new LdapWebApiServerOptions
            {
                ApiBaseUrl = "https://localhost:5101"
            },
            TransportMode = LdapMcpServerOptions.TransportModeStreamableHttp,
            RequestTimeoutSeconds = 30,
            MaxRetries = 2,
            RetryDelayMilliseconds = 400,
            StreamableHttpCors = new LdapMcpStreamableHttpCorsOptions
            {
                AllowAnyOrigin = true,
                AllowedOrigins = []
            }
        };

        var isValid = LdapMcpServerOptionsValidator.TryValidate(options, out var error);

        Assert.True(isValid);
        Assert.Equal(string.Empty, error);
    }
}
