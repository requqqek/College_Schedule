namespace otchet
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            groupComboBox = new ComboBox();
            dayComboBox = new ComboBox();
            tabControl = new TabControl();
            группы = new TabPage();
            преподаватели = new TabPage();
            предметы = new TabPage();
            аудитории = new TabPage();
            tabPage5 = new TabPage();
            showScheduleBtn = new Button();
            addScheduleBtn = new Button();
            refreshBtn = new Button();
            checkConflictsBtn = new Button();
            tabControl.SuspendLayout();
            SuspendLayout();
            // 
            // groupComboBox
            // 
            groupComboBox.FormattingEnabled = true;
            groupComboBox.Location = new Point(21, 22);
            groupComboBox.Name = "groupComboBox";
            groupComboBox.Size = new Size(121, 23);
            groupComboBox.TabIndex = 1;
            // 
            // dayComboBox
            // 
            dayComboBox.FormattingEnabled = true;
            dayComboBox.Location = new Point(21, 51);
            dayComboBox.Name = "dayComboBox";
            dayComboBox.Size = new Size(121, 23);
            dayComboBox.TabIndex = 2;
            // 
            // tabControl
            // 
            tabControl.Controls.Add(группы);
            tabControl.Controls.Add(преподаватели);
            tabControl.Controls.Add(предметы);
            tabControl.Controls.Add(аудитории);
            tabControl.Controls.Add(tabPage5);
            tabControl.Location = new Point(169, 22);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(589, 385);
            tabControl.TabIndex = 3;
            // 
            // группы
            // 
            группы.Location = new Point(4, 24);
            группы.Name = "группы";
            группы.Padding = new Padding(3);
            группы.Size = new Size(581, 357);
            группы.TabIndex = 0;
            группы.Text = "Группы";
            группы.UseVisualStyleBackColor = true;
            группы.Click += tabPage1_Click;
            // 
            // преподаватели
            // 
            преподаватели.Location = new Point(4, 24);
            преподаватели.Name = "преподаватели";
            преподаватели.Padding = new Padding(3);
            преподаватели.Size = new Size(581, 357);
            преподаватели.TabIndex = 1;
            преподаватели.Text = "Преподаватели";
            преподаватели.UseVisualStyleBackColor = true;
            // 
            // предметы
            // 
            предметы.Location = new Point(4, 24);
            предметы.Name = "предметы";
            предметы.Padding = new Padding(3);
            предметы.Size = new Size(581, 357);
            предметы.TabIndex = 2;
            предметы.Text = "Предметы";
            предметы.UseVisualStyleBackColor = true;
            // 
            // аудитории
            // 
            аудитории.Location = new Point(4, 24);
            аудитории.Name = "аудитории";
            аудитории.Padding = new Padding(3);
            аудитории.Size = new Size(581, 357);
            аудитории.TabIndex = 3;
            аудитории.Text = "Аудитории";
            аудитории.UseVisualStyleBackColor = true;
            // 
            // tabPage5
            // 
            tabPage5.Location = new Point(4, 24);
            tabPage5.Name = "tabPage5";
            tabPage5.Padding = new Padding(3);
            tabPage5.Size = new Size(581, 357);
            tabPage5.TabIndex = 4;
            tabPage5.Text = "Расписание (все)";
            tabPage5.UseVisualStyleBackColor = true;
            // 
            // showScheduleBtn
            // 
            showScheduleBtn.Location = new Point(12, 93);
            showScheduleBtn.Name = "showScheduleBtn";
            showScheduleBtn.Size = new Size(136, 39);
            showScheduleBtn.TabIndex = 4;
            showScheduleBtn.Text = "Посмотреть\r\nрассписание";
            showScheduleBtn.UseVisualStyleBackColor = true;
            showScheduleBtn.Click += ShowScheduleBtn_Click;
            // 
            // addScheduleBtn
            // 
            addScheduleBtn.Location = new Point(12, 138);
            addScheduleBtn.Name = "addScheduleBtn";
            addScheduleBtn.Size = new Size(136, 39);
            addScheduleBtn.TabIndex = 5;
            addScheduleBtn.Text = "Добавить \r\nв рассписание\r\n";
            addScheduleBtn.UseVisualStyleBackColor = true;
            addScheduleBtn.Click += AddScheduleBtn_Click;
            // 
            // refreshBtn
            // 
            refreshBtn.Location = new Point(12, 183);
            refreshBtn.Name = "refreshBtn";
            refreshBtn.Size = new Size(136, 23);
            refreshBtn.TabIndex = 6;
            refreshBtn.Text = "Обновить";
            refreshBtn.UseVisualStyleBackColor = true;
            refreshBtn.Click += RefreshBtn_Click;
            // 
            // checkConflictsBtn
            // 
            checkConflictsBtn.BackColor = Color.OrangeRed;
            checkConflictsBtn.ForeColor = Color.White;
            checkConflictsBtn.Location = new Point(12, 212);
            checkConflictsBtn.Name = "checkConflictsBtn";
            checkConflictsBtn.Size = new Size(136, 23);
            checkConflictsBtn.TabIndex = 7;
            checkConflictsBtn.Text = "Проверить конфликты";
            checkConflictsBtn.UseVisualStyleBackColor = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(checkConflictsBtn);
            Controls.Add(showScheduleBtn);
            Controls.Add(refreshBtn);
            Controls.Add(addScheduleBtn);
            Controls.Add(tabControl);
            Controls.Add(dayComboBox);
            Controls.Add(groupComboBox);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            tabControl.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private TabPage группы;
        private TabPage преподаватели;
        private Button showScheduleBtn;
        private Button addScheduleBtn;
        private Button refreshBtn;
        private TabPage предметы;
        private TabPage аудитории;
        private TabPage tabPage5;
        private Button checkConflictsBtn;
    }
}
