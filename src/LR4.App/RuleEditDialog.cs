using LR4.Core.Events;

namespace LR4.App;

public sealed class RuleEditDialog : Form
{
    private readonly TextBox _name = new();
    private readonly ComboBox _eventType = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _reaction = new();
    private readonly NumericUpDown _priority = new() { Minimum = 0, Maximum = 1000, Value = 100, Width = 80 };

    public RuleDefinition? Rule { get; private set; }

    public RuleEditDialog(RuleDefinition? existing = null)
    {
        Text = existing is null ? "Добавить правило" : "Изменить правило";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Width = 400;
        Height = 250;
        StartPosition = FormStartPosition.CenterParent;

        _eventType.Items.AddRange(new object[]
        {
            "ClientMessage",
            "Breakdown",
            "TicketCreated",
            "ExpertReply",
            "(любое — Always)"
        });

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 4, Padding = new Padding(12) };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        void Row(int i, string label, Control c)
        {
            layout.Controls.Add(new Label { Text = label, AutoSize = true }, 0, i);
            c.Dock = DockStyle.Fill;
            layout.Controls.Add(c, 1, i);
        }

        Row(0, "Имя:", _name);
        Row(1, "Событие:", _eventType);
        Row(2, "Реакция:", _reaction);
        Row(3, "Приоритет:", _priority);

        var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Dock = DockStyle.Bottom, Height = 36 };
        ok.Click += (_, _) => Save();

        Controls.Add(layout);
        Controls.Add(ok);
        AcceptButton = ok;

        if (existing is null)
        {
            _eventType.SelectedIndex = 0;
            return;
        }

        _name.Text = existing.Name;
        _reaction.Text = existing.ReactionText;
        _priority.Value = existing.Priority;
        if (existing.PatternType == RulePatternType.Always)
            _eventType.SelectedIndex = 4;
        else
            _eventType.SelectedItem = existing.PatternValue;
    }

    private void Save()
    {
        var selected = _eventType.SelectedItem?.ToString() ?? "";
        var isAlways = selected.StartsWith('(');

        Rule = new RuleDefinition
        {
            Name = _name.Text.Trim(),
            WindowSize = 1,
            PatternType = isAlways ? RulePatternType.Always : RulePatternType.LastEventEquals,
            PatternValue = isAlways ? "" : selected,
            ReactionText = _reaction.Text.Trim(),
            Priority = isAlways ? 1 : (int)_priority.Value
        };

        try
        {
            new RuleSet().SetRules(new[] { Rule });
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            Rule = null;
            DialogResult = DialogResult.None;
        }
    }
}
