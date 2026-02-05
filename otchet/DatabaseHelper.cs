using System.Data;
using Microsoft.Data.SqlClient;

namespace CollegeScheduleApp
{
    public class DatabaseHelper
    {
        private string connectionString = @"Server=CMPUTER228\SQLEXPRESS;Database=CollegeScheduleDB;Trusted_Connection=True;Encrypt=False;";

        public string GetConnectionString()
        {
            return connectionString; 
        }

        public DatabaseHelper()
        {
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            try
            {
                string createTeacherSubjectsTable = @"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='teacher_subjects' AND xtype='U')
                    CREATE TABLE teacher_subjects (
                        teacher_subject_id INT IDENTITY(1,1) PRIMARY KEY,
                        teacher_id INT NOT NULL,
                        subject_id INT NOT NULL,
                        FOREIGN KEY (teacher_id) REFERENCES teachers(teacher_id) ON DELETE CASCADE,
                        FOREIGN KEY (subject_id) REFERENCES subjects(subject_id) ON DELETE CASCADE,
                        UNIQUE (teacher_id, subject_id) -- один преподаватель не может вести один предмет дважды
                    )";
                // Создаем базу данных, если она не существует
                string masterConnectionString = @"Server=CMPUTER228\SQLEXPRESS;Database=master;Trusted_Connection=True;Encrypt=False;";

                using (SqlConnection masterConn = new SqlConnection(masterConnectionString))
                {
                    masterConn.Open();

                    string checkDbQuery = @"
                    IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'CollegeScheduleDB')
                    BEGIN
                        CREATE DATABASE CollegeScheduleDB;
                    END";

                    using (SqlCommand cmd = new SqlCommand(checkDbQuery, masterConn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                // Создаем таблицы в новой базе данных
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Таблица групп
                    string createGroupsTable = @"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='groups' AND xtype='U')
                    CREATE TABLE groups (
                        group_id INT IDENTITY(1,1) PRIMARY KEY,
                        group_name VARCHAR(20) NOT NULL UNIQUE
                    )";

                    // Таблица преподавателей
                    string createTeachersTable = @"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='teachers' AND xtype='U')
                    CREATE TABLE teachers (
                        teacher_id INT IDENTITY(1,1) PRIMARY KEY,
                        full_name VARCHAR(100) NOT NULL,
                        department VARCHAR(100)
                    )";

                    // Таблица предметов
                    string createSubjectsTable = @"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='subjects' AND xtype='U')
                    CREATE TABLE subjects (
                        subject_id INT IDENTITY(1,1) PRIMARY KEY,
                        subject_name VARCHAR(100) NOT NULL,
                        hours_per_semester INT
                    )";

                    // Таблица аудиторий
                    string createClassroomsTable = @"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='classrooms' AND xtype='U')
                    CREATE TABLE classrooms (
                        classroom_id INT IDENTITY(1,1) PRIMARY KEY,
                        room_number VARCHAR(10) NOT NULL,
                        building VARCHAR(20)
                    )";

                    // Таблица временных слотов
                    string createTimeSlotsTable = @"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='time_slots' AND xtype='U')
                    CREATE TABLE time_slots (
                        slot_id INT IDENTITY(1,1) PRIMARY KEY,
                        day_of_week VARCHAR(15) NOT NULL,
                        start_time TIME NOT NULL,
                        end_time TIME NOT NULL
                    )";

                    // Таблица расписания
                    string createScheduleTable = @"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='schedule' AND xtype='U')
                    CREATE TABLE schedule (
                        schedule_id INT IDENTITY(1,1) PRIMARY KEY,
                        group_id INT,
                        subject_id INT,
                        teacher_id INT,
                        classroom_id INT,
                        slot_id INT,
                        FOREIGN KEY (group_id) REFERENCES groups(group_id),
                        FOREIGN KEY (subject_id) REFERENCES subjects(subject_id),
                        FOREIGN KEY (teacher_id) REFERENCES teachers(teacher_id),
                        FOREIGN KEY (classroom_id) REFERENCES classrooms(classroom_id),
                        FOREIGN KEY (slot_id) REFERENCES time_slots(slot_id)
                    )";

                  

                    // Выполняем создание таблиц
                    ExecuteNonQuery(conn, createGroupsTable);
                    ExecuteNonQuery(conn, createTeachersTable);
                    ExecuteNonQuery(conn, createSubjectsTable);
                    ExecuteNonQuery(conn, createClassroomsTable);
                    ExecuteNonQuery(conn, createTimeSlotsTable);
                    ExecuteNonQuery(conn, createScheduleTable);
                    ExecuteNonQuery(conn, createTeacherSubjectsTable);

                    // Добавляем тестовые данные
                    AddTestData(conn);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при инициализации базы данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExecuteNonQuery(SqlConnection conn, string query)
        {
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private void AddTestData(SqlConnection conn)
        {
            string checkData = "SELECT COUNT(*) FROM time_slots";
            using (SqlCommand cmd = new SqlCommand(checkData, conn))
            {
                int count = (int)cmd.ExecuteScalar();
                if (count == 0)
                {
                    string[] days = { "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота" };
                    string[] startTimes = { "08:30", "10:10", "12:00", "13:40", "15:20" };
                    string[] endTimes = { "10:00", "11:40", "13:30", "15:10", "16:50" };

                    foreach (var day in days)
                    {
                        for (int i = 0; i < startTimes.Length; i++)
                        {
                            string insert = @"
                            INSERT INTO time_slots (day_of_week, start_time, end_time) 
                            VALUES (@day, @start, @end)";

                            using (SqlCommand cmdInsert = new SqlCommand(insert, conn))
                            {
                                cmdInsert.Parameters.AddWithValue("@day", day);
                                cmdInsert.Parameters.AddWithValue("@start", startTimes[i]);
                                cmdInsert.Parameters.AddWithValue("@end", endTimes[i]);
                                cmdInsert.ExecuteNonQuery();
                            }
                        }
                    }

                    string[] testGroups = { "П-401", "П-402", "П-403", "ИСП-401", "ИСП-402" };
                    foreach (var group in testGroups)
                    {
                        string insertGroup = @"
                        IF NOT EXISTS (SELECT * FROM groups WHERE group_name = @name)
                        INSERT INTO groups (group_name) VALUES (@name)";

                        using (SqlCommand cmdGroup = new SqlCommand(insertGroup, conn))
                        {
                            cmdGroup.Parameters.AddWithValue("@name", group);
                            cmdGroup.ExecuteNonQuery();
                        }
                    }

                    string insertTeachers = @"
                    IF NOT EXISTS (SELECT * FROM teachers WHERE full_name = 'Иванов И.И.')
                    INSERT INTO teachers (full_name, department) VALUES 
                    ('Иванов И.И.', 'Программирование'),
                    ('Петрова А.С.', 'Математика'),
                    ('Сидоров В.В.', 'Базы данных'),
                    ('Кузнецова Е.П.', 'Сети')";
                    ExecuteNonQuery(conn, insertTeachers);

                    string insertSubjects = @"
                    IF NOT EXISTS (SELECT * FROM subjects WHERE subject_name = 'Программирование')
                    INSERT INTO subjects (subject_name, hours_per_semester) VALUES 
                    ('Программирование', 120),
                    ('Базы данных', 90),
                    ('Математика', 150),
                    ('Сетевые технологии', 80)";
                    ExecuteNonQuery(conn, insertSubjects);

                    string insertClassrooms = @"
                    IF NOT EXISTS (SELECT * FROM classrooms WHERE room_number = '101')
                    INSERT INTO classrooms (room_number, building) VALUES 
                    ('101', 'Главный корпус'),
                    ('102', 'Главный корпус'),
                    ('201', 'Главный корпус'),
                    ('301', 'Главный корпус'),
                    ('401', 'Учебный корпус')";
                    ExecuteNonQuery(conn, insertClassrooms);
                }
            }
        }

        public DataTable GetSchedule(string groupName, string dayOfWeek)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    SELECT 
                        ts.day_of_week,
                        FORMAT(ts.start_time, 'hh\:mm') as start_time,
                        FORMAT(ts.end_time, 'hh\:mm') as end_time,
                        sub.subject_name,
                        t.full_name AS teacher_name,
                        c.room_number,
                        c.building
                    FROM schedule s
                    JOIN time_slots ts ON s.slot_id = ts.slot_id
                    JOIN subjects sub ON s.subject_id = sub.subject_id
                    JOIN teachers t ON s.teacher_id = t.teacher_id
                    JOIN classrooms c ON s.classroom_id = c.classroom_id
                    JOIN groups g ON s.group_id = g.group_id
                    WHERE g.group_name = @groupName 
                      AND ts.day_of_week = @dayOfWeek
                    ORDER BY ts.start_time";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@groupName", groupName);
                    cmd.Parameters.AddWithValue("@dayOfWeek", dayOfWeek);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }

            return dt;
        }

        public DataTable GetGroups()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT group_name FROM groups ORDER BY group_name";
                using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                {
                    adapter.Fill(dt);
                }
            }
            return dt;
        }

