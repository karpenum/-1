using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using Агеенков_курсач.Admin;
using Агеенков_курсач.Customer;
using Агеенков_курсач.Operator;
using Агеенков_курсач.Researcher;
namespace Агеенков_курсач
{
    public partial class MainWindow : Window
    {
        private string connectionString = @"Data Source=DESKTOP-HVQ1BQC\SQLEXPRESS;Initial Catalog=БД_Агеенков;Integrated Security=True";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedRole = (RoleTabControl.SelectedItem as TabItem)?.Header.ToString();
            string username = string.Empty;
            string password = string.Empty;

            // Получение логина и пароля в зависимости от выбранной вкладки
            switch (selectedRole)
            {
                case "Администратор":
                    username = UsernameTextBoxAdmin.Text;
                    password = PasswordBoxAdmin.Password;
                    break;
                case "Оператор":
                    username = UsernameTextBoxOperator.Text;
                    password = PasswordBoxOperator.Password;
                    break;
                case "Исследователь":
                    username = UsernameTextBoxResearcher.Text;
                    password = PasswordBoxResearcher.Password;
                    break;
                case "Заказчик":
                    username = UsernameTextBoxCustomer.Text;
                    password = PasswordBoxCustomer.Password;
                    break;
                default:
                    MessageBox.Show("Пожалуйста, выберите роль.");
                    return;
            }

            // Проверка логина и пароля для админа
            if (selectedRole == "Администратор")
            {
                if (ValidateUser(username, password, 4))
                {
                    AdminMenuWindow adminWindow = new AdminMenuWindow();
                    adminWindow.Show();
                    this.Close();
                }
                
            }
            else if (selectedRole == "Оператор")
            {
                if (ValidateUser(username, password, 3))
                {
                    OperatorMenuWindow adminWindow = new OperatorMenuWindow();
                    adminWindow.Show();
                    this.Close();
                }
               
            }
            else if (selectedRole == "Исследователь")
            {
                if (ValidateUser(username, password, 2))
                {
                    ResearcherMenuWindow adminWindow = new ResearcherMenuWindow();
                    adminWindow.Show();
                    this.Close();
                }
                
            }
            else if (selectedRole == "Заказчик")
            {
                if (ValidateUser(username, password, 1))
                {
                    CustomerMenuWindow adminWindow = new CustomerMenuWindow();
                    adminWindow.Show();
                    this.Close();
                }
              
            }
        }

        public bool ValidateUser(string username, string password, int typeId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // 1. Проверка учетных данных пользователя
                string authQuery = "SELECT COUNT(*) FROM Пользователи WHERE логин = @Username AND пароль = @Password AND тип_id = @TypeId";

                using (SqlCommand authCommand = new SqlCommand(authQuery, connection))
                {
                    authCommand.Parameters.AddWithValue("@Username", username);
                    authCommand.Parameters.AddWithValue("@Password", password);
                    authCommand.Parameters.AddWithValue("@TypeId", typeId);

                    int userCount = Convert.ToInt32(authCommand.ExecuteScalar());
                    if (userCount <= 0)
                    {
                        MessageBox.Show("Неверный логин или пароль.", "Ошибка авторизации",
                                      MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                }

                // 2. Получение данных пользователя
                string userDataQuery = @"
            SELECT 
                проект_id,
                фио
            FROM 
                Пользователи 
            WHERE 
                логин = @Username AND 
                пароль = @Password AND 
                тип_id = @TypeId";

                string userFio = null;
                int? projectId = null;

                using (SqlCommand dataCommand = new SqlCommand(userDataQuery, connection))
                {
                    dataCommand.Parameters.AddWithValue("@Username", username);
                    dataCommand.Parameters.AddWithValue("@Password", password);
                    dataCommand.Parameters.AddWithValue("@TypeId", typeId);

                    using (SqlDataReader reader = dataCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            projectId = reader.IsDBNull(0) ? (int?)null : reader.GetInt32(0);
                            userFio = reader.IsDBNull(1) ? null : reader.GetString(1);
                        }
                    }
                }

                // Сохраняем project_id
                ProjectManager.Instance.CurrentProject = projectId ?? -1;

                // 3. Если это оператор, находим его ID в таблице Операторы
                if (typeId == 3 && !string.IsNullOrEmpty(userFio))
                {
                    string operatorQuery = "SELECT id FROM Операторы WHERE фио = @Fio";

                    using (SqlCommand operatorCommand = new SqlCommand(operatorQuery, connection))
                    {
                        operatorCommand.Parameters.AddWithValue("@Fio", userFio);
                        object result = operatorCommand.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                        {
                            ProjectManager.Instance.CurrentOperator = Convert.ToInt32(result);
                        }
                        else
                        {
                            MessageBox.Show("Оператор с таким ФИО не найден в системе",
                                          "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            ProjectManager.Instance.CurrentOperator = -1;
                        }
                    }
                }

                return true;
            }
        }

        private void FindAndSetOperatorId(SqlConnection connection, string fio)
        {
            string operatorQuery = "SELECT id FROM Операторы WHERE фио = @Fio";

            using (SqlCommand operatorCommand = new SqlCommand(operatorQuery, connection))
            {
                operatorCommand.Parameters.AddWithValue("@Fio", fio);
                object result = operatorCommand.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                {
                    ProjectManager.Instance.CurrentOperator = Convert.ToInt32(result);
                }
                else
                {
                    MessageBox.Show("Оператор с таким ФИО не найден в системе",
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    ProjectManager.Instance.CurrentOperator = -1;
                }
            }
        }
    }
}