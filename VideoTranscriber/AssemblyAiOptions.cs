using System.ComponentModel.DataAnnotations;

namespace VideoTranscriber;

public class AssemblyAiOptions
{
    internal const string DefaultBaseUrl = "https://api.assemblyai.com/v2/";

    public string BaseUrl { get; set; } = DefaultBaseUrl;

    [Required]
    public string ApiKey { get; set; }
}