        public List<string> GetDaysOfWeek()
        {
            List<string> days = new List<string>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                // Просто получим дни и отсортируем в коде C#
                string query = "SELECT DISTINCT day_of_week FROM time_slots";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        days.Add(reader["day_of_week"].ToString());
                    }
                }
            }

            var dayOrder = new Dictionary<string, int>
    {
        { "Понедельник", 1 },
        { "Вторник", 2 },
        { "Среда", 3 },
        { "Четверг", 4 },
        { "Пятница", 5 },
        { "Суббота", 6 },
        { "Воскресенье", 7 }
    };

            days.Sort((a, b) =>
            {
                dayOrder.TryGetValue(a, out int aVal);
                dayOrder.TryGetValue(b, out int bVal);
                return aVal.CompareTo(bVal);
            });

            return days;
        }

        public DataTable GetAllData(string tableName)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                if (tableName.ToLower() == "teachers")
                {
                    // Для преподавателей показываем дисциплины
                    string query = @"
                SELECT 
                    t.teacher_id,
                    t.full_name,
                    STRING_AGG(s.subject_name, ', ') as departments
                FROM teachers t
                LEFT JOIN teacher_subjects ts ON t.teacher_id = ts.teacher_id
                LEFT JOIN subjects s ON ts.subject_id = s.subject_id
                GROUP BY t.teacher_id, t.full_name
                ORDER BY t.full_name";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                    {
                        adapter.Fill(dt);
                    }
                }
                else
                {
                    string query = $"SELECT * FROM {tableName}";
                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            return dt;
        }

        // CRUD операции
        public int AddGroup(string groupName)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO groups (group_name) VALUES (@name); SELECT SCOPE_IDENTITY();";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", groupName);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public int AddTeacher(string fullName)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    string checkQuery = "SELECT teacher_id FROM teachers WHERE full_name = @name";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn, transaction))
                    {
                        checkCmd.Parameters.AddWithValue("@name", fullName);
                        var existingId = checkCmd.ExecuteScalar();

                        if (existingId != null)
                        {
                            transaction.Rollback();
                            MessageBox.Show($"Преподаватель '{fullName}' уже существует в базе (ID: {existingId}).",
                                "Дублирование", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return Convert.ToInt32(existingId);
                        }
                    }

                    string query = "INSERT INTO teachers (full_name) VALUES (@name); SELECT SCOPE_IDENTITY();";
                    int teacherId;

                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@name", fullName);
                        teacherId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    transaction.Commit();
                    return teacherId;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"Ошибка при добавлении преподавателя: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return -1;
                }
            }
        }

        private int GetOrCreateSubjectId(string subjectName, SqlConnection conn, SqlTransaction transaction)
        {
            string checkQuery = "SELECT subject_id FROM subjects WHERE subject_name = @name";
            using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn, transaction))
            {
                checkCmd.Parameters.AddWithValue("@name", subjectName);
                var result = checkCmd.ExecuteScalar();

                if (result != null)
                {
                    return Convert.ToInt32(result);
                }
            }

            string insertQuery = "INSERT INTO subjects (subject_name) VALUES (@name); SELECT SCOPE_IDENTITY();";
            using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn, transaction))
            {
                insertCmd.Parameters.AddWithValue("@name", subjectName);
                return Convert.ToInt32(insertCmd.ExecuteScalar());
            }
        }

        public int AddSubject(string subjectName, int hours)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO subjects (subject_name, hours_per_semester) VALUES (@name, @hours); SELECT SCOPE_IDENTITY();";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", subjectName);
                    cmd.Parameters.AddWithValue("@hours", hours);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public int AddClassroom(string roomNumber, string building)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO classrooms (room_number, building) VALUES (@room, @building); SELECT SCOPE_IDENTITY();";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@room", roomNumber);
                    cmd.Parameters.AddWithValue("@building", building);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void AddScheduleItem(int groupId, int subjectId, int teacherId, int classroomId, int slotId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                INSERT INTO schedule (group_id, subject_id, teacher_id, classroom_id, slot_id) 
                VALUES (@gid, @sid, @tid, @cid, @slid)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@gid", groupId);
                    cmd.Parameters.AddWithValue("@sid", subjectId);
                    cmd.Parameters.AddWithValue("@tid", teacherId);
                    cmd.Parameters.AddWithValue("@cid", classroomId);
                    cmd.Parameters.AddWithValue("@slid", slotId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteRecord(string tableName, int id)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = "";

                switch (tableName.ToLower())
                {
                    case "groups":
                    case "группы":
                        query = "DELETE FROM groups WHERE group_id = @id";
                        break;
                    case "teachers":
                    case "преподаватели":
                        string deleteTeacherSubjects = "DELETE FROM teacher_subjects WHERE teacher_id = @id";
                        using (SqlCommand cmd1 = new SqlCommand(deleteTeacherSubjects, conn))
                        {
                            cmd1.Parameters.AddWithValue("@id", id);
                            cmd1.ExecuteNonQuery();
                        }

                        string deleteSchedule = "DELETE FROM schedule WHERE teacher_id = @id";
                        using (SqlCommand cmd2 = new SqlCommand(deleteSchedule, conn))
                        {
                            cmd2.Parameters.AddWithValue("@id", id);
                            cmd2.ExecuteNonQuery();
                        }

                        query = "DELETE FROM teachers WHERE teacher_id = @id";
                        break;
                    case "subjects":
                    case "предметы":
                        string deleteSubjectLinks = "DELETE FROM teacher_subjects WHERE subject_id = @id";
                        using (SqlCommand cmd1 = new SqlCommand(deleteSubjectLinks, conn))
                        {
                            cmd1.Parameters.AddWithValue("@id", id);
                            cmd1.ExecuteNonQuery();
                        }

                        string deleteSubjectSchedule = "DELETE FROM schedule WHERE subject_id = @id";
                        using (SqlCommand cmd2 = new SqlCommand(deleteSubjectSchedule, conn))
                        {
                            cmd2.Parameters.AddWithValue("@id", id);
                            cmd2.ExecuteNonQuery();
                        }

                        query = "DELETE FROM subjects WHERE subject_id = @id";
                        break;
                    case "classrooms":
                    case "аудитории":
                        string deleteClassroomSchedule = "DELETE FROM schedule WHERE classroom_id = @id";
                        using (SqlCommand cmd1 = new SqlCommand(deleteClassroomSchedule, conn))
                        {
                            cmd1.Parameters.AddWithValue("@id", id);
                            cmd1.ExecuteNonQuery();
                        }

                        query = "DELETE FROM classrooms WHERE classroom_id = @id";
                        break;
                    case "schedule":
                        query = "DELETE FROM schedule WHERE schedule_id = @id";
                        break;
                    default:
                        throw new ArgumentException($"Неизвестная таблица: {tableName}");
                }

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateGroup(int id, string newName)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE groups SET group_name = @name WHERE group_id = @id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", newName);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public int GetGroupIdByName(string name)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT group_id FROM groups WHERE group_name = @name";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    var result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : -1;
                }
            }
        }

        public int GetTeacherIdByName(string name)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT teacher_id FROM teachers WHERE full_name = @name";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    var result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : -1;
                }
            }
        }

        public int GetSubjectIdByName(string name)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT subject_id FROM subjects WHERE subject_name = @name";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    var result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : -1;
                }
            }
        }

        public int GetClassroomIdByInfo(string room, string building)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT classroom_id FROM classrooms WHERE room_number = @room AND building = @building";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@room", room);
                    cmd.Parameters.AddWithValue("@building", building);
                    var result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : -1;
                }
            }
        }

        // Проверка: может ли группа заниматься в это время
        public bool CanGroupAttend(int groupId, int slotId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                SELECT COUNT(*) FROM schedule 
                WHERE group_id = @groupId AND slot_id = @slotId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@groupId", groupId);
                    cmd.Parameters.AddWithValue("@slotId", slotId);
                    int count = (int)cmd.ExecuteScalar();
                    return count == 0;
                }
            }
        }

        // Проверка: занят ли кабинет в это время
        public bool IsClassroomAvailable(int classroomId, int slotId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
        SELECT COUNT(*) FROM schedule 
        WHERE classroom_id = @classroomId AND slot_id = @slotId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@classroomId", classroomId);
                    cmd.Parameters.AddWithValue("@slotId", slotId);
                    int count = (int)cmd.ExecuteScalar();
                    return count < 3;
                }
            }
        }


        // Проверка: преподаватель может вести максимум 3 группы одновременно (совмещенные занятия)
        public bool CanTeacherTeach(int teacherId, int slotId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
            SELECT COUNT(DISTINCT group_id) FROM schedule 
            WHERE teacher_id = @teacherId AND slot_id = @slotId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@teacherId", teacherId);
                    cmd.Parameters.AddWithValue("@slotId", slotId);
                    int groupCount = (int)cmd.ExecuteScalar();
                    return groupCount < 3;
                }
            }
        }

        // Проверка: ведет ли преподаватель этот предмет
        public bool DoesTeacherTeachSubject(int teacherId, int subjectId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
            SELECT COUNT(*) FROM teacher_subjects 
            WHERE teacher_id = @teacherId AND subject_id = @subjectId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@teacherId", teacherId);
                    cmd.Parameters.AddWithValue("@subjectId", subjectId);
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        // Проверка: сколько преподавателей ведут этот предмет
        public bool CanSubjectHaveTeacher(int subjectId, int teacherId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                SELECT COUNT(DISTINCT teacher_id) FROM schedule 
                WHERE subject_id = @subjectId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@subjectId", subjectId);
                    int teacherCount = (int)cmd.ExecuteScalar();

                    // Проверяем, ведет ли уже этот преподаватель этот предмет
                    string checkQuery = @"
                    SELECT COUNT(*) FROM schedule 
                    WHERE subject_id = @subjectId AND teacher_id = @teacherId";

                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@subjectId", subjectId);
                        checkCmd.Parameters.AddWithValue("@teacherId", teacherId);
                        int teachesThisSubject = (int)checkCmd.ExecuteScalar();

                        return teachesThisSubject > 0 || teacherCount < 3;
                    }
                }
            }
        }
        public string ValidateScheduleItem(int groupId, int subjectId, int teacherId, int classroomId, int slotId)
        {
            // 1. Группа может заниматься только с одним преподавателем
            if (!CanGroupHaveOnlyOneTeacher(groupId, slotId, teacherId))
                return "Группа уже занимается с другим преподавателем в это время";

            // 2. Группа может быть только в одном кабинете
            if (!CanGroupBeInOnlyOneClassroom(groupId, slotId, classroomId))
                return "Группа уже занимается в другом кабинете в это время";

            // 3. Группа может быть только на одном предмете
            if (!CanGroupHaveOnlyOneSubject(groupId, slotId, subjectId))
                return "Группа уже занимается по другому предмету в это время";

            // 4. Кабинет может вмещать максимум 3 группы
            if (!IsClassroomAvailable(classroomId, slotId))
                return "Кабинет переполнен (максимум 3 группы)";

            // 5. Преподаватель может вести максимум 3 группы одновременно
            if (!CanTeacherTeach(teacherId, slotId))
                return "Преподаватель уже ведет максимальное количество групп (3) одновременно";

            // 6. Преподаватель может вести только один предмет в один временной слот
            if (!CanTeacherTeachOnlyOneSubjectInSlot(teacherId, slotId, subjectId))
                return "Преподаватель уже ведет другой предмет в это время (совмещать можно только один предмет)";

            // 7. Проверка: ведет ли преподаватель этот предмет
            if (!DoesTeacherTeachSubject(teacherId, subjectId))
                return "Преподаватель не ведет этот предмет";

            // 8. Предмет может иметь 2-3 преподавателя
            if (!CanSubjectHaveTeacher(subjectId, teacherId))
                return "Предмет уже имеет максимальное количество преподавателей (3)";

            return null; // Все проверки пройдены
        }

        public bool CanGroupHaveOnlyOneTeacher(int groupId, int slotId, int newTeacherId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
        SELECT teacher_id FROM schedule 
        WHERE group_id = @groupId AND slot_id = @slotId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@groupId", groupId);
                    cmd.Parameters.AddWithValue("@slotId", slotId);

                    var result = cmd.ExecuteScalar();
                    if (result == null)
                        return true; 

                    int existingTeacherId = Convert.ToInt32(result);
                    return existingTeacherId == newTeacherId;
                }
            }
        }

        // Проверка: группа может быть только в одном кабинете
        public bool CanGroupBeInOnlyOneClassroom(int groupId, int slotId, int newClassroomId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
        SELECT classroom_id FROM schedule 
        WHERE group_id = @groupId AND slot_id = @slotId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@groupId", groupId);
                    cmd.Parameters.AddWithValue("@slotId", slotId);

                    var result = cmd.ExecuteScalar();
                    if (result == null)
                        return true; 

                    int existingClassroomId = Convert.ToInt32(result);
                    return existingClassroomId == newClassroomId;
                }
            }
        }

        // Проверка: группа может быть только на одном предмете
        public bool CanGroupHaveOnlyOneSubject(int groupId, int slotId, int newSubjectId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
        SELECT subject_id FROM schedule 
        WHERE group_id = @groupId AND slot_id = @slotId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@groupId", groupId);
                    cmd.Parameters.AddWithValue("@slotId", slotId);

                    var result = cmd.ExecuteScalar();
                    if (result == null)
                        return true;

                    int existingSubjectId = Convert.ToInt32(result);
                    return existingSubjectId == newSubjectId;
                }
            }
        }

        // Проверка: преподаватель может вести только один предмет в один слот
        public bool CanTeacherTeachOnlyOneSubjectInSlot(int teacherId, int slotId, int newSubjectId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
            SELECT DISTINCT subject_id FROM schedule 
            WHERE teacher_id = @teacherId AND slot_id = @slotId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@teacherId", teacherId);
                    cmd.Parameters.AddWithValue("@slotId", slotId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            return true;

                        reader.Read();
                        int existingSubjectId = reader.GetInt32(0);
                        return existingSubjectId == newSubjectId;
                    }
                }
            }
        }

        public bool AddTeacherSubject(int teacherId, string subjectName)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Проверяем, сколько предметов уже ведет преподаватель
                string checkCountQuery = "SELECT COUNT(*) FROM teacher_subjects WHERE teacher_id = @tid";
                using (SqlCommand checkCountCmd = new SqlCommand(checkCountQuery, conn))
                {
                    checkCountCmd.Parameters.AddWithValue("@tid", teacherId);
                    int currentCount = (int)checkCountCmd.ExecuteScalar();

                    if (currentCount >= 3)
                    {
                        MessageBox.Show($"Преподаватель уже ведет максимальное количество предметов (3)", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }

                int subjectId = GetOrCreateSubjectId(subjectName, conn, null);

                string checkExistingQuery = "SELECT COUNT(*) FROM teacher_subjects WHERE teacher_id = @tid AND subject_id = @sid";
                using (SqlCommand checkExistingCmd = new SqlCommand(checkExistingQuery, conn))
                {
                    checkExistingCmd.Parameters.AddWithValue("@tid", teacherId);
                    checkExistingCmd.Parameters.AddWithValue("@sid", subjectId);
                    int exists = (int)checkExistingCmd.ExecuteScalar();

                    if (exists > 0)
                    {
                        return false;
                    }
                }

                // Добавляем связь
                string query = @"
            INSERT INTO teacher_subjects (teacher_id, subject_id) 
            VALUES (@tid, @sid)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@tid", teacherId);
                    cmd.Parameters.AddWithValue("@sid", subjectId);
                    cmd.ExecuteNonQuery();
                }

                return true;
            }
        }
        public bool IsTeacherTeachingSameSubject(int teacherId, int slotId, int subjectId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
            SELECT COUNT(*) FROM schedule 
            WHERE teacher_id = @teacherId AND slot_id = @slotId AND subject_id = @subjectId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@teacherId", teacherId);
                    cmd.Parameters.AddWithValue("@slotId", slotId);
                    cmd.Parameters.AddWithValue("@subjectId", subjectId);
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
        }

        public bool CanTeacherHaveCombinedLesson(int teacherId, int slotId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
            SELECT COUNT(*) FROM schedule 
            WHERE teacher_id = @teacherId AND slot_id = @slotId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@teacherId", teacherId);
                    cmd.Parameters.AddWithValue("@slotId", slotId);
                    int currentGroups = (int)cmd.ExecuteScalar();
                    return currentGroups < 3; // Максимум 3 группы одновременно
                }
            }
        }
       
    }
}