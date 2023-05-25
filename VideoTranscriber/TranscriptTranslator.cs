using OpenAI.GPT3.Interfaces;
using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.ObjectModels;

namespace VideoTranscriber;

public class TranscriptTranslator
{
    private readonly IOpenAIService openAiService;

    public TranscriptTranslator(IOpenAIService openAIService)
    {
        openAiService = openAIService;
    }

    public async Task TranslateTranscriptAsync(
        string transcriptPath,
        string translatedTranscriptPath
    )
    {
        var transcriptText = File.ReadAllText(transcriptPath);
        var completionResult = await openAiService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
        {
            Messages = new List<ChatMessage>
            {
                ChatMessage.FromUser($"""
                Translate this transcript from Dutch to English:
                {transcriptText}
                """),
            },
            Model = Models.Gpt_4,
            
        });

        if (!completionResult.Successful)
        {
            throw new Exception();
        }

        File.WriteAllText(translatedTranscriptPath, completionResult.Choices.First().Message.Content);
    }
}