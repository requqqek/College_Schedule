using System.Data;


namespace otchet
{
    public partial class ScheduleViewForm : Form
    {
        private DataTable scheduleData;
        private string groupName;
        private string dayOfWeek;

        public ScheduleViewForm(DataTable schedule, string groupName, string dayOfWeek)
        {
            InitializeComponent();
            this.scheduleData = schedule;
            this.groupName = groupName;
            this.dayOfWeek = dayOfWeek;
            InitializeScheduleView();
        }

        private void InitializeScheduleView()
        {
            // Настройки формы
            this.Text = $"Расписание: {groupName} - {dayOfWeek}";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Панель заголовка
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.SteelBlue,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label titleLabel = new Label
            {
                Text = $"Расписание группы: {groupName} | День: {dayOfWeek}",
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // DataGridView для расписания
            DataGridView scheduleGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                DataSource = scheduleData
            };

            // Настройка колонок
            scheduleGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.Navy;
            scheduleGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            scheduleGrid.EnableHeadersVisualStyles = false;

            // Панель с кнопками
            Panel buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.LightGray
            };

            Button backBtn = new Button
            {
                Text = "← Назад",
                Size = new Size(100, 35),
                Location = new Point(10, 8),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                DialogResult = DialogResult.Cancel
            };
            backBtn.Click += (s, e) => this.Hide();

            Button printBtn = new Button
            {
                Text = "Печать",
                Size = new Size(100, 35),
                Location = new Point(120, 8),
                BackColor = Color.SteelBlue,
                ForeColor = Color.White
            };
            printBtn.Click += PrintBtn_Click;

            Button exportBtn = new Button
            {
                Text = "Экспорт",
                Size = new Size(100, 35),
                Location = new Point(230, 8),
                BackColor = Color.Green,
                ForeColor = Color.White
            };
            exportBtn.Click += ExportBtn_Click;

            buttonPanel.Controls.AddRange(new Control[] { backBtn, printBtn, exportBtn });

            // Добавление элементов на форму
            headerPanel.Controls.Add(titleLabel);
            this.Controls.AddRange(new Control[] { scheduleGrid, buttonPanel, headerPanel });

            // Если данных нет, показываем сообщение
            if (scheduleData.Rows.Count == 0)
            {
                Label noDataLabel = new Label
                {
                    Text = $"Расписание для группы {groupName} на {dayOfWeek} не найдено",
                    Font = new Font("Arial", 10, FontStyle.Bold),
                    ForeColor = Color.Red,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                this.Controls.Add(noDataLabel);
                noDataLabel.BringToFront();
            }
        }

        private void PrintBtn_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Функция печати будет реализована позже", "Информация",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ExportBtn_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Функция экспорта будет реализована позже", "Информация",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}