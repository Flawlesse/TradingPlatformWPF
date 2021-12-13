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
using System.Data.SqlClient;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using System.Threading;

namespace TradingPlatform.Pages
{
    /// <summary>
    /// Логика взаимодействия для CreateProductPage.xaml
    /// </summary>
    public partial class CreateEditProductPage : Page
    {
        Account CurrentAccount { get; set; }
        Product CurrentProduct { get; set; } = new Product();
        bool _isCreateWindow = false;
        string _connStr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        List<Category> Categories { get; set; }

        public CreateEditProductPage(Account account)
        {
            InitializeComponent();
            CurrentAccount = account;
            _isCreateWindow = true;

            DataContext = CurrentProduct;
            CreateEditBtn.Content = _isCreateWindow ? "Создать" : "Сохранить";
            Categories = GetAllCategories();

            CategoryCB.ItemsSource = Categories;
            CategoryCB.SelectedItem = null;
        }
        public CreateEditProductPage(Product product, Account account)
        {
            CurrentAccount = account;
            CurrentProduct = product;
            InitializeComponent();

            DataContext = CurrentProduct;
            CreateEditBtn.Content = _isCreateWindow ? "Создать" : "Сохранить";
            Categories = GetAllCategories();

            CategoryCB.ItemsSource = Categories;
            CategoryCB.SelectedIndex = Categories.FindIndex(x => x.Id == CurrentProduct.Category.Id);
            NameTB.Text = CurrentProduct.Name;
            DescriptionTB.Text = CurrentProduct.Description;
            PriceTB.Text = CurrentProduct.Price.ToString();

        }

        private void CreateEditBtn_Click(object sender, RoutedEventArgs e)
        {
            CurrentProduct.Name = NameTB.Text.Trim();
            CurrentProduct.OwnerAccountID = CurrentAccount.Id;
            CurrentProduct.Description = DescriptionTB.Text.Trim();
            CurrentProduct.Price = Convert.ToDecimal(PriceTB.Text.Trim(), NumberFormatInfo.InvariantInfo);
            CurrentProduct.Category = CategoryCB.SelectedItem as Category;


            using (var connection = new SqlConnection(_connStr))
            {
                connection.Open();
                bool error_happened = false;

                if (_isCreateWindow)
                {
                    var cmd = new SqlCommand(
                        "INSERT INTO Product (name, description, price, photo, owner_account_id, category_id) " +
                        "VALUES (@name, @desc, @price, @photo, @ownid, @catid);", connection);
                    cmd.Parameters.AddWithValue("@name", CurrentProduct.Name);
                    cmd.Parameters.AddWithValue("@desc", CurrentProduct.Description);
                    cmd.Parameters.AddWithValue("@price", CurrentProduct.Price);
                    cmd.Parameters.AddWithValue("@photo", (object)CurrentProduct.Photo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ownid", CurrentProduct.OwnerAccountID);
                    cmd.Parameters.AddWithValue("@catid", CurrentProduct.Category.Id);

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        error_happened = true;
                        System.Windows.MessageBox.Show("Ошибка создания. Скорее всего, товар с таким наименованием уже существует.");
                        LogManager.getInstance().LogError(ex);
                    }

                    if (!error_happened)
                    {
                        var msg = $"Товар {CurrentProduct.Name} успешно создан!";
                        System.Windows.MessageBox.Show(msg);
                        LogManager.getInstance().LogInfo(msg);

                        NavigationService.RemoveBackEntry();
                        NavigationService.Navigate(new ProductDetailPage(CurrentAccount, CurrentProduct));
                    }
                }
                else
                {
                    var cmd = new SqlCommand(
                        "Update Product " +
                        "SET name=@name, description=@desc, price=@price, photo=@photo, category_id=@catid " +
                        "WHERE id = @id;", connection);
                    cmd.Parameters.AddWithValue("@id", CurrentProduct.Id);
                    cmd.Parameters.AddWithValue("@name", CurrentProduct.Name);
                    cmd.Parameters.AddWithValue("@desc", CurrentProduct.Description);
                    cmd.Parameters.AddWithValue("@price", CurrentProduct.Price);
                    cmd.Parameters.AddWithValue("@photo", (object)CurrentProduct.Photo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@catid", CurrentProduct.Category.Id);

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        error_happened = true;
                        System.Windows.MessageBox.Show("Ошибка обновления. Скорее всего, товар с таким наименованием уже существует.");
                        LogManager.getInstance().LogError(ex);
                    }

                    if (!error_happened)
                    {
                        var msg = $"Товар {CurrentProduct.Name} успешно обновлён!";
                        System.Windows.MessageBox.Show(msg);
                        LogManager.getInstance().LogInfo(msg);

                        
                        NavigationService.RemoveBackEntry();
                        NavigationService.Navigate(new ProductDetailPage(CurrentAccount, null, CurrentProduct.Id));
                    }
                }

                connection.Close();
            }
        }

