using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAI.GPT3.Extensions;
using OpenAI.GPT3.Interfaces;
using OpenAI.GPT3.Managers;
using VideoTranscriber;


using var host = Host.CreateDefaultBuilder(args)
    .UseEnvironment("Development")
    .ConfigureServices((hostContext, services) =>
    {
        services.AddAssemblyAi();
        services.AddOpenAIService();
        services.AddHttpClient<IOpenAIService, OpenAIService>((httpClient) => httpClient.Timeout = TimeSpan.FromMinutes(5));
        services.AddTransient<TranscriptTranslator>();
    })
    .Build();

var services = host.Services;
var logger = services.GetRequiredService<ILogger<Program>>();

const string mediaPath = "C:\\Users\\niels\\OneDrive\\Desktop\\Dutch Sample.mp4";

var mediaUploader = services.GetRequiredService<MediaUploader>();
var uploadMediaUrl = await mediaUploader.UploadFromPathAsync(mediaPath);
Console.WriteLine($"Uploaded Media URL: {uploadMediaUrl}");

var mediaTranscriber = services.GetRequiredService<MediaTranscriber>();
    
var transcriptionId = await mediaTranscriber.TranscribeAsync(
    uploadMediaUrl,
    languageCode: "nl"
);
Console.WriteLine($"Transcript ID: {transcriptionId}");

var transcript = await mediaTranscriber.WaitForTranscriptAsync(transcriptionId, TimeSpan.FromSeconds(1));
Console.WriteLine($"Transcript: {transcript}");

using var transcriptStream = await mediaTranscriber.GetFormattedTranscriptAsStream(transcriptionId, TranscriptFormat.Srt);
using (var fileStream = File.OpenWrite("./transcript.srt"))
{
    await transcriptStream.CopyToAsync(fileStream);
}

var transcriptTranslator = services.GetRequiredService<TranscriptTranslator>();
await transcriptTranslator.TranslateTranscriptAsync("./transcript.srt", "./transcript.en.srt");