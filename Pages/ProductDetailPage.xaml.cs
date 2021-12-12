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
using System.Data.SqlClient;
using System.Configuration;

namespace TradingPlatform.Pages
{
    /// <summary>
    /// Логика взаимодействия для ProductDetailPage.xaml
    /// </summary>
    public partial class ProductDetailPage : Page
    {
        public Product ProductSelected { get; set; }
        Account CurrentAccount { get; set; }
        string _connStr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        public ProductDetailPage( Account account, Product product=null, long id=0)
        {
            InitializeComponent();

            CurrentAccount = account;
            ProductSelected = GetProductFromDB(product, id);
            DataContext = ProductSelected;
        }

        private Product GetProductFromDB(Product prod, long id)
        {
            Product product = prod;
            using (var connection = new SqlConnection(_connStr))
            {
                connection.Open();
                var cmd = (prod == null) ? 
                    new SqlCommand(
                        "SELECT Product.id as id, Product.name as name, Product.description, Product.price, " +
                        "Product.photo, Product.owner_account_id, Category.id as category_id, Category.name as category_name " +
                        "FROM Product " +
                        $"INNER JOIN Category ON Product.category_id = Category.id WHERE Product.id = {id};", connection)
                    : new SqlCommand(
                        "SELECT Product.id as id, Product.name as name, Product.description, Product.price, " +
                        "Product.photo, Product.owner_account_id, Category.id as category_id, Category.name as category_name " +
                        "FROM Product " +
                        $"INNER JOIN Category ON Product.category_id = Category.id WHERE Product.name = '{prod.Name}';", connection
                    );
                cmd.CommandType = System.Data.CommandType.Text;
                try
                {
                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        product = new Product()
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
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                    LogManager.getInstance().LogError(ex);
                }
                connection.Close();
            }
            if (product != null)
            {
                LogManager.getInstance().LogInfo($"Выполнено действие Показать продукт полностью над продуктом {product.Name}");
            }
            return product ?? new Product();
        }

        private void GoBackBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.RemoveBackEntry();
            NavigationService.Navigate(new MyProductListPage(CurrentAccount));
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            bool error_happened = false;
            // perform delete operation
            if (MessageBox.Show("Вы действительно хотите удалить данный товар?", $"Удаление {ProductSelected.Name}", MessageBoxButton.YesNo) 
                == MessageBoxResult.Yes)
            {
                using (var connection = new SqlConnection(_connStr))
                {
                    connection.Open();

                    var cmd = new SqlCommand("DELETE FROM Product WHERE id = @id;", connection);
                    cmd.Parameters.AddWithValue("@id", ProductSelected.Id);
                    try
                    {
                        cmd.ExecuteNonQuery();
                    } catch (SqlException ex)
                    {
                        error_happened = true;
                        MessageBox.Show("Ошибка удаления товара. Лучше смотрите в код.");
                        LogManager.getInstance().LogError(ex);
                    }

                    connection.Close();
                }
                if (!error_happened)
                {
                    var msg = $"Товар {ProductSelected.Id} был успешно удалён!";
                    MessageBox.Show(msg);
                    LogManager.getInstance().LogInfo(msg);
                    NavigationService.RemoveBackEntry();
                    NavigationService.Navigate(new MyProductListPage(CurrentAccount));
                }
            }
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            // open edit page
            NavigationService.RemoveBackEntry();
            NavigationService.Navigate(new CreateEditProductPage(ProductSelected, CurrentAccount));
        }

        private void MakeDealBtn_Click(object sender, RoutedEventArgs e)
        {
            // create Deal entry and nothing more
        }
    }
}
