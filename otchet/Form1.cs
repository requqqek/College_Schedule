using CollegeScheduleApp;
using System.Data;
using Microsoft.Data.SqlClient;

namespace otchet
{
    public partial class Form1 : Form
    {
        private DatabaseHelper dbHelper;
        private ComboBox groupComboBox;
        private ComboBox dayComboBox;
        private TabControl tabControl;

        public Form1()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();

            showScheduleBtn.Click += ShowScheduleBtn_Click;
            addScheduleBtn.Click += AddScheduleBtn_Click;
            refreshBtn.Click += RefreshBtn_Click;
            checkConflictsBtn.Click += CheckConflictsBtn_Click;
            tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;

            LoadComboBoxes();
        }

        private void LoadComboBoxes()
        {
            try
            {
                // Загрузка групп
                DataTable groups = dbHelper.GetGroups();
                groupComboBox.Items.Clear();
                foreach (DataRow row in groups.Rows)
                {
                    groupComboBox.Items.Add(row["group_name"]);
                }
                if (groupComboBox.Items.Count > 0)
                    groupComboBox.SelectedIndex = 0;

                // Загрузка дней недели
                var days = dbHelper.GetDaysOfWeek();
                dayComboBox.Items.Clear();
                foreach (var day in days)
                {
                    dayComboBox.Items.Add(day);
                }
                if (dayComboBox.Items.Count > 0)
                    dayComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowScheduleBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (groupComboBox.SelectedItem == null || dayComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите группу и день недели", "Предупреждение",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string groupName = groupComboBox.SelectedItem.ToString();
                string dayOfWeek = dayComboBox.SelectedItem.ToString();

                DataTable schedule = dbHelper.GetSchedule(groupName, dayOfWeek);

                // Открываем новую форму с расписанием
                ScheduleViewForm scheduleForm = new ScheduleViewForm(schedule, groupName, dayOfWeek);
                scheduleForm.ShowDialog(); // или Show() если хотите немодально

                // Не нужно очищать DataGrid или скрывать элементы - форма закрывается самостоятельно
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении расписания: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddScheduleBtn_Click(object sender, EventArgs e)
        {
            AddScheduleForm addForm = new AddScheduleForm(dbHelper); // Используйте конструктор с параметром
            if (addForm.ShowDialog() == DialogResult.OK) // Используйте ShowDialog
            {
                LoadComboBoxes();
                LoadTabData(tabControl.SelectedTab);
            }
        }

        private void RefreshBtn_Click(object sender, EventArgs e)
        {
            LoadComboBoxes();
            LoadTabData(tabControl.SelectedTab);
            MessageBox.Show("Данные обновлены", "Информация",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTabData(tabControl.SelectedTab);
        }

        private void LoadTabData(TabPage selectedTab)
        {
            if (selectedTab == null) return;

            DataGridView grid = null;

            // Проверяем, есть ли уже DataGridView на вкладке
            if (selectedTab.Controls.Count > 0 && selectedTab.Controls[0] is DataGridView)
            {
                grid = (DataGridView)selectedTab.Controls[0];
            }
            else
            {
                grid = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    AllowUserToAddRows = false,
                    ReadOnly = selectedTab.Text != "Расписание (все)",
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
                };

                // УДАЛИТЕ все старые элементы с вкладки
                selectedTab.Controls.Clear();

                // Добавляем кнопки для редактирования только на вкладках справочников
                if (selectedTab.Text != "Расписание (все)")
                {
                    Panel buttonPanel = new Panel
                    {
                        Dock = DockStyle.Bottom,
                        Height = 40
                    };

                    Button addBtn = new Button
                    {
                        Text = "Добавить",
                        Location = new Point(10, 5),
                        Size = new Size(80, 30)
                    };

                    Button deleteBtn = new Button
                    {
                        Text = "Удалить",
                        Location = new Point(100, 5),
                        Size = new Size(80, 30),
                        BackColor = Color.OrangeRed,
                        ForeColor = Color.White
                    };

                    addBtn.Click += (s, ev) => ShowAddDialog(selectedTab.Text);
                    deleteBtn.Click += (s, ev) => DeleteSelectedRecord(grid, selectedTab.Text);

                    buttonPanel.Controls.Add(addBtn);
                    buttonPanel.Controls.Add(deleteBtn);

                    selectedTab.Controls.Add(grid);     // Сначала DataGridView
                    selectedTab.Controls.Add(buttonPanel); // Затем панель с кнопками
                }
                else
                {
                    selectedTab.Controls.Add(grid); // Только DataGridView для полного расписания
                }
            }

            // Загружаем данные в зависимости от вкладки
            try
            {
                switch (selectedTab.Text)
                {
                    case "Группы":
                        grid.DataSource = dbHelper.GetAllData("groups");
                        break;
                    case "Преподаватели":
                        grid.DataSource = dbHelper.GetAllData("teachers");
                        break;
                    case "Предметы":
                        grid.DataSource = dbHelper.GetAllData("subjects");
                        break;
                    case "Аудитории":
                        grid.DataSource = dbHelper.GetAllData("classrooms");
                        break;
                    case "Расписание (все)":
                        // Специальный запрос для полного расписания
                        DataTable fullSchedule = GetFullSchedule();
                        grid.DataSource = fullSchedule;
                        break;
                    default:
                        // Если вкладка имеет стандартное имя, переименуем ее
                        RenameDefaultTab(selectedTab);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Метод для переименования стандартных вкладок
        private void RenameDefaultTab(TabPage tab)
        {
            // Определяем порядковый номер вкладки
            int tabIndex = tabControl.TabPages.IndexOf(tab);

            // Массив правильных названий
            string[] correctNames = { "Группы", "Преподаватели", "Предметы", "Аудитории", "Расписание (все)" };

            if (tabIndex < correctNames.Length)
            {
                tab.Text = correctNames[tabIndex];
                LoadTabData(tab); // Перезагружаем данные с новым именем
            }
        }

        private DataTable GetFullSchedule()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
            {
                conn.Open();
                string query = @"
                    SELECT 
                        g.group_name,
                        ts.day_of_week,
                        FORMAT(ts.start_time, 'hh\:mm') as start_time,
                        FORMAT(ts.end_time, 'hh\:mm') as end_time,
                        sub.subject_name,
                        t.full_name AS teacher_name,
                        c.building + ' - ауд. ' + c.room_number as аудитория
                    FROM schedule s
                    JOIN time_slots ts ON s.slot_id = ts.slot_id
                    JOIN subjects sub ON s.subject_id = sub.subject_id
                    JOIN teachers t ON s.teacher_id = t.teacher_id
                    JOIN classrooms c ON s.classroom_id = c.classroom_id
                    JOIN groups g ON s.group_id = g.group_id
                    ORDER BY g.group_name, ts.day_of_week, ts.start_time";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(dt);
                }
            }
            return dt;
        }

        private void ShowAddDialog(string tableName)
        {
            // Для преподавателей используем специальную форму
            if (tableName == "Преподаватели")
            {
                ShowAddTeacherDialog();
                return;
            }

            Form dialog = new Form
            {
                Text = $"Добавить запись в {tableName}",
                Size = new Size(400, 200),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(15),
                AutoSize = true
            };

            TextBox textBox1 = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 5, 0, 5)
            };

            TextBox textBox2 = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 5, 0, 5)
            };

            Button okBtn = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Size = new Size(80, 30)
            };

