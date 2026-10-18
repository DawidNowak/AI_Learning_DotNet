# 01_Maze

An AI agent that finds its way out of Ed Donner's maze at [maze.edwarddonner.com](https://maze.edwarddonner.com). Every room in the maze is a web page. The agent reads the pages with a `read_web_page` tool, follows the exits one step at a time, and brings back the message waiting at the way out.

This is a .NET port of the course's Python exercise. The model is called through [OpenRouter](https://openrouter.ai) using the official OpenAI .NET client.

## Prerequisites

- .NET 10 SDK
- An OpenRouter API key, stored in `appsettings.local.json` next to `appsettings.json`. This file is git-ignored, so the key never gets committed:

  ```json
  {
    "Settings": {
      "OpenRouterApiKey": "sk-or-..."
    }
  }
  ```

The model is set to `openrouter/free`, which routes each request to one of OpenRouter's free models.

## How to Run

The same code can be run in two ways:

| Way | File | Best for |
|-----|------|----------|
| Console app | `01_Maze.csproj` | Running the whole thing from start to finish |
| Jupyter notebook | `Maze.ipynb` | Running it step by step and inspecting results along the way |

### Console app (full run)

From the `Week_1/01_Maze` folder:

```bash
dotnet run
```

You can also run `01_Maze.csproj` from Visual Studio, Rider or VS Code. The app runs every step in order: it checks the API key, sends a hello test message, reads a couple of web pages, makes a single tool call, and then lets the agent loose in the maze. NuGet packages come from the project file, and both appsettings files are copied to the build output.

### Jupyter notebook (interactive)

1. Open `Maze.ipynb` in VS Code with the Jupyter extension installed.
2. Select the **.NET (C#)** kernel. If it's not on the list, install it first, as described in [Installing the .NET (C#) Jupyter Kernel](../../README.md#installing-the-net-c-jupyter-kernel) in the main README.
3. Run the cells one by one, starting from the first.

Variables and functions stay in memory between cells, like in a Python notebook. The first cell loads the NuGet packages with `#r "nuget: ..."` directives, and the settings are read from the notebook's folder.

## Project Files

| File | Description |
|------|-------------|
| `Program.cs` | Console app: config, OpenRouter client, `read_web_page` tool and agent loop |
| `Maze.ipynb` | The same code split into notebook cells |
| `01_Maze.csproj` | Project file with package references (OpenAI, HtmlAgilityPack, Microsoft.Extensions.Configuration.Json) |
| `appsettings.json` | Settings template, committed with an empty key |
| `appsettings.local.json` | Your local settings with the real API key (git-ignored) |
