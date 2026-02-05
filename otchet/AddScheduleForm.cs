using Microsoft.Data.SqlClient;
using System.Data;


namespace CollegeScheduleApp
{
    public partial class AddScheduleForm : Form
    {
        private DatabaseHelper dbHelper;
        private ComboBox groupCombo;
        private ComboBox subjectCombo;
        private ComboBox teacherCombo;
        private ComboBox classroomCombo;
        private ComboBox dayCombo;
        private ComboBox timeSlotCombo;

        // Словарь для хранения предметов по преподавателям
        private Dictionary<int, List<ComboBoxItem>> teacherSubjectDict = new Dictionary<int, List<ComboBoxItem>>();

        public AddScheduleForm()
        {
            InitializeComponent();
        }

        public AddScheduleForm(DatabaseHelper db)
        {
            dbHelper = db;
            InitializeComponent();
            LoadComboBoxData();
        }

        public void SetDatabaseHelper(DatabaseHelper db)
        {
            dbHelper = db;
        }

        private void InitializeComponent()
        {
            this.Text = "Добавить занятие в расписание";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 7,
                Padding = new Padding(20),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };

            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));

            // Создаем и добавляем элементы управления
            AddControlRow(mainLayout, "Группа:", 0);
            groupCombo = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            mainLayout.Controls.Add(groupCombo, 1, 0);

            AddControlRow(mainLayout, "Преподаватель:", 1);
            teacherCombo = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            mainLayout.Controls.Add(teacherCombo, 1, 1);

            AddControlRow(mainLayout, "Предмет:", 2);
            subjectCombo = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            subjectCombo.Enabled = false; // Отключаем до выбора преподавателя
            mainLayout.Controls.Add(subjectCombo, 1, 2);

            AddControlRow(mainLayout, "Аудитория:", 3);
            classroomCombo = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            mainLayout.Controls.Add(classroomCombo, 1, 3);

            AddControlRow(mainLayout, "День недели:", 4);
            dayCombo = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            mainLayout.Controls.Add(dayCombo, 1, 4);

            AddControlRow(mainLayout, "Время занятия:", 5);
            timeSlotCombo = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            mainLayout.Controls.Add(timeSlotCombo, 1, 5);

            // Кнопки
            Panel buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(10)
            };

            Button addBtn = new Button
            {
                Text = "Добавить",
                Size = new Size(100, 40),
                Location = new Point(150, 10),
                BackColor = Color.Green,
                ForeColor = Color.White
            };

            Button cancelBtn = new Button
            {
                Text = "Отмена",
                Size = new Size(100, 40),
                Location = new Point(260, 10)
            };

            addBtn.Click += AddButton_Click;
            cancelBtn.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            // Добавляем обработчик изменения выбора преподавателя
            teacherCombo.SelectedIndexChanged += TeacherCombo_SelectedIndexChanged;

            buttonPanel.Controls.Add(addBtn);
            buttonPanel.Controls.Add(cancelBtn);

            // Обработчик изменения дня недели
            dayCombo.SelectedIndexChanged += DayCombo_SelectedIndexChanged;

            this.Controls.Add(mainLayout);
            this.Controls.Add(buttonPanel);
        }

        private void AddControlRow(TableLayoutPanel panel, string labelText, int row)
        {
            Label label = new Label
            {
                Text = labelText,
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Fill,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            panel.Controls.Add(label, 0, row);
        }

        private void LoadComboBoxData()
        {
            try
            {
                // Загрузка групп
                DataTable groups = dbHelper.GetAllData("groups");
                foreach (DataRow row in groups.Rows)
                {
                    groupCombo.Items.Add(new ComboBoxItem(
                        row["group_name"].ToString(),
                        Convert.ToInt32(row["group_id"])));
                }

                // Загрузка преподавателей
                DataTable teachers = dbHelper.GetAllData("teachers");
                foreach (DataRow row in teachers.Rows)
                {
                    int teacherId = Convert.ToInt32(row["teacher_id"]);
                    string teacherName = row["full_name"].ToString();

                    teacherCombo.Items.Add(new ComboBoxItem(teacherName, teacherId));

                    // Загружаем предметы для этого преподавателя
                    LoadTeacherSubjects(teacherId, teacherName);
                }

                // Загрузка аудиторий
                DataTable classrooms = dbHelper.GetAllData("classrooms");
                foreach (DataRow row in classrooms.Rows)
                {
                    classroomCombo.Items.Add(new ComboBoxItem(
                        $"{row["building"]} - ауд. {row["room_number"]}",
                        Convert.ToInt32(row["classroom_id"])));
                }

                // Загрузка дней недели
                var days = dbHelper.GetDaysOfWeek();
                foreach (var day in days)
                {
                    dayCombo.Items.Add(day);
                }

                // Устанавливаем значения по умолчанию
                if (groupCombo.Items.Count > 0) groupCombo.SelectedIndex = 0;
                if (teacherCombo.Items.Count > 0) teacherCombo.SelectedIndex = 0;
                if (classroomCombo.Items.Count > 0) classroomCombo.SelectedIndex = 0;
                if (dayCombo.Items.Count > 0) dayCombo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Загружаем предметы для конкретного преподавателя
        private void LoadTeacherSubjects(int teacherId, string teacherName)
        {
            try
            {
                DataTable subjects = GetSubjectsForTeacher(teacherId);
                List<ComboBoxItem> subjectList = new List<ComboBoxItem>();

                foreach (DataRow row in subjects.Rows)
                {
                    subjectList.Add(new ComboBoxItem(
                        row["subject_name"].ToString(),
                        Convert.ToInt32(row["subject_id"])));
                }

                teacherSubjectDict[teacherId] = subjectList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке предметов для преподавателя {teacherName}: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Получаем предметы для преподавателя из базы данных
        public DataTable GetSubjectsForTeacher(int teacherId)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(dbHelper?.GetConnectionString() ?? @"Server=localhost\SQLEXPRESS;Database=CollegeScheduleDB;Trusted_Connection=True;Encrypt=False;"))
            {
                conn.Open();
                string query = @"
            SELECT s.subject_id, s.subject_name 
            FROM subjects s
            JOIN teacher_subjects ts ON s.subject_id = ts.subject_id
            WHERE ts.teacher_id = @teacherId
            ORDER BY s.subject_name";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@teacherId", teacherId);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            return dt;
        }

        // Обработчик выбора преподавателя
        private void TeacherCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (teacherCombo.SelectedItem == null)
            {
                subjectCombo.Items.Clear();
                subjectCombo.Enabled = false;
                return;
            }

            try
            {
                int teacherId = ((ComboBoxItem)teacherCombo.SelectedItem).Id;
                string teacherName = ((ComboBoxItem)teacherCombo.SelectedItem).Text;

                // Очищаем список предметов
                subjectCombo.Items.Clear();

                // Проверяем, есть ли предметы для этого преподавателя
                if (teacherSubjectDict.ContainsKey(teacherId) && teacherSubjectDict[teacherId].Count > 0)
                {
                    // Добавляем предметы этого преподавателя
                    foreach (var subject in teacherSubjectDict[teacherId])
                    {
                        subjectCombo.Items.Add(subject);
                    }

                    subjectCombo.Enabled = true;

                    if (subjectCombo.Items.Count > 0)
                        subjectCombo.SelectedIndex = 0;

                    // Просто выводим информацию в консоль или StatusBar если есть
                    Console.WriteLine($"{teacherName} ведет {subjectCombo.Items.Count} предметов");
                }
                else
                {
                    // Если у преподавателя нет предметов
                    subjectCombo.Items.Add(new ComboBoxItem("У преподавателя нет предметов", -1));
                    subjectCombo.Enabled = false;
                    subjectCombo.SelectedIndex = 0;

                    MessageBox.Show($"Преподаватель {teacherName} не ведет ни одного предмета.\nСначала добавьте предметы в профиле преподавателя.",
                        "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке предметов преподавателя: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DayCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dayCombo.SelectedItem == null || dbHelper == null) return;

            try
            {
                string selectedDay = dayCombo.SelectedItem.ToString();
                timeSlotCombo.Items.Clear();

                DataTable timeSlots = GetTimeSlotsForDay(selectedDay);
                foreach (DataRow row in timeSlots.Rows)
                {
                    string timeText = $"{row["start_time"]} - {row["end_time"]}";
                    timeSlotCombo.Items.Add(new ComboBoxItem(
                        timeText,
                        Convert.ToInt32(row["slot_id"])));
                }

                if (timeSlotCombo.Items.Count > 0)
                    timeSlotCombo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке временных слотов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DataTable GetTimeSlotsForDay(string dayOfWeek)
        {
            DataTable dt = new DataTable();
            using (var conn = new SqlConnection(dbHelper?.GetConnectionString() ?? @"Server=localhost\SQLEXPRESS;Database=CollegeScheduleDB;Trusted_Connection=True;Encrypt=False;"))
            {
                conn.Open();
                string query = "SELECT slot_id, CONVERT(VARCHAR(5), start_time, 108) as start_time, " +
                               "CONVERT(VARCHAR(5), end_time, 108) as end_time " +
                               "FROM time_slots WHERE day_of_week = @day ORDER BY start_time";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@day", dayOfWeek);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            return dt;
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            // Проверка заполнения полей
            if (groupCombo.SelectedItem == null || subjectCombo.SelectedItem == null ||
                teacherCombo.SelectedItem == null || classroomCombo.SelectedItem == null ||
                dayCombo.SelectedItem == null || timeSlotCombo.SelectedItem == null)
            {
                MessageBox.Show("Заполните все поля", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int groupId = ((ComboBoxItem)groupCombo.SelectedItem).Id;
                int subjectId = ((ComboBoxItem)subjectCombo.SelectedItem).Id;
                int teacherId = ((ComboBoxItem)teacherCombo.SelectedItem).Id;
                int classroomId = ((ComboBoxItem)classroomCombo.SelectedItem).Id;
                int slotId = ((ComboBoxItem)timeSlotCombo.SelectedItem).Id;

                // Проверка на валидность предмета
                if (subjectId == -1)
                {
                    MessageBox.Show("Выберите действительный предмет", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Проверяем ограничения
                string validationError = dbHelper.ValidateScheduleItem(groupId, subjectId, teacherId, classroomId, slotId);

                if (validationError != null)
                {
                    // Предлагаем создать совмещенное занятие
                    if (validationError.Contains("уже ведет другой предмет"))
                    {
                        var result = MessageBox.Show($"Преподаватель уже ведет занятие в это время.\n" +
                            "Хотите создать совмещенное занятие? (Преподаватель будет вести один предмет у нескольких групп)",
                            "Совмещенное занятие", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (result == DialogResult.No)
                        {
                            return;
                        }
                        // Если Да - ПРОВЕРЯЕМ, можно ли создать совмещение
                        // Нужна дополнительная проверка, что преподаватель ведет ТОТ ЖЕ предмет

                        // Проверяем, какой предмет уже ведет преподаватель в это время
                        if (!dbHelper.IsTeacherTeachingSameSubject(teacherId, slotId, subjectId))
                        {
                            MessageBox.Show("Нельзя создать совмещенное занятие: преподаватель ведет другой предмет в это время",
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        // Также проверяем другие ограничения для совмещенного занятия
                        if (!dbHelper.CanTeacherHaveCombinedLesson(teacherId, slotId))
                        {
                            MessageBox.Show("Преподаватель уже ведет максимальное количество групп (3) в это время",
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Невозможно добавить занятие:\n{validationError}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                dbHelper.AddScheduleItem(groupId, subjectId, teacherId, classroomId, slotId);

                MessageBox.Show("Занятие успешно добавлено в расписание", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении занятия: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Вспомогательный класс для ComboBox
        private class ComboBoxItem
        {
            public string Text { get; set; }
            public int Id { get; set; }

            public ComboBoxItem(string text, int id)
            {
                Text = text;
                Id = id;
            }

            public override string ToString()
            {
                return Text;
            }
        }
    }
}