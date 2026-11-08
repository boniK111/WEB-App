using System;
using System.Windows.Forms;

namespace AlfaSoftApp
{
    public class EmployeeForm : Form
    {
        private readonly int? _id;
        private readonly TextBox txtFullName = new TextBox { Width = 300 };
        private readonly TextBox txtEmail = new TextBox { Width = 300 };
        private readonly TextBox txtPhone = new TextBox { Width = 300 };
        private readonly TextBox txtHireDate = new TextBox { Width = 300 };
        private readonly ComboBox cbRole = new ComboBox { Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };

        public EmployeeForm(int? id)
        {
            _id = id;
            Text = id == null ? "Новый сотрудник" : $"Сотрудник #{id}";
            Width = 440; Height = 300;
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

            AddRow(layout, "ФИО:", txtFullName);
            AddRow(layout, "E-mail:", txtEmail);
            AddRow(layout, "Телефон:", txtPhone);
            AddRow(layout, "Дата приёма:", txtHireDate);
            AddRow(layout, "Роль:", cbRole);

            var btnSave = new Button { Text = "Сохранить", DialogResult = DialogResult.OK };
            var btnCancel = new Button { Text = "Отмена", DialogResult = DialogResult.Cancel };
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 40, FlowDirection = FlowDirection.RightToLeft };
            buttons.Controls.Add(btnCancel);
            buttons.Controls.Add(btnSave);

            Controls.Add(layout);
            Controls.Add(buttons);
            AcceptButton = btnSave;
            CancelButton = btnCancel;

            FillRoles();
            if (id != null) LoadData(id.Value);

            btnSave.Click += (_, _) => Save();
        }

        private static void AddRow(TableLayoutPanel layout, string label, Control input)
        {
            var lbl = new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 8, 3, 3) };
            layout.Controls.Add(lbl);
            layout.Controls.Add(input);
        }

        private void FillRoles()
        {
            cbRole.DataSource = Db.Query("SELECT role_id, name FROM \"Роль\" ORDER BY name");
            cbRole.DisplayMember = "name";
            cbRole.ValueMember = "role_id";
        }

        private void LoadData(int id)
        {
            var dt = Db.Query("SELECT * FROM \"Сотрудник\" WHERE employee_id = @id", ("@id", id));
            if (dt.Rows.Count == 0) return;
            var row = dt.Rows[0];
            cbRole.SelectedValue = row["role_id"];
            txtFullName.Text = row["full_name"].ToString();
            txtEmail.Text = row["email"].ToString();
            txtPhone.Text = row["phone"]?.ToString() ?? "";
            txtHireDate.Text = row["hire_date"]?.ToString() ?? "";
        }

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("ФИО и E-mail обязательны.");
                return;
            }

            try
            {
                if (_id == null)
                {
                    Db.Execute(
                        @"INSERT INTO ""Сотрудник""(role_id, full_name, email, phone, hire_date)
                          VALUES(@role, @name, @email, @phone, @hire)",
                        ("@role", cbRole.SelectedValue),
                        ("@name", txtFullName.Text),
                        ("@email", txtEmail.Text),
                        ("@phone", txtPhone.Text),
                        ("@hire", txtHireDate.Text));
                }
                else
                {
                    Db.Execute(
                        @"UPDATE ""Сотрудник""
                          SET role_id=@role, full_name=@name, email=@email,
                              phone=@phone, hire_date=@hire
                          WHERE employee_id=@id",
                        ("@role", cbRole.SelectedValue),
                        ("@name", txtFullName.Text),
                        ("@email", txtEmail.Text),
                        ("@phone", txtPhone.Text),
                        ("@hire", txtHireDate.Text),
                        ("@id", _id));
                }
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message);
            }
        }
    }
}