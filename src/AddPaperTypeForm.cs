using System.Globalization;
using System.Windows.Forms;

namespace PrintCoverageAnalyzer;

public sealed class AddPaperTypeForm : Form
{
    private readonly TextBox _nameBox;
    private readonly TextBox _priceBox;

    public PaperType? Result { get; private set; }

    public AddPaperTypeForm()
    {
        Text = "Papiersorte hinzufügen";
        Width = 420;
        Height = 210;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;

        var nameLabel = new Label { Left = 16, Top = 20, Width = 150, Text = "Name der Papiersorte" };
        _nameBox = new TextBox { Left = 16, Top = 42, Width = 370 };

        var priceLabel = new Label { Left = 16, Top = 78, Width = 220, Text = "Preis pro m² (EUR)" };
        _priceBox = new TextBox { Left = 16, Top = 100, Width = 180, Text = "10,00" };

        var saveButton = new Button { Text = "Speichern", Left = 220, Top = 132, Width = 80, DialogResult = DialogResult.None };
        var cancelButton = new Button { Text = "Abbrechen", Left = 306, Top = 132, Width = 80, DialogResult = DialogResult.Cancel };

        saveButton.Click += (_, _) => OnSave();

        Controls.Add(nameLabel);
        Controls.Add(_nameBox);
        Controls.Add(priceLabel);
        Controls.Add(_priceBox);
        Controls.Add(saveButton);
        Controls.Add(cancelButton);

        AcceptButton = saveButton;
        CancelButton = cancelButton;
    }

    private void OnSave()
    {
        string name = _nameBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Bitte einen Namen für die Papiersorte eingeben.", "Fehlende Eingabe", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string normalized = _priceBox.Text.Trim().Replace('.', ',');
        if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.GetCultureInfo("de-DE"), out decimal price) || price <= 0)
        {
            MessageBox.Show("Bitte einen gültigen Preis > 0 eingeben (z. B. 12,50).", "Ungültiger Preis", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Result = new PaperType(name, decimal.Round(price, 2));
        DialogResult = DialogResult.OK;
        Close();
    }
}
