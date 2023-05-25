using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace VideoTranscriber;

public static class ServiceCollectionExtensions
{
    internal const string HttpClientName = "AssemblyAi";

    public static IServiceCollection AddAssemblyAi(this IServiceCollection services)
    {
        var optionsBuilder = services.AddOptions<AssemblyAiOptions>();
        optionsBuilder.BindConfiguration("AssemblyAi")
            .ValidateDataAnnotations();

        services.AddHttpClient(HttpClientName, (services, httpClient) =>
        {
            var options = services.GetRequiredService<IOptions<AssemblyAiOptions>>().Value;

            var baseUrl = options.BaseUrl;
            if (!baseUrl.EndsWith('/')) baseUrl += '/';
            httpClient.BaseAddress = new Uri(baseUrl);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(options.ApiKey);
        })
            .AddTypedClient<MediaUploader>()
            .AddTypedClient<MediaTranscriber>();


        return services;
    }
}