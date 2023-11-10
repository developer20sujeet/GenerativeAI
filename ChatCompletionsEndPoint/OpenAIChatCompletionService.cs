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
                    /*
                     * 
                       -Role "system": 
                                - This is typically used for sending instructions or setting the context for the AI model.
                                - It might be used to specify the behavior, persona, or rules that the AI should follow while generating responses. 
                                - This is equal to custome instrcution in chatGPT on the OpenAI website
                           

                        - role = "user"                             
                             - This is the prompt that the AI will respond to, generating a completion that is a poem about recursion
                    */

                    new { role = "system", content = "Your are Data structure and Algorithms expert and you will help with Tricks and Tips of DSA" },
                    new { role = "user", content = "Please give me interview note for Linked List DSA " }
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


