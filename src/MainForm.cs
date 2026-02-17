using System.Globalization;
using System.Windows.Forms;

namespace PrintCoverageAnalyzer;

public sealed class MainForm : Form
{
    private readonly ComboBox _paperCombo;
    private readonly Button _addPaperButton;
    private readonly Button _selectFilesButton;
    private readonly Button _calculateButton;
    private readonly NumericUpDown _areaInput;
    private readonly NumericUpDown _whiteThresholdInput;
    private readonly NumericUpDown _pdfDpiInput;
    private readonly TextBox _outputBox;

    private readonly PaperTypeStore _paperTypeStore = new();
    private readonly List<PaperType> _paperTypes;
    private List<string> _selectedFiles = [];

    public MainForm(IEnumerable<string>? initialFiles = null)
    {
        Text = "PrintCoverageAnalyzer";
        Width = 860;
        Height = 640;
        StartPosition = FormStartPosition.CenterScreen;

        AllowDrop = true;
        DragEnter += OnDragEnter;
        DragDrop += OnDragDrop;

        _paperTypes = _paperTypeStore.Load();

        var paperLabel = new Label { Left = 16, Top = 18, Width = 130, Text = "Papiersorte wählen" };
        _paperCombo = new ComboBox
        {
            Left = 16,
            Top = 40,
            Width = 330,
            DropDownStyle = ComboBoxStyle.DropDownList,
            DataSource = _paperTypes,
            DisplayMember = nameof(PaperType.Name),
        };

        _addPaperButton = new Button { Left = 360, Top = 39, Width = 180, Height = 28, Text = "+ Neue Papiersorte" };
        _addPaperButton.Click += (_, _) => AddPaperType();

        var areaLabel = new Label { Left = 16, Top = 82, Width = 170, Text = "Fläche in m² (Postergröße)" };
        _areaInput = new NumericUpDown
        {
            Left = 16,
            Top = 104,
            Width = 120,
            DecimalPlaces = 3,
            Minimum = 0.001m,
            Maximum = 500,
            Value = 1.000m,
            Increment = 0.050m,
        };

        var whiteLabel = new Label { Left = 160, Top = 82, Width = 120, Text = "Weiß-Schwelle" };
        _whiteThresholdInput = new NumericUpDown
        {
            Left = 160,
            Top = 104,
            Width = 80,
            Minimum = 1,
            Maximum = 255,
            Value = 245,
        };

        var dpiLabel = new Label { Left = 260, Top = 82, Width = 90, Text = "PDF DPI" };
        _pdfDpiInput = new NumericUpDown
        {
            Left = 260,
            Top = 104,
            Width = 80,
            Minimum = 72,
            Maximum = 1200,
            Value = 300,
            Increment = 10,
        };

        _selectFilesButton = new Button { Left = 16, Top = 146, Width = 250, Height = 34, Text = "Datei(en) auswählen (JPG/PNG/PDF)" };
        _selectFilesButton.Click += (_, _) => SelectFiles();

        _calculateButton = new Button { Left = 280, Top = 146, Width = 260, Height = 34, Text = "Fläche berechnen & Preis kalkulieren" };
        _calculateButton.Click += (_, _) => Calculate();

        _outputBox = new TextBox
        {
            Left = 16,
            Top = 196,
            Width = 810,
            Height = 390,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new System.Drawing.Font("Consolas", 10),
        };

        Controls.AddRange([
            paperLabel, _paperCombo, _addPaperButton,
            areaLabel, _areaInput, whiteLabel, _whiteThresholdInput, dpiLabel, _pdfDpiInput,
            _selectFilesButton, _calculateButton, _outputBox,
        ]);

        if (initialFiles is not null)
        {
            SetSelectedFiles(initialFiles);
        }
    }

    private void OnDragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
        {
            e.Effect = DragDropEffects.Copy;
        }
    }

    private void OnDragDrop(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetData(DataFormats.FileDrop) is not string[] paths)
        {
            return;
        }

        SetSelectedFiles(paths);
    }

    private void SetSelectedFiles(IEnumerable<string> files)
    {
        _selectedFiles = files
            .Where(File.Exists)
            .Where(IsSupportedFile)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        _outputBox.Text = _selectedFiles.Count == 0
            ? "Keine unterstützten Dateien ausgewählt."
            : $"{_selectedFiles.Count} Datei(en) ausgewählt.{Environment.NewLine}";
    }

    private static bool IsSupportedFile(string path)
    {
        string ext = Path.GetExtension(path);
        return ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".png", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".tif", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".tiff", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".bmp", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".webp", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".pdf", StringComparison.OrdinalIgnoreCase);
    }

    private void AddPaperType()
    {
        using var dialog = new AddPaperTypeForm();
        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Result is null)
        {
            return;
        }

        _paperTypes.Add(dialog.Result);
        _paperTypes.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.CurrentCultureIgnoreCase));
        _paperTypeStore.Save(_paperTypes);

        _paperCombo.DataSource = null;
        _paperCombo.DataSource = _paperTypes;
        _paperCombo.DisplayMember = nameof(PaperType.Name);
        _paperCombo.SelectedItem = dialog.Result;
    }

    private void SelectFiles()
    {
        using var dialog = new OpenFileDialog
        {
            Multiselect = true,
            Filter = "Bilder/PDF|*.jpg;*.jpeg;*.png;*.tif;*.tiff;*.bmp;*.webp;*.pdf",
            Title = "Dateien für die Flächenanalyse auswählen",
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            SetSelectedFiles(dialog.FileNames);
        }
    }

    private void Calculate()
    {
        if (_paperCombo.SelectedItem is not PaperType paper)
        {
            MessageBox.Show("Bitte zuerst eine Papiersorte auswählen.", "Fehlende Auswahl", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_selectedFiles.Count == 0)
        {
            MessageBox.Show("Bitte zuerst mindestens eine Datei auswählen.", "Keine Datei ausgewählt", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var options = new CoverageOptions
        {
            WhiteThreshold = (byte)_whiteThresholdInput.Value,
            TransparentThreshold = 8,
            PdfDpi = (int)_pdfDpiInput.Value,
        };

        decimal area = _areaInput.Value;
        var culture = CultureInfo.GetCultureInfo("de-DE");

        var lines = new List<string>
        {
            $"Papiersorte: {paper.Name} ({paper.PricePerSquareMeter.ToString("C", culture)} pro m²)",
            $"Fläche: {area:0.###} m²",
            ""
        };

        decimal total = 0;

        try
        {
            foreach (string path in _selectedFiles)
            {
                foreach (var (name, image) in InputLoader.Load(path, options))
                {
                    using (image)
                    {
                        var result = CoverageAnalyzer.Analyze(image, name, options);
                        decimal factor = (decimal)result.PrintedPercent / 100m;
                        decimal price = decimal.Round(area * paper.PricePerSquareMeter * factor, 2);
                        total += price;

                        lines.Add($"[{result.SourceFile}] {result.Width}x{result.Height}");
                        lines.Add($"  Bedruckte Fläche: {result.PrintedPercent:F2}%");
                        lines.Add($"  Preis (nach Flächendeckung): {price.ToString("C", culture)}");
                        lines.Add($"  CMYK-Schätzung: C={result.CyanPercent:F2}% M={result.MagentaPercent:F2}% Y={result.YellowPercent:F2}% K={result.BlackPercent:F2}%");
                        lines.Add(string.Empty);
                    }
                }
            }

            lines.Add($"GESAMTPREIS: {total.ToString("C", culture)}");
            _outputBox.Text = string.Join(Environment.NewLine, lines);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Fehler bei der Berechnung: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
