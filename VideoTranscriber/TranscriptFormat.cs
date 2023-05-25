namespace VideoTranscriber;

public enum TranscriptFormat
{
    Srt,
    Vtt
}

public static class TranscriptFormatExtensions
{
    private static readonly Dictionary<TranscriptFormat, string> TranscriptFormatMap = new Dictionary<TranscriptFormat, string>
    {
        {TranscriptFormat.Srt, "srt" },
        {TranscriptFormat.Vtt, "vtt" }
    };

    public static string GetString(this TranscriptFormat format)
        => TranscriptFormatMap[format];
}