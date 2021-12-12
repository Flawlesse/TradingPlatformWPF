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
using System.Collections.ObjectModel;
using TradingPlatform.Models;
using System.Configuration;
using System.Data.SqlClient;

namespace TradingPlatform.Pages
{
    /// <summary>
    /// Логика взаимодействия для MyProductListPage.xaml
    /// </summary>
    public partial class MyProductListPage : Page
    {
        public ObservableCollection<Product> ListOfMyProucts { get; set; }
        Account CurrentAccount { get; set; }
        string _connStr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        public MyProductListPage(Account account)
        {
            InitializeComponent();

            CurrentAccount = account;
            ListOfMyProucts = GetMyProductsFromDB();
            myProductsLB.ItemsSource = ListOfMyProucts;
        }

        private void CreateBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.RemoveBackEntry();
            NavigationService.Navigate(new CreateEditProductPage(CurrentAccount));
        }

        private void ShowProductBtnClick(object sender, RoutedEventArgs e)
        {
            Button btnClicked = sender as Button;
            long idProductClicked = (long)btnClicked.DataContext;

            // trigger new frame to popup
            //var currentWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            NavigationService.RemoveBackEntry();
            NavigationService.Navigate(new ProductDetailPage(CurrentAccount, null, idProductClicked));
        }

        private ObservableCollection<Product> GetMyProductsFromDB()
        {
            ObservableCollection<Product> products = new ObservableCollection<Product>();
            bool error_happened = false;
            using (var connection = new SqlConnection(_connStr))
            {
                connection.Open();

                var cmd = new SqlCommand(
                    "SELECT Product.id as id, Product.name as name, Product.description, Product.price, " +
                    "Product.photo, Product.owner_account_id, Category.id as category_id, Category.name as category_name " +
                    "FROM Product " +
                    "INNER JOIN Category ON Product.category_id = Category.id WHERE Product.owner_account_id = @id;", connection);
                cmd.Parameters.AddWithValue("@id", CurrentAccount.Id);
                try
                {
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var product = new Product()
                        {
                            Id = (long)reader["id"],
                            Name = (string)reader["name"],
                            Description = (string)reader["description"],
                            Price = (decimal)reader["price"],
                            Photo = (reader["photo"] != DBNull.Value) ? (string)reader["photo"] : null,
                            OwnerAccountID = (long)reader["owner_account_id"],
                            Category = new Category()
                            {
                                Id = (long)reader["category_id"],
                                Name = (string)reader["category_name"]
                            }
                        };
                        products.Add(product); // so, here everything is ok
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                    LogManager.getInstance().LogError(ex);
                    error_happened = true;
                }
                connection.Close();
            }
            if (!error_happened)
            {
                LogManager.getInstance().LogInfo("Выполнено действие Показать мои товары.");
            }
            return products;
        }
    }
}