            Button cancelBtn = new Button
            {
                Text = "Отмена",
                DialogResult = DialogResult.Cancel,
                Size = new Size(80, 30)
            };

            switch (tableName)
            {
                case "Группы":
                    dialog.Size = new Size(300, 150);
                    layout.RowCount = 2;
                    layout.Controls.Add(new Label
                    {
                        Text = "Название группы:",
                        TextAlign = ContentAlignment.MiddleRight,
                        Margin = new Padding(0, 5, 0, 5)
                    }, 0, 0);
                    layout.Controls.Add(textBox1, 1, 0);
                    break;

                case "Предметы":
                    dialog.Size = new Size(400, 180);
                    layout.RowCount = 3;
                    layout.Controls.Add(new Label
                    {
                        Text = "Название предмета:",
                        TextAlign = ContentAlignment.MiddleRight,
                        Margin = new Padding(0, 5, 0, 5)
                    }, 0, 0);
                    layout.Controls.Add(textBox1, 1, 0);
                    layout.Controls.Add(new Label
                    {
                        Text = "Часов в семестре:",
                        TextAlign = ContentAlignment.MiddleRight,
                        Margin = new Padding(0, 5, 0, 5)
                    }, 0, 1);
                    layout.Controls.Add(textBox2, 1, 1);
                    break;

                case "Аудитории":
                    dialog.Size = new Size(400, 180);
                    layout.RowCount = 3;
                    layout.Controls.Add(new Label
                    {
                        Text = "Номер аудитории:",
                        TextAlign = ContentAlignment.MiddleRight,
                        Margin = new Padding(0, 5, 0, 5)
                    }, 0, 0);
                    layout.Controls.Add(textBox1, 1, 0);
                    layout.Controls.Add(new Label
                    {
                        Text = "Корпус:",
                        TextAlign = ContentAlignment.MiddleRight,
                        Margin = new Padding(0, 5, 0, 5)
                    }, 0, 1);
                    layout.Controls.Add(textBox2, 1, 1);
                    break;
            }