        private void PriceTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            CreateEditBtn.IsEnabled = ShouldEnableCreateEditBtn();
        }

        private void NameTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            CreateEditBtn.IsEnabled = ShouldEnableCreateEditBtn();
        }

        private void CategoryCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CreateEditBtn.IsEnabled = ShouldEnableCreateEditBtn();
        }

        private List<Category> GetAllCategories()
        {
            var list = new List<Category>();
            using (var connection = new SqlConnection(_connStr))
            {
                connection.Open();
                var cmd = new SqlCommand("SELECT * FROM Category;", connection);
                try
                {
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var category = new Category()
                        {
                            Id = (long)reader["id"],
                            Name = (string)reader["name"],
                        };
                        list.Add(category);
                    }
                    LogManager.getInstance().LogInfo("Успешно считаны все возможные категории.");
                }
                catch (SqlException ex)
                {
                    System.Windows.MessageBox.Show("Не удалось получить все возможные категории.");
                    LogManager.getInstance().LogError(ex);
                }
                connection.Close();
            }
            return list;
        }

        private bool ShouldEnableCreateEditBtn()
        {
            bool isNameOk = !NameTB.Text.Contains("'") && !NameTB.Text.Contains('"') && NameTB.Text.Trim().Length > 0;
            bool isDescriptionOk = !DescriptionTB.Text.Contains("'") && !DescriptionTB.Text.Contains('"') && DescriptionTB.Text.Length <= 2000;
            bool isCategoryOk = CategoryCB.SelectedIndex != -1;
            // Either 1-4 digits without dot, or 1-4 digits WITH a dot, but followed by 1-2 digits afterwards
            bool isPriceOk = Regex.IsMatch(PriceTB.Text.Trim(), @"(^[0-9]{1,4}$)|(^[0-9]{1,4}\.[0-9]{1,2}$)");
            if (isPriceOk)
            {
                decimal res = Convert.ToDecimal(PriceTB.Text.Trim(), NumberFormatInfo.InvariantInfo);
                isPriceOk = res > 0;
            }
            return isNameOk && isDescriptionOk && isCategoryOk && isPriceOk;
        }

        private void ProductImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            using (var fd = new OpenFileDialog())
            {
                fd.Filter = "ImageFiles(*.BMP;*.JPG;*.GIF,*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG";
                System.Windows.Forms.DialogResult res = fd.ShowDialog();
                if (res == System.Windows.Forms.DialogResult.OK || res == System.Windows.Forms.DialogResult.Yes)
                {
                    Uri fileUri = new Uri(fd.FileName);
                    CurrentProduct.Photo = fileUri.OriginalString;
                    ProductImage.Source = new BitmapImage(fileUri);
                }
            }
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.RemoveBackEntry();
            if (_isCreateWindow)
            {
                NavigationService.Navigate(new MyProductListPage(CurrentAccount));
            } else
            {
                NavigationService.Navigate(new ProductDetailPage(CurrentAccount, null, CurrentProduct.Id));
            }
        }
    }
}
