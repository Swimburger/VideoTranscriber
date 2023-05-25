using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace VideoTranscriber;

public class MediaUploader
{
    private readonly HttpClient httpClient;

    public MediaUploader(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    internal static FileStream ReadFromPath(string path) => File.OpenRead(path);

    public async Task<string> UploadFromPathAsync(string path)
    {
        using var mediaStream = ReadFromPath(path);

        await using var fileStream = File.OpenRead(path);
        using var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        using var response = await httpClient.PostAsync("upload", fileContent);

        if (response.IsSuccessStatusCode == false)
            throw new Exception($"Error: {response.StatusCode} - {response.ReasonPhrase}");

        var jsonDoc = await response.Content.ReadFromJsonAsync<JsonDocument>();
        return jsonDoc.RootElement.GetProperty("upload_url").GetString()
            ?? throw new Exception("upload_url not found in API response body.");
    }
}