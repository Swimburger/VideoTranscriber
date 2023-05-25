using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Contrib.HttpClient;
using System.Net;
using System.Security.Cryptography;

namespace VideoTranscriber.Test;

public class Tests
{
    private const string MediaPath = "C:\\Users\\niels\\OneDrive\\Desktop\\Dutch Sample.mp4";

    [Test]
    public void Extensions_Should_Register_Options()
    {
        var dummyBaseUrl = "https://localhost/dummy/";
        var dummyApiKey = "DummyApiKey";
        var builder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"AssemblyAi:BaseUrl",  dummyBaseUrl},
                {"AssemblyAi:ApiKey", dummyApiKey }
            });
        IConfiguration configuration = builder.Build();

        var services = new ServiceCollection();
        services.AddScoped(_ => configuration);
        services.AddAssemblyAi();

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptionsSnapshot<AssemblyAiOptions>>().Value;

        Assert.Multiple(() =>
        {
            Assert.That(options.BaseUrl, Is.EqualTo(dummyBaseUrl));
            Assert.That(options.ApiKey, Is.EqualTo(dummyApiKey));
        });
    }

    [Test]
    public void Extensions_Should_Register_HttpClient()
    {
        var dummyBaseUrl = "https://localhost/dummy/";
        var dummyApiKey = "DummyApiKey";
        var builder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"AssemblyAi:BaseUrl",  dummyBaseUrl},
                {"AssemblyAi:ApiKey", dummyApiKey }
            });
        IConfiguration configuration = builder.Build();

        var services = new ServiceCollection();
        services.AddScoped(_ => configuration);
        services.AddAssemblyAi();

        var serviceProvider = services.BuildServiceProvider();
        var httpClient = serviceProvider.GetRequiredService<IHttpClientFactory>()
            .CreateClient(ServiceCollectionExtensions.HttpClientName);

        Assert.That(httpClient.BaseAddress, Is.EqualTo(new Uri(dummyBaseUrl)));
    }

    [Test]
    public void Extensions_Should_Register_Services()
    {
        var dummyBaseUrl = "https://localhost/dummy/";
        var dummyApiKey = "DummyApiKey";
        var builder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"AssemblyAi:BaseUrl",  dummyBaseUrl},
                {"AssemblyAi:ApiKey", dummyApiKey }
            });
        IConfiguration configuration = builder.Build();

        var services = new ServiceCollection();
        services.AddScoped(_ => configuration);
        services.AddAssemblyAi();

        var serviceProvider = services.BuildServiceProvider();

        var mediaUploader = serviceProvider.GetService<MediaUploader>();
        Assert.That(mediaUploader, Is.Not.Null);

        var mediaTranscriber = serviceProvider.GetService<MediaTranscriber>();
        Assert.That(mediaTranscriber, Is.Not.Null);
    }

    [Test]
    public async Task MediaUploader_Should_Load_Media()
    {
        using var mediaStream = MediaUploader.ReadFromPath(MediaPath);
        var hashBytes = await MD5.HashDataAsync(mediaStream);
        var hash = BitConverter.ToString(hashBytes).Replace("-", "");
        Assert.That(hash, Is.EqualTo("AC4DD0609AB745A077AC6204E034608E"));
    }

    [Test]
    public async Task MediaUploader_Should_Upload_Media()
    {
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var client = handler.CreateClient();
        client.BaseAddress = new Uri(AssemblyAiOptions.DefaultBaseUrl);

        handler.SetupAnyRequest()
            .ReturnsResponse(HttpStatusCode.NotFound);

        handler.SetupRequest(HttpMethod.Post, new Uri($"{AssemblyAiOptions.DefaultBaseUrl}upload"))
            .ReturnsJsonResponse(new { upload_url = "https://localhost/file" });

        var mediaUploader = new MediaUploader(client);
        var uploadedMediaPath = await mediaUploader.UploadFromPathAsync(MediaPath);

        Assert.That(uploadedMediaPath, Is.EqualTo("https://localhost/file"));
    }
}