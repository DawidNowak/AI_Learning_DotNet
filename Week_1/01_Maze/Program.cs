using System.ClientModel;
using System.Text.Json;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;

// Check key
Console.WriteLine("\nCHECKING OPENROUTER API KEY\n");

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json")
    .AddJsonFile("appsettings.local.json", optional: true)
    .Build();

var openrouterApiKey = config["Settings:OpenRouterApiKey"];

if (string.IsNullOrEmpty(openrouterApiKey))
{
    Console.WriteLine("No OpenRouter API key found. Please set the 'OpenRouterApiKey' in appsettings.local.json.");
    return;
}
else
{
    Console.WriteLine($"OpenRouter API key exists and begins {openrouterApiKey[..9]}\n");
}

// Connect to OpenRouter

const string OPENROUTER_BASE_URL = "https://openrouter.ai/api/v1";
const string MODEL = "openrouter/free";

OpenAIClient client = new(new ApiKeyCredential(openrouterApiKey), new OpenAIClientOptions
{
    Endpoint = new Uri(OPENROUTER_BASE_URL)
});

var chatClient = client.GetChatClient(MODEL);

var message = new UserChatMessage("Hello! How are you today?");
var completion = await chatClient.CompleteChatAsync(message);

Console.WriteLine($"\nResponse from OpenRouter: {completion.Value.Content[0].Text}");

// Part 1 of the harness: the tool

const string MAZE_URL = "https://maze.edwarddonner.com";
var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };

async Task<string> ReadWebPage(string url)
{
    Console.WriteLine($"\nReading web page: {url}");
    if (!url.StartsWith(MAZE_URL))
    {
        return $"Sorry, this tool can only read pages in the maze. Please ask for a URL that starts with {MAZE_URL}";
    }

    var html = await httpClient.GetStringAsync(url);
    var page = new HtmlDocument();
    page.LoadHtml(html);

    var body = page.DocumentNode.SelectSingleNode("//body") ?? page.DocumentNode;
    var lines = body
        .DescendantsAndSelf()
        .OfType<HtmlTextNode>()
        .Where(node => node.ParentNode?.Name is not ("script" or "style"))
        .Select(node => HtmlEntity.DeEntitize(node.Text).Trim())
        .Where(text => text.Length > 0);

    return string.Join("\n", lines);
}

Console.WriteLine("\nREADING WEB PAGES");
Console.WriteLine($"\n{await ReadWebPage("https://maze.edwarddonner.com/")}");

Console.WriteLine($"\n{await ReadWebPage("https://nowakdawid.pl/")}");

// The tool for a ChatClient

var readWebPageTool = ChatTool.CreateFunctionTool(
    functionName: "read_web_page",
    functionDescription: "Read a web page and return the text on the page",
    functionParameters: BinaryData.FromString("""
        {
            "type": "object",
            "properties": {
                "url": { "type": "string", "description": "The full URL of the web page to request" }
            },
            "required": ["url"]
        }
        """));

var chatOptions = new ChatCompletionOptions
{
    Tools = { readWebPageTool }
};

// Handle tool call

async Task<ToolChatMessage> HandleToolCall(ChatToolCall toolCall)
{
    using var arguments = JsonDocument.Parse(toolCall.FunctionArguments.ToString());
    var url = arguments.RootElement.GetProperty("url").GetString() ?? "";
    var result = await ReadWebPage(url);
    return new ToolChatMessage(toolCall.Id, result);
}

// Using the tool once
Console.WriteLine("\nUSING THE TOOL ONCE");

List<ChatMessage> messages =
[
    new UserChatMessage("Please read https://maze.edwarddonner.com and tell me what it says. Just describe the page for now, without going into the maze.")
];

ChatCompletion response = await chatClient.CompleteChatAsync(messages, chatOptions);

Console.WriteLine($"\nAnswered by {response.Model}");
Console.WriteLine($"Finish reason: {response.FinishReason}");
foreach (var toolCall in response.ToolCalls)
{
    Console.WriteLine($"Tool call {toolCall.Id}: {toolCall.FunctionName}({toolCall.FunctionArguments})");
}

messages.Add(new AssistantChatMessage(response));
foreach (var toolCall in response.ToolCalls)
{
    messages.Add(await HandleToolCall(toolCall));
}

response = await chatClient.CompleteChatAsync(messages, chatOptions);

Console.WriteLine($"\nAnswered by {response.Model}");
Console.WriteLine(response.Content[0].Text);

// Part 2 of the harness: the loop

const string SYSTEM_PROMPT = """
You are an agent that completes tasks by using your tools, one step at a time.
Put any map or diagram in a formatted way so that it lines up. Do not use emojis.
""";

async Task RunAgent(string task, int maxSteps = 25)
{
    List<ChatMessage> messages =
    [
        new SystemChatMessage(SYSTEM_PROMPT),
        new UserChatMessage(task)
    ];

    for (int i = 0; i < maxSteps; i++)
    {
        ChatCompletion response = await chatClient.CompleteChatAsync(messages, chatOptions);
        messages.Add(new AssistantChatMessage(response));
        Console.WriteLine($"Step {i + 1}: Answered by {response.Model}");
        
        if (response.FinishReason == ChatFinishReason.ToolCalls)
        {
            foreach (var toolCall in response.ToolCalls)
            {
                messages.Add(await HandleToolCall(toolCall));
            }
        }
        else if (response.FinishReason == ChatFinishReason.Stop)
        {
            Console.WriteLine($"\nThe agent completed the task successfully.");
            Console.WriteLine($"\nFinal answer: {response.Content[0].Text}");
            return;
        }        
    }

    Console.WriteLine($"\nThe agent ran out of steps.");
}

// Into the maze
Console.WriteLine("\nINTO THE MAZE");

await RunAgent("Have a look at https://maze.edwarddonner.com and do what it says.");