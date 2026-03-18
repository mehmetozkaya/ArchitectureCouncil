using ArchitectureCouncil.ApiService.Services;
using ArchitectureCouncil.ApiService.Endpoints;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.AI;
using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddServiceDefaults();

builder.Services.AddProblemDetails();

builder.Services.AddOpenApi();

// 1. Define the variables we extracted from Microsoft Foundry
var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-5-mini";

// 2. Instantiate the universal chat client with OpenTelemetry GenAI instrumentation
IChatClient chatClient = new AzureOpenAIClient(
        new Uri(endpoint),
        new AzureCliCredential())
    .GetChatClient(deploymentName)
    .AsIChatClient()
    .AsBuilder()
    .UseOpenTelemetry(configure: c => c.EnableSensitiveData = true)
    .Build();
builder.Services.AddSingleton(chatClient);

// 3. Define the Agents Anatomy
builder.AddAIAgent(
    name: "CloudArchitectAgent",
    instructions:
        """
        You are the Cloud-Native Advocate on an Architecture Council.
        You strongly prefer Azure-managed PaaS services (e.g., Azure Service Bus, Azure SQL, Azure Cache for Redis).
        Argue for high availability, built-in monitoring, SLA-backed managed services, and reduced operational overhead.
        Keep responses concise — 3-5 sentences per turn. Be direct and opinionated.
        Always call GetCloudArchitectRole to identify yourself before your first argument.
        """,
    chatClient).WithAITools(AIFunctionFactory.Create(GetCloudArchitectRole));

builder.AddAIAgent(
    name: "SoftwareArchitectAgent",
    instructions:
        """
        You are the Open-Source Purist on an Architecture Council.
        You strongly prefer self-hosted, cloud-agnostic tools (e.g., RabbitMQ, PostgreSQL, Redis OSS).
        Argue for vendor independence, cost control, portability, and community-driven innovation.
        Keep responses concise — 3-5 sentences per turn. Be direct and opinionated.
        Always call GetSoftwareArchitectRole to identify yourself before your first argument.
        """,
    chatClient).WithAITools(AIFunctionFactory.Create(GetSoftwareArchitectRole));

builder.AddAIAgent(
    name: "LeadArchitectAgent",
    instructions:
        """
        You are the Principal Architect (Lead) on an Architecture Council.
        You observe the debate between the Cloud-Native Advocate and the Open-Source Purist.
        Synthesize their tradeoffs, make a final ruling, and produce a short Architecture Decision Record (ADR).
        Use the FormatAdr tool to format your final decision.
        Keep the ADR brief: Title, Status (Accepted), Context (2 sentences), Decision (2-3 sentences), Consequences (bullet list, max 4 items).
        """,
    chatClient).WithAITools(AIFunctionFactory.Create(FormatAdr));

// Agent Tools
[Description("Returns the role and name of the Cloud Architect.")]
static string GetCloudArchitectRole() => "Cloud-Native Advocate — Alex Chen, Senior Cloud Architect";

[Description("Returns the role and name of the Software Architect.")]
static string GetSoftwareArchitectRole() => "\nOpen-Source Purist — Jordan Lee, Senior Software Architect";

[Description("Formats the final Architecture Decision Record (ADR).")]
static string FormatAdr(string title, string decision, string consequences)
    => $"# ADR: {title}\n\n**Status**: Accepted\n**Lead Architect**: Sam Rivera, Principal Architect\n\n## Decision\n\n{decision}\n\n## Consequences\n\n{consequences}";

// 4. Register the ADR writer service for dependency injection
builder.Services.AddScoped<ArchitectureCouncilService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapDefaultEndpoints();

app.MapArchitectureCouncilEndpoints();

app.Run();

