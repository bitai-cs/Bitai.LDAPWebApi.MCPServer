using Bitai.LDAPWebApi.MCP.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.AspNetCore;

namespace Bitai.LDAPWebApi.MCP;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var transportMode = ResolveTransportMode();

        if (string.Equals(transportMode, LdapMcpServerOptions.TransportModeStreamableHttp, StringComparison.OrdinalIgnoreCase))
        {
            await RunStreamableHttpAsync(args);
            return;
        }

        await RunStdioAsync(args);
    }

    private static async Task RunStdioAsync(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // Stdio need a clear buffered console to work properly, so we need to disable the console logger.
        builder.Logging.ClearProviders();

        var mcpBuilder = ConfigureServices(builder, builder.Services, builder.Configuration, useStreamableHttp: false);

        await builder.Build().RunAsync();
    }

    private static async Task RunStreamableHttpAsync(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Configuration.AddEnvironmentVariables(prefix: LdapMcpServerOptions.EnvironmentVariablePrefix);

        ConfigureServices(builder, builder.Services, builder.Configuration, useStreamableHttp: true);

        var app = builder.Build();
        app.UseCors();
        app.MapMcp("/mcp");

        await app.RunAsync();
    }
    
    private static IMcpServerBuilder ConfigureServices(IHostApplicationBuilder builder, IServiceCollection services, IConfiguration configuration, bool useStreamableHttp)
    {
        // builder.Configuration.AddEnvironmentVariables(prefix: LdapMcpServerOptions.EnvironmentVariablePrefix);

        services
            .AddOptions<LdapMcpServerOptions>()
            .Bind(configuration.GetSection(LdapMcpServerOptions.SectionName))
            .Validate(
                static options => LdapMcpServerOptionsValidator.TryValidate(options, out _),
                $"The {LdapMcpServerOptions.SectionName} configuration section is invalid.")
            .ValidateOnStart();

        services.AddSingleton<LdapClientFactory>();
        services.AddSingleton<LdapApiExecutor>();

        if (useStreamableHttp)
        {
            // Read the CORS settings from configuration and apply them to the CORS policy.
            var corsOptions = configuration
            .GetSection($"{LdapMcpServerOptions.SectionName}:{nameof(LdapMcpServerOptions.StreamableHttpCors)}")
            .Get<LdapMcpStreamableHttpCorsOptions>()
            ?? new LdapMcpStreamableHttpCorsOptions();

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policyBuilder =>
                {
                    policyBuilder
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowAnyOrigin();

                    if (corsOptions.AllowAnyOrigin)
                    {
                        policyBuilder.AllowAnyOrigin();
                    }
                    else
                    {
                        policyBuilder.WithOrigins(corsOptions.AllowedOrigins.ToArray());
                    }
                });
            });

            return services
                .Configure<HttpServerTransportOptions>(options =>
                {
                    // It's a good practice to configure the service as stateless when scaling.
                    options.Stateless = true;
                })
                .AddMcpServer()
                .WithHttpTransport()
                .WithToolsFromAssembly()
                .WithResourcesFromAssembly()
                .WithPromptsFromAssembly();
        }
        else
        {
            return services
                .AddMcpServer()
                .WithStdioServerTransport()
                .WithToolsFromAssembly()
                .WithResourcesFromAssembly()
                .WithPromptsFromAssembly();
        }
    }

    private static string ResolveTransportMode()
    {
        var environmentName =
            Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ??
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
            Environments.Production;

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables(prefix: LdapMcpServerOptions.EnvironmentVariablePrefix)
            .Build();

        return configuration[$"{LdapMcpServerOptions.SectionName}:{nameof(LdapMcpServerOptions.TransportMode)}"]
               ?? LdapMcpServerOptions.TransportModeStreamableHttp;
    }
}
