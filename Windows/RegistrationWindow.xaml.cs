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
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Data.Sql;
using System.Text.RegularExpressions;

namespace TradingPlatform.Windows
{
    /// <summary>
    /// Логика взаимодействия для RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : Window
    {
        public RegistrationWindow()
        {
            InitializeComponent();
        }

        private void RegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            Logger logger = LogManager.getInstance();
            bool error_happened = false;
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new SqlCommand("INSERT INTO Account (username, password) " +
                    $"VALUES (@un, @ps);", connection);
                cmd.Parameters.AddWithValue("@un", UsernameTB.Text.Trim());
                cmd.Parameters.AddWithValue("@ps", PasswordPB.Password);
                try
                {
                    cmd.ExecuteNonQuery();
                    logger.LogInfo($"Аккаунт {UsernameTB.Text.Trim()} успешно зарегестрирован.");
                }
                catch (SqlException ex)
                {
                    error_happened = true;
                    MessageBox.Show("Что-то пошло не так. Скорее всего пользователь с данным логином уже существует.");
                    logger.LogError(ex);
                    Reset();
                }
                connection.Close();
            }

            if (!error_happened)
            {
                MessageBox.Show("Отлично, вы прошли регистрацию! Далее выполните вход.");
                LoginWindow loginWindow = new LoginWindow(UsernameTB.Text.Trim(), PasswordPB.Password);
                loginWindow.Show();
                Close();
            }
        }

        private void EnterBtn_Click(object sender, RoutedEventArgs e)
        {
            if (checkNull())
                return;

            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            if (checkNull())
                return;

            string username = UsernameTB.Text.Trim();
            RegisterBtn.IsEnabled = ShouldEnableRegBtn();
            if (!Regex.IsMatch(username, @"^[A-Za-z0-9_\-#!@{}()\[\]]+$"))
                RegisterBtn.IsEnabled = false;
        }

        private void PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (checkNull())
                return;

            string password = PasswordPB.Password;
            RegisterBtn.IsEnabled = ShouldEnableRegBtn();
            if (!Regex.IsMatch(password, @"^[A-Za-z0-9_\-#!@{}()\[\];:\\| ]+$"))
                RegisterBtn.IsEnabled = false;

            if (PasswordPB.Password != PasswordRepPB.Password)
                PasswordsDontMatch.Visibility = Visibility.Visible;
            else
                PasswordsDontMatch.Visibility = Visibility.Collapsed;
        }


        private bool checkNull()
        {
            return PasswordsDontMatch == null || PasswordRepPB == null || PasswordPB == null
                || UsernameTB == null || RegisterBtn == null || EnterBtn == null;
        }

        private bool ShouldEnableRegBtn()
        {
            return UsernameTB.Text.Trim().Length >= 8
                && PasswordPB.Password == PasswordRepPB.Password
                && PasswordRepPB.Password.Length >= 8
                && PasswordPB.Password.Length >= 8;
        }
        private void Reset()
        {
            if (checkNull())
                return;

            UsernameTB.Text = UsernameTB.Text.Trim();
            PasswordPB.Password = "";
            PasswordRepPB.Password = "";
        }
    }
}