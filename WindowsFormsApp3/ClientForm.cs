using System;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace AlfaSoftApp;

public class ClientForm : Form
{
    private readonly int? _id;
    private readonly TextBox txtName = new() { Width = 300 };
    private readonly TextBox txtInn = new() { Width = 300 };
    private readonly TextBox txtKpp = new() { Width = 300 };
    private readonly TextBox txtAddress = new() { Width = 300 };
    private readonly TextBox txtPhone = new() { Width = 300 };
    private readonly TextBox txtEmail = new() { Width = 300 };

    public ClientForm(int? id)
    {
        _id = id;
        Text = id == null ? "Новый клиент" : $"Клиент #{id}";
        Width = 420; Height = 320;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Padding = new Padding(10)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddRow(layout, "Название:", txtName);
        AddRow(layout, "ИНН:", txtInn);
        AddRow(layout, "КПП:", txtKpp);
        AddRow(layout, "Адрес:", txtAddress);
        AddRow(layout, "Телефон:", txtPhone);
        AddRow(layout, "E-mail:", txtEmail);

        var btnSave = new Button { Text = "Сохранить", DialogResult = DialogResult.OK };
        var btnCancel = new Button { Text = "Отмена", DialogResult = DialogResult.Cancel };
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 40, FlowDirection = FlowDirection.RightToLeft };
        buttons.Controls.Add(btnCancel);
        buttons.Controls.Add(btnSave);

        Controls.Add(layout);
        Controls.Add(buttons);
        AcceptButton = btnSave;
        CancelButton = btnCancel;

        if (id != null) LoadData(id.Value);

        btnSave.Click += (_, _) => { Save(); DialogResult = DialogResult.OK; };
    }

    private static void AddRow(TableLayoutPanel layout, string label, Control input)
    {
        var lbl = new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 8, 3, 3) };
        layout.Controls.Add(lbl);
        layout.Controls.Add(input);
    }

    private void LoadData(int id)
    {
        var dt = Db.Query("SELECT * FROM \"Клиент\" WHERE client_id = @id", ("@id", id));
        if (dt.Rows.Count == 0) return;
        var row = dt.Rows[0];
        txtName.Text = row["name"].ToString();
        txtInn.Text = row["inn"].ToString();
        txtKpp.Text = row["kpp"]?.ToString() ?? "";
        txtAddress.Text = row["address"]?.ToString() ?? "";
        txtPhone.Text = row["phone"]?.ToString() ?? "";
        txtEmail.Text = row["email"]?.ToString() ?? "";
    }

    private void Save()
    {
        if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtInn.Text))
        {
            MessageBox.Show("Название и ИНН обязательны.");
            return;
        }

        try
        {
            if (_id == null)
            {
                Db.Execute(
                    @"INSERT INTO ""Клиент""(name, inn, kpp, address, phone, email)
                      VALUES(@name, @inn, @kpp, @address, @phone, @email)",
                    ("@name", txtName.Text), ("@inn", txtInn.Text),
                    ("@kpp", txtKpp.Text), ("@address", txtAddress.Text),
                    ("@phone", txtPhone.Text), ("@email", txtEmail.Text));
            }
            else
            {
                Db.Execute(
                    @"UPDATE ""Клиент""
                      SET name=@name, inn=@inn, kpp=@kpp,
                          address=@address, phone=@phone, email=@email
                      WHERE client_id=@id",
                    ("@name", txtName.Text), ("@inn", txtInn.Text),
                    ("@kpp", txtKpp.Text), ("@address", txtAddress.Text),
                    ("@phone", txtPhone.Text), ("@email", txtEmail.Text),
                    ("@id", _id));
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка сохранения: " + ex.Message);
        }
    }
}