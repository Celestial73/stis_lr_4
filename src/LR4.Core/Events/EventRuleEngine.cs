using LR4.Core.Lazy;
using LR4.Core.Streams;

namespace LR4.Core.Events;

public sealed class EventRuleEngine
{
    private readonly RuleSet _rules;
    private readonly Queue<EventRecord> _window = new();
    private int _maxWindow;

    public EventRuleEngine(RuleSet rules)
    {
        _rules = rules;
        ResetWindow();
    }

    public RuleSet Rules => _rules;

    public void ResetWindow()
    {
        _window.Clear();
        _maxWindow = _rules.MaxWindowSize;
    }

    public ReactionRecord? ProcessEvent(EventRecord ev)
    {
        _window.Enqueue(ev);
        while (_window.Count > _maxWindow)
            _window.Dequeue();

        var snapshot = _window.ToList();
        if (!_rules.TryMatch(snapshot, out var rule) || rule is null)
            return new ReactionRecord("(no reaction)", null, DateTime.UtcNow);

        return new ReactionRecord(rule.ReactionText, rule.Name, DateTime.UtcNow);
    }

    public IReadOnlyList<ReactionRecord> ProcessStream(ReadOnlyStream<EventRecord> input)
    {
        input.Open();
        var reactions = new List<ReactionRecord>();
        try
        {
            while (!input.IsEndOfStream())
            {
                var ev = input.Read();
                var reaction = ProcessEvent(ev);
                if (reaction is not null)
                    reactions.Add(reaction);
            }
        }
        finally
        {
            input.Close();
        }

        return reactions;
    }

    public static LazySequence<EventRecord> CreateCyclicEventSource(string[] types, TimeSpan step)
    {
        var index = 0;
        var start = DateTime.UtcNow;
        return new LazySequence<EventRecord>((_) =>
        {
            var type = types[index % types.Length];
            var ts = start.AddMilliseconds(step.TotalMilliseconds * index);
            index++;
            return new EventRecord(type, null, ts);
        }, Array.Empty<EventRecord>());
    }
}
