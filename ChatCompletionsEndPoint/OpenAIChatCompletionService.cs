using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace GenAI.ChatCompletionsEndPoint
{

    internal class OpenAIChatCompletionService
    {
        private readonly HttpClient _httpClient;
        private readonly OpenAIOptions _options;

        public OpenAIChatCompletionService(HttpClient httpClient, IOptions<OpenAIOptions> options)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

            // Assuming the HttpClient is already configured with the base address and default headers
        }
        public async Task<ChatCompletionResponse> ChatCompletionAsync()
        {
            var requestBody = new
            {
                model = _options.Model,
                messages = new[]
                {
                    new { role = "system", content = "You are a poetic assistant, skilled in explaining complex programming concepts with creative flair." },
                    new { role = "user", content = "Compose a poem that explains the concept of recursion in programming." }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(_options.Endpoint, content);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var chatCompletionResponse = JsonSerializer.Deserialize<ChatCompletionResponse>(responseBody, options);
                if (chatCompletionResponse == null)
                    throw new InvalidOperationException("Failed to deserialize the response.");

                return chatCompletionResponse;
            }
            catch (HttpRequestException e)
            {
                // Handle HTTP request exceptions
                throw new InvalidOperationException($"Error during the chat completion request: {e.Message}", e);
            }
            catch (JsonException e)
            {
                // Handle JSON serialization/deserialization exceptions
                throw new InvalidOperationException($"Error processing the response: {e.Message}", e);
            }
        }
    }
}


#region Json Response class

public class ChatCompletionResponse
{
    public string Id { get; set; }
    public string Object { get; set; }
    public long Created { get; set; }
    public string Model { get; set; }
    public List<Choice> Choices { get; set; }
    public Usage Usage { get; set; }
}

public class Choice
{
    public int Index { get; set; }
    public Message Message { get; set; }
    public string FinishReason { get; set; }
}

public class Message
{
    public string Role { get; set; }
    public string Content { get; set; }
}

public class Usage
{
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int TotalTokens { get; set; }
}

#endregion