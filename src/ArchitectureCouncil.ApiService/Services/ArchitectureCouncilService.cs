using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;

namespace ArchitectureCouncil.ApiService.Services;

public class ArchitectureCouncilService(
    [FromKeyedServices("CloudArchitectAgent")] AIAgent cloudArchitectAgent,
    [FromKeyedServices("SoftwareArchitectAgent")] AIAgent softwareArchitectAgent,
    [FromKeyedServices("LeadArchitectAgent")] AIAgent leadArchitectAgent
    )
{
    public async IAsyncEnumerable<string> DebateStreamingAsync(
        string context,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var workflow = AgentWorkflowBuilder
            .CreateGroupChatBuilderWith(agents =>
                new RoundRobinGroupChatManager(agents)
                {
                    MaximumIterationCount = 3  // One turn per agent (Cloud, Software, Lead)
                })
            .AddParticipants(cloudArchitectAgent, softwareArchitectAgent, leadArchitectAgent)
            .Build();

        AIAgent workflowAgent = workflow.AsAIAgent();

        await foreach (var update in workflowAgent.RunStreamingAsync(context, cancellationToken: cancellationToken))
        {
            if (!string.IsNullOrEmpty(update.Text))
            {
                yield return update.Text;
            }
        }

    }
}
