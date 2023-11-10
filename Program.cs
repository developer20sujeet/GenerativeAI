using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Microsoft.Extensions.Options;
using GenAI;
using GenAI.ChatCompletionsEndPoint;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((hostContext, services) =>
    {
        // Bind the OpenAIOptions class to the OpenAI section of the appsettings.json
        services.Configure<OpenAIOptions>(hostContext.Configuration.GetSection("OpenAI"));

        // Register the OpenAIService with HttpClient configured
        services.AddHttpClient<OpenAIChatCompletionService>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<OpenAIOptions>>().Value;
            client.BaseAddress = new Uri(options.Endpoint);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {options.ApiKey}");
        });

        // Add other services as needed
    })
    .Build();

await GetDataFromOpenAIForChat(host);

await host.RunAsync();

static async Task GetDataFromOpenAIForChat(IHost host)
{
    // Get the service and do something with it
    var openAIChatCompletionService = host.Services.GetRequiredService<OpenAIChatCompletionService>();
    var chatCompletionResponse = await openAIChatCompletionService.ChatCompletionAsync();
    Console.WriteLine(chatCompletionResponse.Choices[0].Message.Content);

    // ... Use openAIService to do something ...
}