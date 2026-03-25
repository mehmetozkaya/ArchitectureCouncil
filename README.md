# 🏛️ Architecture Council — Multi-Agent Workflows with .NET

A **.NET 10** sample application demonstrating how to orchestrate **multi-agent AI workflows** using the **Microsoft Agent Framework** and **.NET Aspire**. Three AI-powered architect agents debate a given architectural topic in real time and produce a final **Architecture Decision Record (ADR)**.

> 📖 **Full article:** [Orchestrating Multi-Agent Workflows with .NET and Microsoft Agent Framework](https://mehmetozkaya.medium.com/orchestrating-multi-agent-workflows-with-net-and-microsoft-agent-framework-79585d6b0260)

![.NET](https://img.shields.io/badge/.NET-10-512BD4) ![Aspire](https://img.shields.io/badge/.NET%20Aspire-AppHost-blueviolet) ![Azure OpenAI](https://img.shields.io/badge/Azure-OpenAI-0078D4) ![Microsoft Agents](https://img.shields.io/badge/Microsoft-Agent%20Framework-green)

---

## Overview

The application simulates an **Architecture Council** where three specialised AI agents participate in a round-robin group chat:

| Agent | Role | Perspective |
|---|---|---|
| ☁️ **Cloud-Native Advocate** | Alex Chen — Senior Cloud Architect | Prefers Azure-managed PaaS services (Azure Service Bus, Azure SQL, Azure Cache for Redis). Argues for high availability, built-in monitoring, and SLA-backed managed services. |
| 🐧 **Open-Source Purist** | Jordan Lee — Senior Software Architect | Prefers self-hosted, cloud-agnostic tools (RabbitMQ, PostgreSQL, Redis OSS). Argues for vendor independence, cost control, and portability. |
| ⚖️ **Lead Architect** | Sam Rivera — Principal Architect | Observes the debate, synthesises trade-offs, and delivers a final ruling as an ADR. |

The agents are orchestrated via a **RoundRobinGroupChatManager** from the Microsoft Agent Framework's `AgentWorkflow` API, each agent taking one turn before the Lead Architect renders the final decision.

---

## Architecture

```
┌──────────────────────────────────────────────┐
│           ArchitectureCouncil.AppHost        │  .NET Aspire orchestrator
│              (Aspire App Host)               │
└──────┬───────────────────┬───────────────────┘
       │                   │
       ▼                   ▼
┌──────────────┐   ┌──────────────────────────┐
│  Web Frontend│   │      API Service         │
│  (Blazor SSR)│──▶│  POST /debate            │
│              │   │                          │
│  Razor Pages │   │  ┌────────────────────┐  │
│  + Streaming │   │  │ ArchitectureCouncil│  │
│              │   │  │     Service        │  │
└──────────────┘   │  └────────┬───────────┘  │
                   │           │              │
                   │  ┌────────▼───────────┐  │
                   │  │  AgentWorkflow     │  │
                   │  │  (RoundRobin Group)│  │
                   │  │                    │  │
                   │  │  ☁️ CloudArchitect  │  │
                   │  │  🐧 SoftwareArchitect│ │
                   │  │  ⚖️ LeadArchitect   │  │
                   │  └────────┬───────────┘    │
                   │           │                │
                   │           ▼                │
                   │     Azure OpenAI           │
                   └────────────────────────────┘
```

---

## Tech Stack

- **.NET 10** — Target framework
- **.NET Aspire** — Service orchestration and service defaults
- **Microsoft Agent Framework** (`Microsoft.Agents.AI.Hosting`, `Microsoft.Agents.AI.OpenAI`) — Multi-agent workflows with `AIAgent`, `AgentWorkflow`, and `RoundRobinGroupChatManager`
- **Azure OpenAI** (`Azure.AI.OpenAI`) — LLM backend for agent reasoning
- **Blazor Server** — Interactive web frontend with real-time streaming
- **Microsoft.Extensions.AI** — Unified `IChatClient` abstraction

---

## Project Structure

```
src/
├── ArchitectureCouncil.AppHost/          # .NET Aspire App Host (orchestrator)
│   └── AppHost.cs
├── ArchitectureCouncil.ApiService/       # Backend API
│   ├── Program.cs                        # Agent definitions & tool functions
│   ├── Endpoints/
│   │   └── ArchitectureCouncilEndpoints.cs  # POST /debate (streaming JSON)
│   └── Services/
│       └── ArchitectureCouncilService.cs    # AgentWorkflow orchestration
├── ArchitectureCouncil.Web/              # Blazor Server frontend
│   ├── Program.cs
│   ├── ApiClients/
│   │   └── ArchitectureCouncilApiClient.cs  # Streaming HTTP client
│   └── Components/Pages/
│       └── CouncilDebate.razor              # Debate UI page
└── ArchitectureCouncil.ServiceDefaults/  # Shared Aspire service defaults
```

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [.NET Aspire workload](https://learn.microsoft.com/dotnet/aspire/fundamentals/setup-tooling)
- An **Azure OpenAI** resource with a deployed model (e.g., `gpt-4o-mini`)
- **Azure CLI** authenticated (`az login`) — the app uses `AzureCliCredential`

---

## Getting Started

1. **Clone the repository**
   ```bash
   git clone https://github.com/mehmetozkaya/ArchitectureCouncil.git
   cd ArchitectureCouncil
   ```

2. **Set environment variables**
   ```bash
   export AZURE_OPENAI_ENDPOINT="https://<your-resource>.openai.azure.com/"
   export AZURE_OPENAI_DEPLOYMENT_NAME="gpt-4o-mini"
   ```

3. **Run the Aspire App Host**
   ```bash
   dotnet run --project src/ArchitectureCouncil.AppHost
   ```

4. Open the **Aspire dashboard** (displayed in the terminal output) and navigate to the **Web Frontend** URL.

5. Go to the **Architecture Council** page, enter a debate topic, and click **Start Debate** to watch the agents argue in real time.

---

## How It Works

1. **Agent Registration** — Three `AIAgent` instances are registered in `Program.cs` using `builder.AddAIAgent(...)`, each with distinct system instructions and tool functions.
2. **Workflow Orchestration** — `ArchitectureCouncilService` builds an `AgentWorkflow` with a `RoundRobinGroupChatManager` (max 3 iterations — one per agent).
3. **Streaming Response** — The `/debate` endpoint streams JSON chunks as the agents generate responses. The Blazor frontend consumes these via `IAsyncEnumerable<string>` for a real-time typing effect.
4. **ADR Generation** — The Lead Architect agent uses the `FormatAdr` tool to produce a structured Architecture Decision Record as the final output.

---

## License

This project is intended as a learning resource accompanying the article above.

---

## Author

**Mehmet Özkaya**
- 📝 [Medium](https://mehmetozkaya.medium.com/)
- 🐙 [GitHub](https://github.com/mehmetozkaya)
