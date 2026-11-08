using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using System.Linq;

namespace AlfaSoftApp;

public partial class MainForm : Form
{
    private readonly DataGridView grid = new()
    {
        Dock = DockStyle.Fill,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect
    };

    private readonly ComboBox tableSelector = new()
    {
        Dock = DockStyle.Top,
        DropDownStyle = ComboBoxStyle.DropDownList,
        Height = 30
    };

    // Список «вкладок»: имя, SQL-запрос, ключевое поле
    private readonly Dictionary<string, (string sql, string key)> _views = new()
    {
        ["Клиенты"] = (
            @"SELECT client_id, name, inn, kpp, address, phone, email, created_at
              FROM ""Клиент"" ORDER BY client_id",
            "client_id"),

        ["Сотрудники"] = (
            @"SELECT e.employee_id, e.full_name, r.name AS role_name,
                     e.email, e.phone, e.hire_date
              FROM ""Сотрудник"" e
              JOIN ""Роль"" r ON r.role_id = e.role_id
              ORDER BY e.employee_id",
            "employee_id"),

        ["Заявки"] = (
            @"SELECT t.ticket_id, c.name AS client_name, tt.name AS type_name,
                     ts.name AS status_name, e.full_name AS assignee,
                     t.subject, t.created_at, t.closed_at
              FROM ""Заявка"" t
              JOIN ""Клиент"" c        ON c.client_id  = t.client_id
              JOIN ""Тип заявки"" tt   ON tt.type_id   = t.type_id
              JOIN ""Статус заявки"" ts ON ts.status_id = t.status_id
              LEFT JOIN ""Сотрудник"" e ON e.employee_id = t.assignee_id
              ORDER BY t.ticket_id",
            "ticket_id"),

        ["Трудозатраты"] = (
            @"SELECT te.entry_id, e.full_name AS employee,
                     te.ticket_id, te.task_id, te.act_id,
                     te.work_date, te.hours, te.comment
              FROM ""Трудозатраты"" te
              JOIN ""Сотрудник"" e ON e.employee_id = te.employee_id
              ORDER BY te.entry_id",
            "entry_id")
    };

    public MainForm()
    {
        Text = "ИС «Альфа Софт» — демо";
        Width = 1000;
        Height = 600;
        StartPosition = FormStartPosition.CenterScreen;

        // Панель кнопок
        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 45,
            Padding = new Padding(5)
        };

        var btnRefresh = new Button { Text = "Обновить", Width = 100 };
        var btnAdd = new Button { Text = "Добавить", Width = 100 };
        var btnEdit = new Button { Text = "Изменить", Width = 100 };
        var btnDelete = new Button { Text = "Удалить", Width = 100 };

        btnRefresh.Click += (_, _) => LoadCurrent();
        btnAdd.Click += (_, _) => AddRecord();
        btnEdit.Click += (_, _) => EditRecord();
        btnDelete.Click += (_, _) => DeleteRecord();

        buttonPanel.Controls.AddRange(new Control[]
            { tableSelector, btnRefresh, btnAdd, btnEdit, btnDelete });

        tableSelector.Items.AddRange(_views.Keys.ToArray());
        tableSelector.SelectedIndex = 0;
        tableSelector.SelectedIndexChanged += (_, _) => LoadCurrent();

        // Важно: сначала grid, потом панели — иначе Dock перекроет
        Controls.Add(grid);
        Controls.Add(buttonPanel);

        Load += (_, _) => LoadCurrent();
    }

    private void LoadCurrent()
    {
        if (tableSelector.SelectedItem is not string key) return;
        if (!_views.TryGetValue(key, out var view)) return;
        grid.DataSource = Db.Query(view.sql);
    }

    private void AddRecord()
    {
        var key = tableSelector.SelectedItem as string;
        switch (key)
        {
            case "Клиенты": new ClientForm(null).ShowDialog(); break;
            case "Сотрудники": new EmployeeForm(null).ShowDialog(); break;
            case "Заявки": new TicketForm(null).ShowDialog(); break;
            
            default:
                MessageBox.Show("Форма добавления для этого раздела не реализована.");
                return;
        }
        LoadCurrent();
    }

    private void EditRecord()
    {
        if (grid.CurrentRow == null) return;
        var key = tableSelector.SelectedItem as string;

        switch (key)
        {
            case "Клиенты":
                new ClientForm(Convert.ToInt32(grid.CurrentRow.Cells["client_id"].Value)).ShowDialog();
                break;
            case "Сотрудники":
                new EmployeeForm(Convert.ToInt32(grid.CurrentRow.Cells["employee_id"].Value)).ShowDialog();
                break;
            case "Заявки":
                new TicketForm(Convert.ToInt32(grid.CurrentRow.Cells["ticket_id"].Value)).ShowDialog();
                break;
            default:
                MessageBox.Show("Редактирование для этого раздела не реализовано.");
                return;
        }
        LoadCurrent();
    }

    private void DeleteRecord()
    {
        if (grid.CurrentRow == null) return;
        var key = tableSelector.SelectedItem as string;
        if (!_views.TryGetValue(key, out var view)) return;

        int id = Convert.ToInt32(grid.CurrentRow.Cells[view.key].Value);
        if (MessageBox.Show($"Удалить запись #{id}?", "Подтверждение",
                MessageBoxButtons.YesNo) != DialogResult.Yes) return;

        try
        {
            string table = key switch
            {
                "Клиенты" => @"""Клиент""",
                "Сотрудники" => @"""Сотрудник""",
                "Заявки" => @"""Заявка""",
                "Трудозатраты" => @"""Трудозатраты""",
                _ => throw new InvalidOperationException()
            };
            Db.Execute($"DELETE FROM {table} WHERE {view.key} = @id",
                ("@id", id));
            LoadCurrent();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Не удалось удалить (возможно, есть связанные записи):\n" + ex.Message,
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void InitializeComponent()
    {
            this.SuspendLayout();
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "MainForm";
            this.Text = "7";
            this.ResumeLayout(false);

    }
}
