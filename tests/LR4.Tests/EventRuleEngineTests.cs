using LR4.Core.Events;
using LR4.Core.Lazy;
using LR4.Core.Sequences;
using LR4.Core.Streams;

namespace LR4.Tests;

public class EventRuleEngineTests
{
    [Fact]
    public void LastEventRule_TriggersReaction()
    {
        var rules = new RuleSet();
        rules.SetRules(new[]
        {
            new RuleDefinition
            {
                Name = "Alarm",
                WindowSize = 1,
                PatternType = RulePatternType.LastEventEquals,
                PatternValue = "Alarm",
                ReactionText = "Siren",
                Priority = 10
            }
        });

        var engine = new EventRuleEngine(rules);
        var reaction = engine.ProcessEvent(new EventRecord("Alarm", null, DateTime.UtcNow));
        Assert.Equal("Siren", reaction?.Text);
        Assert.Equal("Alarm", reaction?.RuleName);
    }

    [Fact]
    public void WindowSequenceRule_MatchesTwoEvents()
    {
        var rules = new RuleSet();
        rules.SetRules(new[]
        {
            new RuleDefinition
            {
                Name = "Seq",
                WindowSize = 2,
                PatternType = RulePatternType.WindowSequenceEquals,
                PatternValue = "Error,Alarm",
                ReactionText = "Escalate",
                Priority = 10
            }
        });

        var engine = new EventRuleEngine(rules);
        engine.ProcessEvent(new EventRecord("Error", null, DateTime.UtcNow));
        var reaction = engine.ProcessEvent(new EventRecord("Alarm", null, DateTime.UtcNow));
        Assert.Equal("Escalate", reaction?.Text);
    }

    [Fact]
    public void HigherPriorityRule_Wins()
    {
        var rules = new RuleSet();
        rules.SetRules(new[]
        {
            new RuleDefinition
            {
                Name = "Low",
                WindowSize = 1,
                PatternType = RulePatternType.Always,
                ReactionText = "Low",
                Priority = 1
            },
            new RuleDefinition
            {
                Name = "High",
                WindowSize = 1,
                PatternType = RulePatternType.LastEventEquals,
                PatternValue = "Tick",
                ReactionText = "High",
                Priority = 100
            }
        });

        var engine = new EventRuleEngine(rules);
        var reaction = engine.ProcessEvent(new EventRecord("Tick", null, DateTime.UtcNow));
        Assert.Equal("High", reaction?.Text);
    }

    [Fact]
    public void ProcessStream_ReadsFromSequenceStream()
    {
        var rules = new RuleSet();
        rules.SetRules(new[]
        {
            new RuleDefinition
            {
                Name = "Always",
                WindowSize = 1,
                PatternType = RulePatternType.Always,
                ReactionText = "OK",
                Priority = 1
            }
        });

        var seq = new ArraySequence<EventRecord>();
        seq = (ArraySequence<EventRecord>)seq.Append(new EventRecord("A", null, DateTime.UtcNow));
        seq = (ArraySequence<EventRecord>)seq.Append(new EventRecord("B", null, DateTime.UtcNow));

        var engine = new EventRuleEngine(rules);
        var reactions = engine.ProcessStream(new SequenceReadOnlyStream<EventRecord>(seq));
        Assert.Equal(2, reactions.Count);
    }
}
