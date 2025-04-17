using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SimpleWebRequest
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<Product> _products = new ObservableCollection<Product>();
        private Product _selectedProduct = null;
        private readonly string _baseUrl = "https://pmarcelis.mid-ica.nl/products/";

        public MainWindow()
        {
            InitializeComponent();
            UpdateButton.IsEnabled = false; // Initially disable the update button
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadProductsAsync();
        }

        private async Task LoadProductsAsync()
        {
            try
            {
                var products = await GetProductsAsync(_baseUrl);
                _products.Clear();
                
                foreach (var product in products)
                {
                    _products.Add(product);
                }
                
                ProductsDataGrid.ItemsSource = _products;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading products: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // GET the list of products 
        private async Task<List<Product>> GetProductsAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Could not retrieve products: {response.StatusCode}");
                    return new List<Product>();
                }

                string json = await response.Content.ReadAsStringAsync();
                var wrapper = JsonConvert.DeserializeObject<ApiResponse<List<Product>>>(json);

                return wrapper?.Data ?? new List<Product>();
            }
        }

        // POST a new product
        private async Task<Product> PostProductAsync(string url, Product product)
        {
            using (HttpClient client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(product);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);
                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Error creating product: {response.StatusCode}");
                    return null;
                }

                string responseBody = await response.Content.ReadAsStringAsync();
                var returnData = JsonConvert.DeserializeObject<ApiResponse<Product>>(responseBody);
                return returnData?.Data;
            }
        }

        // PUT (update) a product
        private async Task<bool> UpdateProductAsync(string url, int productId, Product product)
        {
            using (HttpClient client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(product);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Add the ID as a query parameter to the URL
                var requestUrl = $"{url}?id={productId}";
                
                var response = await client.PutAsync(requestUrl, content);
                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Error updating product: {response.StatusCode}");
                    return false;
                }

                string responseBody = await response.Content.ReadAsStringAsync();
                var returnData = JsonConvert.DeserializeObject<ApiResponse<Product>>(responseBody);
                
                if (returnData != null && !string.IsNullOrEmpty(returnData.Message))
                {
                    MessageBox.Show(returnData.Message);
                }
                
                return true;
            }
        }

        // DELETE a product
        private async Task<bool> DeleteProductAsync(string url, int productId)
        {
            using (HttpClient client = new HttpClient())
            {
                // Add the ID as a query parameter to the URL
                var requestUrl = $"{url}?id={productId}";
                
                var response = await client.DeleteAsync(requestUrl);
                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Error deleting product: {response.StatusCode}");
                    return false;
                }

                string responseBody = await response.Content.ReadAsStringAsync();
                var returnData = JsonConvert.DeserializeObject<ApiResponse<object>>(responseBody);
                
                if (returnData != null && !string.IsNullOrEmpty(returnData.Message))
                {
                    MessageBox.Show(returnData.Message);
                }
                
                return true;
            }
        }

        private void ClearForm()
        {
            NameTextBox.Text = string.Empty;
            PriceTextBox.Text = string.Empty;
            DescriptionTextBox.Text = string.Empty;
            _selectedProduct = null;
            UpdateButton.IsEnabled = false;
        }

        // Edit button click - populate form with selected product data
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                _selectedProduct = button.DataContext as Product;
                if (_selectedProduct != null)
                {
                    NameTextBox.Text = _selectedProduct.Name;
                    PriceTextBox.Text = _selectedProduct.Price.ToString();
                    DescriptionTextBox.Text = _selectedProduct.ShortDescription ?? _selectedProduct.Description;
                    UpdateButton.IsEnabled = true;
                }
            }
        }

        // Delete button click
        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                var product = button.DataContext as Product;
                if (product != null)
                {
                    var result = MessageBox.Show($"Are you sure you want to delete '{product.Name}'?", 
                        "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        var success = await DeleteProductAsync(_baseUrl, product.Id);
                        if (success)
                        {
                            // Remove the product from the collection
                            _products.Remove(product);
                        }
                    }
                }
            }
        }

        // Add new product
        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NameTextBox.Text) || 
                    string.IsNullOrWhiteSpace(PriceTextBox.Text) || 
                    string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
                {
                    MessageBox.Show("Please fill in all fields", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(PriceTextBox.Text, out decimal price))
                {
                    MessageBox.Show("Please enter a valid price", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var newProduct = new Product
                {
                    Name = NameTextBox.Text,
                    Price = price,
                    ShortDescription = DescriptionTextBox.Text
                };

                var createdProduct = await PostProductAsync(_baseUrl, newProduct);
                if (createdProduct != null)
                {
                    _products.Add(createdProduct);
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding product: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Update existing product
        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_selectedProduct == null)
                {
                    MessageBox.Show("No product selected for update", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(NameTextBox.Text) || 
                    string.IsNullOrWhiteSpace(PriceTextBox.Text) || 
                    string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
                {
                    MessageBox.Show("Please fill in all fields", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(PriceTextBox.Text, out decimal price))
                {
                    MessageBox.Show("Please enter a valid price", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var updatedProduct = new Product
                {
                    Name = NameTextBox.Text,
                    Price = price,
                    ShortDescription = DescriptionTextBox.Text
                };

                var success = await UpdateProductAsync(_baseUrl, _selectedProduct.Id, updatedProduct);
                if (success)
                {
                    // Update the product in our local collection
                    var index = _products.IndexOf(_selectedProduct);
                    if (index >= 0)
                    {
                        updatedProduct.Id = _selectedProduct.Id; // Keep the same ID
                        _products[index] = updatedProduct;
                    }
                    
                    ClearForm();
                    // Refresh the DataGrid to show changes
                    ProductsDataGrid.Items.Refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating product: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }
    }
}