# AI Learning (.NET)

A collection of hands-on .NET projects covering generative AI, large language models, and agentic AI architecture. I'm switching from Python to .NET, so the exercises here are implemented in C#.

![Hello](./ai_learning.png)

## Source Course

The projects follow Ed Donner's course [AI FDE: The Forward Deployed Engineer & Agentic AI Architect](https://www.udemy.com/course/ai-fde-forward-deployed-engineer-agentic-ai-architect/). The course material and examples are in Python; the implementations in this repository are my own .NET ports.

## Repository Structure

Projects are grouped by course week:

| Path | Description |
|------|-------------|
| `Week_1/01_Maze` | Week 1, exercise 1: Maze |

## Installing the .NET (C#) Jupyter Kernel

Some projects include Jupyter notebooks (`.ipynb`) that run on the **.NET (C#)** kernel. To make the kernel available in VS Code's Jupyter extension on Windows:

1. Install the .NET Interactive tool, if you don't have it yet:

   ```powershell
   dotnet tool install -g Microsoft.dotnet-interactive
   ```

2. Create the Jupyter kernels folder:

   ```powershell
   New-Item -ItemType Directory -Force -Path "$env:APPDATA\jupyter\kernels"
   ```

3. Register the .NET kernels with Jupyter:

   ```powershell
   dotnet interactive jupyter install
   ```

4. Restart VS Code, so the Jupyter extension picks up the new kernel.

After that, open a notebook in VS Code and pick the **.NET (C#)** kernel.

## Tech Stack

- .NET / C#
