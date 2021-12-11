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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TradingPlatform.Models;
using System.Configuration;
using System.Data.SqlClient;
using TradingPlatform.Windows;

namespace TradingPlatform.Pages
{
    /// <summary>
    /// Логика взаимодействия для UserNavPage.xaml
    /// </summary>
    public partial class UserNavPage : Page
    {
        public Account CurrentAccount { get; set; }
        public UserNavPage(Account account)
        {
            CurrentAccount = account;
            InitializeComponent();
            DataContext = CurrentAccount;

            EnableAdvancedButtons();
        }

        private void EnableAdvancedButtons()
        {
            string connectStr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            using(var connection = new SqlConnection(connectStr))
            {
                connection.Open();

                var cmd = new SqlCommand("SELECT (account_id) FROM AdminGroup WHERE account_id = @aid", connection);
                cmd.Parameters.AddWithValue("@aid", CurrentAccount.Id);
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    AdminLogListBtn.Visibility = Visibility.Visible;
                    AdminUserManagementListBtn.Visibility = Visibility.Visible;
                }

                connection.Close();
            }
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите выйти?", "Выйти из аккаунта", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                LoginWindow loginWindow = new LoginWindow();
                var currentWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
                loginWindow.Show();
                currentWindow.Close();
            }
        }
    }
}
