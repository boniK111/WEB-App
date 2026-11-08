using System;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace AlfaSoftApp;

public class TicketForm : Form
{
    private readonly int? _id;
    private readonly ComboBox cbClient = new() { Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox cbType = new() { Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox cbStatus = new() { Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox cbSla = new() { Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox txtSubject = new() { Width = 300 };
    private readonly TextBox txtDesc = new() { Width = 300, Height = 60, Multiline = true };

    public TicketForm(int? id)
    {
        _id = id;
        Text = id == null ? "Новая заявка" : $"Заявка #{id}";
        Width = 440; Height = 360;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(10) };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddRow(layout, "Клиент:", cbClient);
        AddRow(layout, "Тип:", cbType);
        AddRow(layout, "Статус:", cbStatus);
        AddRow(layout, "SLA:", cbSla);
        AddRow(layout, "Тема:", txtSubject);
        AddRow(layout, "Описание:", txtDesc);

        var btnSave = new Button { Text = "Сохранить", DialogResult = DialogResult.OK };
        var btnCancel = new Button { Text = "Отмена", DialogResult = DialogResult.Cancel };
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 40, FlowDirection = FlowDirection.RightToLeft };
        buttons.Controls.Add(btnCancel);
        buttons.Controls.Add(btnSave);

        Controls.Add(layout);
        Controls.Add(buttons);
        AcceptButton = btnSave;
        CancelButton = btnCancel;

        FillCombos();
        if (id != null) LoadData(id.Value);

        btnSave.Click += (_, _) => { Save(); DialogResult = DialogResult.OK; };
    }

    private static void AddRow(TableLayoutPanel layout, string label, Control input)
    {
        var lbl = new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 8, 3, 3) };
        layout.Controls.Add(lbl);
        layout.Controls.Add(input);
    }

    private void FillCombos()
    {
        cbClient.DataSource = Db.Query("SELECT client_id, name FROM \"Клиент\" ORDER BY name");
        cbClient.DisplayMember = "name";
        cbClient.ValueMember = "client_id";

        cbType.DataSource = Db.Query("SELECT type_id, name FROM \"Тип заявки\" ORDER BY name");
        cbType.DisplayMember = "name";
        cbType.ValueMember = "type_id";

        cbStatus.DataSource = Db.Query("SELECT status_id, name FROM \"Статус заявки\" ORDER BY status_id");
        cbStatus.DisplayMember = "name";
        cbStatus.ValueMember = "status_id";

        cbSla.DataSource = Db.Query("SELECT sla_id, name FROM \"SLA-правило\" ORDER BY sla_id");
        cbSla.DisplayMember = "name";
        cbSla.ValueMember = "sla_id";
    }

    private void LoadData(int id)
    {
        var dt = Db.Query("SELECT * FROM \"Заявка\" WHERE ticket_id = @id", ("@id", id));
        if (dt.Rows.Count == 0) return;
        var row = dt.Rows[0];
        cbClient.SelectedValue = row["client_id"];
        cbType.SelectedValue = row["type_id"];
        cbStatus.SelectedValue = row["status_id"];
        cbSla.SelectedValue = row["sla_id"];
        txtSubject.Text = row["subject"].ToString();
        txtDesc.Text = row["description"]?.ToString() ?? "";
    }

    private void Save()
    {
        if (string.IsNullOrWhiteSpace(txtSubject.Text))
        {
            MessageBox.Show("Тема обязательна.");
            return;
        }
        try
        {
            if (_id == null)
            {
                Db.Execute(
                    @"INSERT INTO ""Заявка""(client_id, type_id, status_id, sla_id, subject, description)
                      VALUES(@client, @type, @status, @sla, @subject, @desc)",
                    ("@client", cbClient.SelectedValue),
                    ("@type", cbType.SelectedValue),
                    ("@status", cbStatus.SelectedValue),
                    ("@sla", cbSla.SelectedValue),
                    ("@subject", txtSubject.Text),
                    ("@desc", txtDesc.Text));
            }
            else
            {
                Db.Execute(
                    @"UPDATE ""Заявка""
                      SET client_id=@client, type_id=@type, status_id=@status,
                          sla_id=@sla, subject=@subject, description=@desc
                      WHERE ticket_id=@id",
                    ("@client", cbClient.SelectedValue),
                    ("@type", cbType.SelectedValue),
                    ("@status", cbStatus.SelectedValue),
                    ("@sla", cbSla.SelectedValue),
                    ("@subject", txtSubject.Text),
                    ("@desc", txtDesc.Text),
                    ("@id", _id));
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка сохранения: " + ex.Message);
        }
    }
}