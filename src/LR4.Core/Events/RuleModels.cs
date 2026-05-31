using System.Text.Json;
using System.Text.Json.Serialization;

namespace LR4.Core.Events;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RulePatternType
{
    LastEventEquals,
    WindowSequenceEquals,
    WindowContains,
    Always
}

public sealed class RuleDefinition
{
    public string Name { get; set; } = string.Empty;
    public int WindowSize { get; set; } = 1;
    public RulePatternType PatternType { get; set; } = RulePatternType.LastEventEquals;
    public string PatternValue { get; set; } = string.Empty;
    public string ReactionText { get; set; } = string.Empty;
    public int Priority { get; set; }
}

public sealed class RuleSet
{
    private readonly List<RuleDefinition> _rules = new();

    public IReadOnlyList<RuleDefinition> Rules => _rules;

    public void SetRules(IEnumerable<RuleDefinition> rules)
    {
        _rules.Clear();
        _rules.AddRange(rules.OrderByDescending(r => r.Priority));
        Validate();
    }

    public void Add(RuleDefinition rule)
    {
        _rules.Add(rule);
        _rules.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        ValidateRule(rule);
    }

    public void RemoveAt(int index) => _rules.RemoveAt(index);

    public int MaxWindowSize => _rules.Count == 0 ? 1 : Math.Max(1, _rules.Max(r => r.WindowSize));

    public void Validate()
    {
        foreach (var rule in _rules)
            ValidateRule(rule);
    }

    private static void ValidateRule(RuleDefinition rule)
    {
        if (string.IsNullOrWhiteSpace(rule.Name))
            throw new ArgumentException("Rule name is required.");
        if (rule.WindowSize < 1)
            throw new ArgumentException("WindowSize must be >= 1.");
    }

    public bool TryMatch(IReadOnlyList<EventRecord> window, out RuleDefinition? matched)
    {
        foreach (var rule in _rules)
        {
            if (Matches(rule, window))
            {
                matched = rule;
                return true;
            }
        }

        matched = null;
        return false;
    }

    private static bool Matches(RuleDefinition rule, IReadOnlyList<EventRecord> window)
    {
        if (window.Count == 0)
            return false;

        var slice = window.TakeLast(rule.WindowSize).ToList();
        if (slice.Count < rule.WindowSize && rule.PatternType != RulePatternType.Always)
            return false;

        return rule.PatternType switch
        {
            RulePatternType.LastEventEquals => slice[^1].Type.Equals(rule.PatternValue, StringComparison.OrdinalIgnoreCase),
            RulePatternType.WindowSequenceEquals => string.Join(",", slice.Select(e => e.Type))
                .Equals(rule.PatternValue, StringComparison.OrdinalIgnoreCase),
            RulePatternType.WindowContains => slice.Any(e => e.Type.Equals(rule.PatternValue, StringComparison.OrdinalIgnoreCase)),
            RulePatternType.Always => true,
            _ => false
        };
    }

    public static RuleSet LoadFromJson(string path)
    {
        var json = File.ReadAllText(path);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new JsonStringEnumConverter());
        var rules = JsonSerializer.Deserialize<List<RuleDefinition>>(json, options)
            ?? throw new InvalidOperationException("Invalid rules file.");
        var set = new RuleSet();
        set.SetRules(rules);
        return set;
    }

    public void SaveToJson(string path)
    {
        var json = JsonSerializer.Serialize(_rules, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }
}
