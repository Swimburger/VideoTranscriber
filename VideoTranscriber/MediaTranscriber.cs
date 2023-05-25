using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace VideoTranscriber;

public class MediaTranscriber
{
    private readonly HttpClient httpClient;

    public MediaTranscriber(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<string> TranscribeAsync(string audioUrl, string languageCode)
    {
        var data = new { 
            audio_url = audioUrl, 
            language_code = languageCode 
        };
        var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

        using var response = await httpClient.PostAsync("transcript", content);
        if(!response.IsSuccessStatusCode)
        {
            throw new Exception($"""
            Error: {response.StatusCode} - {response.ReasonPhrase}
            Content: {await response.Content.ReadAsStringAsync()}
            """);
        }
        
        var responseJson = await response.Content.ReadFromJsonAsync<JsonDocument>();

        var transcriptId = responseJson.RootElement.GetProperty("id").GetString();
        return transcriptId;
    }

    public async Task<string> WaitForTranscriptAsync(string transcriptId, TimeSpan pollingInterval)
    {
        var pollingEndpoint = $"transcript/{transcriptId}";
        while (true)
        {
            var pollingResponse = await httpClient.GetAsync(pollingEndpoint);
            var pollingJsonDocument = await pollingResponse.Content.ReadFromJsonAsync<JsonDocument>();
            var pollingJsonObject = pollingJsonDocument.RootElement;

            var status = pollingJsonObject.GetProperty("status").GetString();
            switch (status)
            {
                case "processing":
                    await Task.Delay(pollingInterval);
                    break;
                case "completed":
                    return pollingJsonObject.GetProperty("text").GetString();
                case "error":
                    var error = pollingJsonObject.GetProperty("error").GetString();
                    throw new Exception($"Transcription failed: {error}");
                default:
                    throw new Exception("This code should not be reachable.");
            }
        }
    }

    public async Task<Stream> GetFormattedTranscriptAsStream(string transcriptId, TranscriptFormat format)
    {
        var formatString = format.GetString();
        string url = $"transcript/{transcriptId}/{formatString}";
        HttpResponseMessage response = await httpClient.GetAsync(url);
        return await response.Content.ReadAsStreamAsync();
    }
}