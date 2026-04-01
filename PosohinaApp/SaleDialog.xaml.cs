using Microsoft.EntityFrameworkCore;
using PosohinaLibrary;
using PosohinaLibrary.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace PosohinaApp
{
    public partial class SaleDialog : Window
    {
        private AppDbContext _context;
        private Sale _sale;
        private int _partnerId;
        private const int MAX_QUANTITY = 1000000; // Максимальное количество
        private const decimal MAX_TOTAL_SUM = 99999999.99m; // Максимальная сумма

        public SaleDialog(AppDbContext context, int partnerId, Sale sale = null)
        {
            InitializeComponent();
            _context = context;
            _partnerId = partnerId;

            if (sale == null)
            {
                _sale = new Sale
                {
                    PartnerId = partnerId,
                    SaleDate = DateTime.Now,
                    CreatedAt = DateTime.Now
                };
                Title = "Новая продажа";
            }
            else
            {
                _sale = sale;
                Title = "Редактирование продажи";
            }

            LoadProducts();

            if (sale != null)
            {
                LoadSaleData();
            }

            QuantityTextBox.TextChanged += QuantityTextBox_TextChanged;
            ProductComboBox.SelectionChanged += ProductComboBox_SelectionChanged;
        }

        private void LoadProducts()
        {
            try
            {
                var products = _context.Products.ToList();
                ProductComboBox.ItemsSource = products;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки продуктов: {ex.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadSaleData()
        {
            ProductComboBox.SelectedValue = _sale.ProductId;
            QuantityTextBox.Text = _sale.Quantity.ToString();
            SaleDatePicker.SelectedDate = _sale.SaleDate;
            UpdateTotalSum();
        }

        private void ProductComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdateTotalSum();
        }

        private void QuantityTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            UpdateTotalSum();
        }

        private void UpdateTotalSum()
        {
            // Проверка на пустое поле
            if (string.IsNullOrWhiteSpace(QuantityTextBox.Text))
            {
                TotalSumTextBox.Text = "0 ₽";
                TotalSumTextBox.Foreground = System.Windows.Media.Brushes.Black;
                return;
            }

            // Проверка на корректность ввода
            if (!int.TryParse(QuantityTextBox.Text, out int quantity) || quantity <= 0)
            {
                TotalSumTextBox.Text = "0 ₽";
                TotalSumTextBox.Foreground = System.Windows.Media.Brushes.Black;
                return;
            }

            // Проверка на превышение максимального количества
            if (quantity > MAX_QUANTITY)
            {
                TotalSumTextBox.Text = $"ПРЕВЫШЕН ЛИМИТ! (макс. {MAX_QUANTITY:N0})";
                TotalSumTextBox.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            if (ProductComboBox.SelectedItem is Product selectedProduct)
            {
                try
                {
                    decimal total = selectedProduct.Cost * quantity;

                    // Проверка на превышение максимальной суммы
                    if (total > MAX_TOTAL_SUM)
                    {
                        TotalSumTextBox.Text = $"ПРЕВЫШЕН ЛИМИТ СУММЫ! (макс. {MAX_TOTAL_SUM:N0} ₽)";
                        TotalSumTextBox.Foreground = System.Windows.Media.Brushes.Red;
                        return;
                    }

                    TotalSumTextBox.Text = total.ToString("N0") + " ₽";
                    TotalSumTextBox.Foreground = System.Windows.Media.Brushes.Blue;
                    _sale.TotalSum = total;
                }
                catch (OverflowException)
                {
                    TotalSumTextBox.Text = "ПЕРЕПОЛНЕНИЕ!";
                    TotalSumTextBox.Foreground = System.Windows.Media.Brushes.Red;
                    _sale.TotalSum = 0;
                }
            }
            else
            {
                TotalSumTextBox.Text = "0 ₽";
                TotalSumTextBox.Foreground = System.Windows.Media.Brushes.Black;
                _sale.TotalSum = 0;
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ProductComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите продукт", "Предупреждение",
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(QuantityTextBox.Text, out int quantity) || quantity <= 0)
                {
                    MessageBox.Show("Введите корректное количество", "Предупреждение",
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверка на превышение лимита перед сохранением
                if (quantity > MAX_QUANTITY)
                {
                    MessageBox.Show($"Количество превышает максимально допустимое!\n\nМаксимум: {MAX_QUANTITY:N0}",
                                   "Лимит превышен", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SaleDatePicker.SelectedDate == null)
                {
                    MessageBox.Show("Выберите дату", "Предупреждение",
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var selectedProduct = (Product)ProductComboBox.SelectedItem;
                decimal total = selectedProduct.Cost * quantity;

                if (total > MAX_TOTAL_SUM)
                {
                    MessageBox.Show($"Сумма продажи превышает максимально допустимую!\n\nМаксимум: {MAX_TOTAL_SUM:N0} ₽",
                                   "Лимит превышен", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _sale.ProductId = selectedProduct.Id;
                _sale.Quantity = quantity;
                _sale.SaleDate = SaleDatePicker.SelectedDate.Value;
                _sale.PartnerId = _partnerId;
                _sale.TotalSum = total;

                if (_sale.Id == 0)
                {
                    _sale.CreatedAt = DateTime.Now;
                    _context.Sales.Add(_sale);
                }
                else
                {
                    _context.Entry(_sale).State = EntityState.Modified;
                }

                _context.SaveChanges();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += $"\n\nДетали: {ex.InnerException.Message}";
                }
                MessageBox.Show($"Ошибка: {errorMessage}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // Ограничение на ввод только цифр и backspace
        private void QuantityTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true; // Запрещаем пробел
            }
        }
    }
}