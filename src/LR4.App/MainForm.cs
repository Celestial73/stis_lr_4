using LR4.Core.Events;
using LR4.Core.Streams;

namespace LR4.App;

public partial class MainForm : Form
{
    private static readonly string[] EventTypes =
    {
        "ClientMessage",
        "Breakdown",
        "TicketCreated",
        "ExpertReply"
    };

    private readonly EventJsonSerializer _serializer = new();
    private RuleSet _ruleSet = new();
    private EventRuleEngine _engine;
    private ReadOnlyStream<EventRecord>? _demoStream;
    private readonly List<EventRecord> _events = new();
    private readonly List<ReactionRecord> _reactions = new();

    public MainForm()
    {
        InitializeComponent();
        cmbEventType.Items.AddRange(EventTypes);
        cmbEventType.SelectedIndex = 0;

        _engine = new EventRuleEngine(_ruleSet);
        LoadDefaults();
        Shown += (_, _) => ApplyLayout();
        Resize += (_, _) => ApplyLayout();
    }

    private void ApplyLayout()
    {
        SetSplitDistance(splitGrids, splitGrids.Width / 2, panel1Min: 120, panel2Min: 120, vertical: true);
        SetSplitDistance(splitOuter, (int)(splitOuter.Height * 0.38), panel1Min: 160, panel2Min: 240, vertical: false);
    }

    private static void SetSplitDistance(SplitContainer split, int distance, int panel1Min, int panel2Min, bool vertical)
    {
        split.Panel1MinSize = panel1Min;
        split.Panel2MinSize = panel2Min;

        var total = vertical ? split.Width : split.Height;
        var max = total - split.SplitterWidth - panel2Min;
        if (max < panel1Min)
            return;

        split.SplitterDistance = Math.Clamp(distance, panel1Min, max);
    }

    private void LoadDefaults()
    {
        var dataDir = ResolveDataDirectory();
        var rulesPath = Path.Combine(dataDir, "sample_rules.json");
        if (File.Exists(rulesPath))
        {
            _ruleSet = RuleSet.LoadFromJson(rulesPath);
            _engine = new EventRuleEngine(_ruleSet);
            ruleEditor.Bind(_ruleSet);
        }

        var eventsPath = Path.Combine(dataDir, "sample_events.jsonl");
        if (File.Exists(eventsPath))
            _demoStream = new FileReadOnlyStream<EventRecord>(eventsPath, _serializer);
    }

    private static string ResolveDataDirectory()
    {
        var baseDir = AppContext.BaseDirectory;
        var candidate = Path.Combine(baseDir, "data");
        if (Directory.Exists(candidate))
            return candidate;
        return Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "data"));
    }

    private void BtnAdd_Click(object sender, EventArgs e)
    {
        var type = cmbEventType.SelectedItem?.ToString();
        if (string.IsNullOrEmpty(type))
            return;
        ProcessEvent(new EventRecord(type, NullIfEmpty(txtPayload.Text), DateTime.UtcNow));
    }

    private static string? NullIfEmpty(string text)
    {
        var trimmed = text.Trim();
        return trimmed.Length == 0 ? null : trimmed;
    }

    private void BtnNextDemo_Click(object sender, EventArgs e)
    {
        if (_demoStream is null)
        {
            MessageBox.Show("Файл data/sample_events.jsonl не найден.", "Ошибка",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!_demoStream.IsOpen)
            _demoStream.Open();

        if (_demoStream.IsEndOfStream())
        {
            MessageBox.Show("Демо-сценарий завершён. Перезапустите приложение.", "Конец потока",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        ProcessEvent(_demoStream.Read());
    }

    private void ProcessEvent(EventRecord ev)
    {
        _events.Add(ev);
        var reaction = _engine.ProcessEvent(ev);
        if (reaction is not null)
            _reactions.Add(reaction);
        RefreshGrids();
    }

    private void RefreshGrids()
    {
        eventsGrid.Rows.Clear();
        foreach (var ev in _events)
            eventsGrid.Rows.Add(ev.Type, ev.Payload ?? "—");

        reactionsGrid.Rows.Clear();
        foreach (var r in _reactions)
            reactionsGrid.Rows.Add(r.RuleName ?? "—", r.Text);
    }

    private void RuleEditor_RulesChanged(object sender, EventArgs e)
    {
        ruleEditor.ApplyToRuleSet();
        _engine = new EventRuleEngine(_ruleSet);
    }
}
