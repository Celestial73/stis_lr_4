namespace LR4.App;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null!;
    private SplitContainer splitOuter;
    private SplitContainer splitGrids;
    private DataGridView eventsGrid;
    private DataGridView reactionsGrid;
    private TabControl tabBottom;
    private TabPage tabEvents;
    private TabPage tabRules;
    private Panel panelInput;
    private ComboBox cmbEventType;
    private TextBox txtPayload;
    private Button btnAdd;
    private Button btnNextDemo;
    private RuleEditorControl ruleEditor;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        splitOuter = new SplitContainer();
        splitGrids = new SplitContainer();
        eventsGrid = new DataGridView();
        reactionsGrid = new DataGridView();
        tabBottom = new TabControl();
        tabEvents = new TabPage();
        tabRules = new TabPage();
        panelInput = new Panel();
        cmbEventType = new ComboBox();
        txtPayload = new TextBox();
        btnAdd = new Button();
        btnNextDemo = new Button();
        ruleEditor = new RuleEditorControl();

        ((System.ComponentModel.ISupportInitialize)splitOuter).BeginInit();
        splitOuter.Panel1.SuspendLayout();
        splitOuter.Panel2.SuspendLayout();
        splitOuter.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitGrids).BeginInit();
        splitGrids.Panel1.SuspendLayout();
        splitGrids.Panel2.SuspendLayout();
        splitGrids.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)eventsGrid).BeginInit();
        ((System.ComponentModel.ISupportInitialize)reactionsGrid).BeginInit();
        tabBottom.SuspendLayout();
        tabEvents.SuspendLayout();
        tabRules.SuspendLayout();
        SuspendLayout();

        Text = "Техподдержка — ЛР-4";
        Width = 960;
        Height = 680;
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(800, 520);

        eventsGrid.Dock = DockStyle.Fill;
        eventsGrid.ReadOnly = true;
        eventsGrid.AllowUserToAddRows = false;
        eventsGrid.RowHeadersVisible = false;
        eventsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        eventsGrid.Columns.Add("type", "Событие");
        eventsGrid.Columns.Add("payload", "Детали");

        reactionsGrid.Dock = DockStyle.Fill;
        reactionsGrid.ReadOnly = true;
        reactionsGrid.AllowUserToAddRows = false;
        reactionsGrid.RowHeadersVisible = false;
        reactionsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        reactionsGrid.Columns.Add("rule", "Правило");
        reactionsGrid.Columns.Add("text", "Реакция");

        splitGrids.Dock = DockStyle.Fill;
        splitGrids.Orientation = Orientation.Vertical;
        splitGrids.Panel1.Controls.Add(eventsGrid);
        splitGrids.Panel2.Controls.Add(reactionsGrid);

        splitOuter.Dock = DockStyle.Fill;
        splitOuter.Orientation = Orientation.Horizontal;
        splitOuter.Panel1.Controls.Add(splitGrids);
        splitOuter.Panel2.Controls.Add(tabBottom);
        splitOuter.SplitterWidth = 6;

        cmbEventType.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbEventType.Dock = DockStyle.Fill;

        txtPayload.Dock = DockStyle.Fill;
        txtPayload.PlaceholderText = "Текст обращения, комментарий…";

        btnAdd.Text = "Добавить";
        btnAdd.Dock = DockStyle.Fill;
        btnAdd.MinimumSize = new Size(90, 32);
        btnAdd.Click += BtnAdd_Click;

        btnNextDemo.Text = "Следующее (демо)";
        btnNextDemo.Dock = DockStyle.Fill;
        btnNextDemo.MinimumSize = new Size(120, 32);
        btnNextDemo.Click += BtnNextDemo_Click;

        var inputLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 48,
            Padding = new Padding(8, 6, 8, 6),
            ColumnCount = 6,
            RowCount = 1
        };
        inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 108));
        inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 138));
        inputLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var lblEvent = new Label { Text = "Событие:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 8, 4, 0) };
        var lblDetails = new Label { Text = "Детали:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(8, 8, 4, 0) };
        inputLayout.Controls.Add(lblEvent, 0, 0);
        inputLayout.Controls.Add(cmbEventType, 1, 0);
        inputLayout.Controls.Add(lblDetails, 2, 0);
        inputLayout.Controls.Add(txtPayload, 3, 0);
        inputLayout.Controls.Add(btnAdd, 4, 0);
        inputLayout.Controls.Add(btnNextDemo, 5, 0);

        panelInput.Dock = DockStyle.Top;
        panelInput.Height = 48;
        panelInput.Controls.Add(inputLayout);

        tabEvents.Text = "События";
        tabEvents.Padding = new Padding(4);
        tabEvents.Controls.Add(panelInput);

        ruleEditor.Dock = DockStyle.Fill;
        ruleEditor.RulesChanged += RuleEditor_RulesChanged;

        tabRules.Text = "Правила";
        tabRules.Padding = new Padding(4);
        tabRules.Controls.Add(ruleEditor);

        tabBottom.Dock = DockStyle.Fill;
        tabBottom.TabPages.Add(tabEvents);
        tabBottom.TabPages.Add(tabRules);

        Controls.Add(splitOuter);

        ((System.ComponentModel.ISupportInitialize)splitGrids).EndInit();
        splitGrids.Panel1.ResumeLayout(false);
        splitGrids.Panel2.ResumeLayout(false);
        splitGrids.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitOuter).EndInit();
        splitOuter.Panel1.ResumeLayout(false);
        splitOuter.Panel2.ResumeLayout(false);
        splitOuter.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)eventsGrid).EndInit();
        ((System.ComponentModel.ISupportInitialize)reactionsGrid).EndInit();
        tabRules.ResumeLayout(false);
        tabEvents.ResumeLayout(false);
        tabBottom.ResumeLayout(false);
        ResumeLayout(false);
    }
}
