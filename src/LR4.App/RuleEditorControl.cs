using LR4.Core.Events;

namespace LR4.App;

public sealed class RuleEditorControl : UserControl
{
    private RuleSet? _ruleSet;
    private readonly DataGridView _grid = new();
    private readonly BindingSource _binding = new();
    private List<RuleDefinition> _editableRules = new();

    public event EventHandler? RulesChanged;

    public RuleEditorControl()
    {
        var bar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 36,
            Padding = new Padding(8, 4, 8, 0),
            FlowDirection = FlowDirection.LeftToRight
        };

        var btnAdd = new Button { Text = "Добавить", Width = 90, Height = 28 };
        var btnEdit = new Button { Text = "Изменить", Width = 90, Height = 28 };
        var btnDelete = new Button { Text = "Удалить", Width = 90, Height = 28 };

        btnAdd.Click += (_, _) => AddRule();
        btnEdit.Click += (_, _) => EditSelected();
        btnDelete.Click += (_, _) => DeleteSelected();

        bar.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete });

        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.AutoGenerateColumns = false;
        _grid.AllowUserToAddRows = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.RowHeadersVisible = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(RuleDefinition.Name), HeaderText = "Правило" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(RuleDefinition.PatternValue), HeaderText = "Событие" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(RuleDefinition.ReactionText), HeaderText = "Реакция" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(RuleDefinition.Priority), HeaderText = "Приоритет", Width = 70 });

        _binding.DataSource = _editableRules;
        _grid.DataSource = _binding;

        Controls.Add(_grid);
        Controls.Add(bar);
    }

    public void Bind(RuleSet ruleSet)
    {
        _ruleSet = ruleSet;
        ReloadFromRuleSet();
    }

    public void ApplyToRuleSet()
    {
        if (_ruleSet is null)
            return;
        _ruleSet.SetRules(_editableRules);
    }

    private void ReloadFromRuleSet()
    {
        if (_ruleSet is null)
            return;

        _editableRules = _ruleSet.Rules.ToList();
        _binding.DataSource = _editableRules;
        _binding.ResetBindings(false);
    }

    private void CommitChanges()
    {
        ApplyToRuleSet();
        ReloadFromRuleSet();
        RulesChanged?.Invoke(this, EventArgs.Empty);
    }

    private void AddRule()
    {
        using var dlg = new RuleEditDialog();
        if (dlg.ShowDialog() != DialogResult.OK || dlg.Rule is null)
            return;

        _editableRules.Add(dlg.Rule);
        CommitChanges();
    }

    private void EditSelected()
    {
        if (_grid.CurrentRow?.DataBoundItem is not RuleDefinition rule)
        {
            MessageBox.Show("Выберите правило в таблице.", "Подсказка",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dlg = new RuleEditDialog(rule);
        if (dlg.ShowDialog() != DialogResult.OK || dlg.Rule is null)
            return;

        _editableRules[_grid.CurrentRow.Index] = dlg.Rule;
        CommitChanges();
    }

    private void DeleteSelected()
    {
        if (_grid.CurrentRow is null)
        {
            MessageBox.Show("Выберите правило для удаления.", "Подсказка",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        _editableRules.RemoveAt(_grid.CurrentRow.Index);
        CommitChanges();
    }
}
