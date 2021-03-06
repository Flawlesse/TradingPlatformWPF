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
using TradingPlatform.Pages;

namespace TradingPlatform.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Account CurrentAccount { get; private set; }
        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(Account account)
        {
            CurrentAccount = account;
            InitializeComponent();
            UsernameTextBlock.Text = account.Username;

            UserNavFrame.Content = new UserNavPage(CurrentAccount, ContentFrame);
        }
    }
}
