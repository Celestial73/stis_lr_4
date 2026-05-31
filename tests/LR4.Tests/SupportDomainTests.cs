using LR4.Core.Events;

namespace LR4.Tests;

public class SupportDomainTests
{
    private static RuleSet CreateSupportRules()
    {
        var set = new RuleSet();
        set.SetRules(new[]
        {
            new RuleDefinition
            {
                Name = "ClientMessageToExpert",
                WindowSize = 1,
                PatternType = RulePatternType.LastEventEquals,
                PatternValue = "ClientMessage",
                ReactionText = "Направить эксперту",
                Priority = 100
            },
            new RuleDefinition
            {
                Name = "TicketCreatedToEngineer",
                WindowSize = 1,
                PatternType = RulePatternType.LastEventEquals,
                PatternValue = "TicketCreated",
                ReactionText = "Направить инженеру",
                Priority = 100
            },
            new RuleDefinition
            {
                Name = "ExpertReplyToClient",
                WindowSize = 1,
                PatternType = RulePatternType.LastEventEquals,
                PatternValue = "ExpertReply",
                ReactionText = "Направить клиенту",
                Priority = 100
            },
            new RuleDefinition
            {
                Name = "BreakdownCreateTicket",
                WindowSize = 1,
                PatternType = RulePatternType.LastEventEquals,
                PatternValue = "Breakdown",
                ReactionText = "Создать обращение",
                Priority = 100
            }
        });
        return set;
    }

    [Theory]
    [InlineData("ClientMessage", "Направить эксперту")]
    [InlineData("TicketCreated", "Направить инженеру")]
    [InlineData("ExpertReply", "Направить клиенту")]
    [InlineData("Breakdown", "Создать обращение")]
    public void CoreSupportEvents_ProduceExpectedReactions(string eventType, string expectedReaction)
    {
        var engine = new EventRuleEngine(CreateSupportRules());
        var reaction = engine.ProcessEvent(new EventRecord(eventType, null, DateTime.UtcNow));
        Assert.Equal(expectedReaction, reaction?.Text);
    }
}