            // Добавляем кнопки
            FlowLayoutPanel buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(10)
            };

            buttonPanel.Controls.Add(okBtn);
            buttonPanel.Controls.Add(cancelBtn);

            dialog.Controls.Add(layout);
            dialog.Controls.Add(buttonPanel);

            // Фокусировка на первом поле
            dialog.Shown += (s, e) => textBox1.Focus();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    switch (tableName)
                    {
                        case "Группы":
                            if (string.IsNullOrWhiteSpace(textBox1.Text))
                            {
                                MessageBox.Show("Введите название группы", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                            dbHelper.AddGroup(textBox1.Text.Trim());
                            break;

                        case "Предметы":
                            if (string.IsNullOrWhiteSpace(textBox1.Text))
                            {
                                MessageBox.Show("Введите название предмета", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                            if (!int.TryParse(textBox2.Text, out int hours) || hours <= 0)
                            {
                                MessageBox.Show("Введите корректное количество часов", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                            dbHelper.AddSubject(textBox1.Text.Trim(), hours);
                            break;

                        case "Аудитории":
                            if (string.IsNullOrWhiteSpace(textBox1.Text))
                            {
                                MessageBox.Show("Введите номер аудитории", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                            dbHelper.AddClassroom(textBox1.Text.Trim(), textBox2.Text?.Trim() ?? "");
                            break;
                    }

                    LoadTabData(tabControl.SelectedTab);
                    LoadComboBoxes();

                    MessageBox.Show("Запись успешно добавлена", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ShowAddTeacherDialog()
        {
            Form dialog = new Form
            {
                Text = "Добавить преподавателя",
                Size = new Size(500, 400),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(15)
            };

            // Поле для ФИО
            TextBox nameTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 5, 0, 5)
            };

            // Список предметов
            CheckedListBox subjectsListBox = new CheckedListBox
            {
                Dock = DockStyle.Fill,
                CheckOnClick = true,
                Margin = new Padding(0, 5, 0, 5)
            };

            // Загружаем все предметы из базы данных
            try
            {
                DataTable subjects = dbHelper.GetAllData("subjects");
                foreach (DataRow row in subjects.Rows)
                {
                    subjectsListBox.Items.Add(row["subject_name"].ToString(), false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке предметов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            mainLayout.Controls.Add(new Label
            {
                Text = "ФИО преподавателя:",
                TextAlign = ContentAlignment.MiddleRight,
                Margin = new Padding(0, 5, 0, 5)
            }, 0, 0);

            mainLayout.Controls.Add(nameTextBox, 1, 0);

            mainLayout.Controls.Add(new Label
            {
                Text = "Предметы (макс. 3):",
                TextAlign = ContentAlignment.MiddleRight,
                Margin = new Padding(0, 5, 0, 5)
            }, 0, 1);

            mainLayout.Controls.Add(subjectsListBox, 1, 1);

            // Метка для отображения количества выбранных предметов
            Label countLabel = new Label
            {
                Text = "Выбрано: 0/3",
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = Color.DarkBlue,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };

            mainLayout.Controls.Add(countLabel, 0, 2);

            // Обработчик изменения выбора предметов
            subjectsListBox.ItemCheck += (s, e) =>
            {
                int selectedCount = subjectsListBox.CheckedItems.Count;

                // Если пытаются выбрать предмет при уже 3 выбранных
                if (e.NewValue == CheckState.Checked && selectedCount >= 3)
                {
                    e.NewValue = CheckState.Unchecked;
                    MessageBox.Show("Можно выбрать не более 3 предметов", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Обновляем счетчик
                int newCount = (e.NewValue == CheckState.Checked) ? selectedCount + 1 : selectedCount - 1;
                countLabel.Text = $"Выбрано: {newCount}/3";

                // Меняем цвет если достигнут лимит
                if (newCount >= 3)
                    countLabel.ForeColor = Color.Red;
                else
                    countLabel.ForeColor = Color.DarkBlue;
            };

            // Кнопки
            FlowLayoutPanel buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(10)
            };

            Button okBtn = new Button
            {
                Text = "OK",
                Size = new Size(80, 30),
                DialogResult = DialogResult.OK
            };

            Button cancelBtn = new Button
            {
                Text = "Отмена",
                Size = new Size(80, 30),
                DialogResult = DialogResult.Cancel
            };

            okBtn.Click += (s, e) =>
            {
                // Проверка ФИО
                if (string.IsNullOrWhiteSpace(nameTextBox.Text))
                {
                    MessageBox.Show("Введите ФИО преподавателя", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    nameTextBox.Focus();
                    return;
                }

                // Проверка количества предметов
                int selectedCount = subjectsListBox.CheckedItems.Count;
                if (selectedCount == 0)
                {
                    if (MessageBox.Show("Преподаватель не ведет ни одного предмета. Продолжить?",
                        "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    {
                        return;
                    }
                }

                try
                {
                    // Добавляем преподавателя
                    int teacherId = dbHelper.AddTeacher(nameTextBox.Text.Trim());

                    if (teacherId <= 0)
                    {
                        MessageBox.Show("Не удалось добавить преподавателя", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Добавляем выбранные предметы
                    foreach (var item in subjectsListBox.CheckedItems)
                    {
                        string subjectName = item.ToString();
                        dbHelper.AddTeacherSubject(teacherId, subjectName);
                    }

                    // Обновляем интерфейс
                    LoadTabData(tabControl.SelectedTab);
                    LoadComboBoxes();

                    MessageBox.Show("Преподаватель успешно добавлен", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            buttonPanel.Controls.Add(okBtn);
            buttonPanel.Controls.Add(cancelBtn);

            dialog.Controls.Add(mainLayout);
            dialog.Controls.Add(buttonPanel);

            // Фокусировка на поле ФИО
            dialog.Shown += (sender, e) => nameTextBox.Focus();

            dialog.ShowDialog();
        }

        private void DeleteSelectedRecord(DataGridView grid, string tableName)
        {
            if (grid.CurrentRow == null)
            {
                MessageBox.Show("Выберите запись для удаления", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Удалить выбранную запись?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    int id = Convert.ToInt32(grid.CurrentRow.Cells[0].Value);
                    dbHelper.DeleteRecord(tableName.ToLower(), id);
                    LoadTabData(tabControl.SelectedTab);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CheckConflictsBtn_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable conflicts = GetScheduleConflicts();

                if (conflicts.Rows.Count == 0)
                {
                    MessageBox.Show("Конфликтов в расписании не найдено", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Создаем форму для отображения конфликтов
                    Form conflictsForm = new Form
                    {
                        Text = "Обнаруженные конфликты в расписании",
                        Size = new Size(1000, 400),
                        StartPosition = FormStartPosition.CenterParent
                    };

                    DataGridView dgv = new DataGridView
                    {
                        Dock = DockStyle.Fill,
                        DataSource = conflicts,
                        ReadOnly = true,
                        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
                    };

                    conflictsForm.Controls.Add(dgv);
                    conflictsForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проверке конфликтов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DataTable GetScheduleConflicts()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Тип конфликта");
            dt.Columns.Add("Группа");
            dt.Columns.Add("День");
            dt.Columns.Add("Время");
            dt.Columns.Add("Детали");

            using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
            {
                conn.Open();

                // 1. Проверка: группы, которые занимаются в одно время в разных местах
                string query1 = @"
            SELECT DISTINCT 
                'Группа в двух местах одновременно' as conflict_type,
                g1.group_name,
                ts.day_of_week,
                FORMAT(ts.start_time, 'hh\:mm') + ' - ' + FORMAT(ts.end_time, 'hh\:mm') as time_slot,
                'Группа ' + g1.group_name + ' имеет несколько занятий в ' + 
                ts.day_of_week + ' с ' + FORMAT(ts.start_time, 'hh\:mm') as details
            FROM schedule s1
            JOIN schedule s2 ON s1.group_id = s2.group_id 
                AND s1.slot_id = s2.slot_id 
                AND s1.schedule_id != s2.schedule_id
            JOIN groups g1 ON s1.group_id = g1.group_id
            JOIN time_slots ts ON s1.slot_id = ts.slot_id
            GROUP BY g1.group_name, ts.day_of_week, ts.start_time, ts.end_time";

                // 2. Проверка: переполненные кабинеты (больше 3 групп)
                string query2 = @"
            SELECT 
                'Переполненный кабинет' as conflict_type,
                'Несколько групп' as group_name,
                ts.day_of_week,
                FORMAT(ts.start_time, 'hh\:mm') + ' - ' + FORMAT(ts.end_time, 'hh\:mm') as time_slot,
                'Кабинет ' + c.building + ' - ' + c.room_number + 
                ' содержит ' + CAST(COUNT(*) as VARCHAR) + ' групп(ы)' as details
            FROM schedule s
            JOIN time_slots ts ON s.slot_id = ts.slot_id
            JOIN classrooms c ON s.classroom_id = c.classroom_id
            GROUP BY s.slot_id, s.classroom_id, ts.day_of_week, ts.start_time, ts.end_time, c.building, c.room_number
            HAVING COUNT(*) > 3";

                string query3 = @"
            SELECT DISTINCT 
                'Группа разделена в расписании' as conflict_type,
                g.group_name,
                ts.day_of_week,
                FORMAT(ts.start_time, 'hh\:mm') + ' - ' + FORMAT(ts.end_time, 'hh\:mm') as time_slot,
                'Группа ' + g.group_name + ' имеет разные занятия в ' + 
                ts.day_of_week + ' с ' + FORMAT(ts.start_time, 'hh\:mm') as details
            FROM schedule s1
            JOIN schedule s2 ON s1.group_id = s2.group_id 
                AND s1.slot_id = s2.slot_id 
                AND s1.schedule_id != s2.schedule_id
                AND (s1.teacher_id != s2.teacher_id 
                    OR s1.subject_id != s2.subject_id 
                    OR s1.classroom_id != s2.classroom_id)
            JOIN groups g ON s1.group_id = g.group_id
            JOIN time_slots ts ON s1.slot_id = ts.slot_id
            GROUP BY g.group_name, ts.day_of_week, ts.start_time, ts.end_time";

                // 4. Проверка: преподаватель ведет разные предметы у разных групп в одно время
                string query4 = @"
    SELECT DISTINCT 
        'Преподаватель ведет разные предметы' as conflict_type,
        t.full_name as group_name,
        ts.day_of_week,
        FORMAT(ts.start_time, 'hh\:mm') + ' - ' + FORMAT(ts.end_time, 'hh\:mm') as time_slot,
        'Преподаватель ' + t.full_name + ' ведет разные предметы в ' + 
        ts.day_of_week + ' с ' + FORMAT(ts.start_time, 'hh\:mm') as details
    FROM schedule s1
    JOIN schedule s2 ON s1.teacher_id = s2.teacher_id 
        AND s1.slot_id = s2.slot_id 
        AND s1.schedule_id != s2.schedule_id
        AND s1.subject_id != s2.subject_id
    JOIN teachers t ON s1.teacher_id = t.teacher_id
    JOIN time_slots ts ON s1.slot_id = ts.slot_id
    GROUP BY t.full_name, ts.day_of_week, ts.start_time, ts.end_time";

                // Добавляем результаты в DataTable
                AddQueryResultsToTable(conn, query1, dt);
                AddQueryResultsToTable(conn, query2, dt);
                AddQueryResultsToTable(conn, query3, dt);

            }

            return dt;
        }

        private void AddQueryResultsToTable(SqlConnection conn, string query, DataTable dt)
        {
            using (SqlCommand cmd = new SqlCommand(query, conn))
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    dt.Rows.Add(
                        reader["conflict_type"].ToString(),
                        reader["group_name"].ToString(),
                        reader["day_of_week"].ToString(),
                        reader["time_slot"].ToString(),
                        reader["details"].ToString()
                    );
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

    }
}