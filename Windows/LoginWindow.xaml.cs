using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.Sql;
using TradingPlatform.Models;

namespace TradingPlatform.Windows
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        public LoginWindow(string username, string password)
        {
            InitializeComponent();
            PasswordPB.Password = password;
            UsernameTB.Text = username;
            EnterBtn.IsEnabled = ShouldEnableEnterBtn();
        }

        private void EnterBtn_Click(object sender, RoutedEventArgs e)
        {
            Logger logger = LogManager.getInstance();
            bool error_happened = false;
            Account account = null;
            string connectStr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            using(var connection = new SqlConnection(connectStr))
            {
                connection.Open();
                var cmd = new SqlCommand("SELECT * FROM Account WHERE (username = @un AND password = @ps);",
                    connection);
                cmd.Parameters.AddWithValue("@un", UsernameTB.Text.Trim());
                cmd.Parameters.AddWithValue("@ps", PasswordPB.Password);
                try
                {
                    var dataReader = cmd.ExecuteReader();
                    if (dataReader.Read())
                    {
                        account = new Account
                        {
                            Id = (long)dataReader["id"],
                            Username = (string)dataReader["username"],
                            PhotoURI = (dataReader["photo"] != null) ? dataReader["photo"] as string : "",
                        };
                    } else
                    {
                        throw new NoResultSelectedException("Не существует аккаунта с заданными логином и паролем");
                    }
                } catch(SqlException ex)
                {
                    error_happened = true;
                    logger.LogError(ex);
                    MessageBox.Show("Ошибка запроса, что-то пошло не так...");
                } catch(NoResultSelectedException ex)
                {
                    error_happened = true;
                    logger.LogError(ex);
                    MessageBox.Show("Не существует аккаунта с данными логином и паролем.");
                    Reset();
                }
                if (!error_happened && account != null)
                {
                    logger.LogInfo($"Был вход через аккаунт {account.Username}.");
                    MainWindow mainWindow = new MainWindow(account);
                    mainWindow.Show();
                    Close();
                }
            }
        }

        private void RegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            if (checkNull())
                return;

            RegistrationWindow registrationWindow = new RegistrationWindow();
            registrationWindow.Show();
            Close();
        }


        private bool checkNull()
        {
            return PasswordPB == null || UsernameTB == null || EnterBtn == null || RegisterBtn == null;
        }

        private bool ShouldEnableEnterBtn()
        {
            return UsernameTB.Text.Trim().Length >= 8 && PasswordPB.Password.Length >= 8;
        }
        private void Reset()
        {
            if (checkNull())
                return;

            UsernameTB.Text = UsernameTB.Text.Trim();
            PasswordPB.Password = "";
        }

        private void UsernameTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (checkNull())
                return;

            string username = UsernameTB.Text.Trim();
            EnterBtn.IsEnabled = ShouldEnableEnterBtn();
            if (!Regex.IsMatch(username, @"^[A-Za-z0-9_\-#!@{}()\[\]]+$"))
                EnterBtn.IsEnabled = false;
        }

        private void PasswordPB_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (checkNull())
                return;

            string password = PasswordPB.Password;
            EnterBtn.IsEnabled = ShouldEnableEnterBtn();
            if (!Regex.IsMatch(password, @"^[A-Za-z0-9_\-#!@{}()\[\];:\\| ]+$"))
                EnterBtn.IsEnabled = false;

        }
    }
}