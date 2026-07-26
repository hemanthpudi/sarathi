using Sarathi.API.Application.Services;

namespace Sarathi.API.Tests;

public class ProjectManagerAiChatPolicyTests
{
    [Fact]
    public void IsRelevantQuestion_ReturnsFalse_ForIrrelevantTopic()
    {
        var relevant = ProjectManagerAiChatPolicy.IsRelevantQuestion("What is the weather in London today?", "FinBank Portal");

        Assert.False(relevant);
    }

    [Fact]
    public void BuildScopeResponse_ContainsAssignedProjectGuardrail()
    {
        var response = ProjectManagerAiChatPolicy.BuildScopeResponse("FinBank Portal");

        Assert.Contains("FinBank Portal", response);
        Assert.Contains("assigned project", response, StringComparison.OrdinalIgnoreCase);
    }
}
