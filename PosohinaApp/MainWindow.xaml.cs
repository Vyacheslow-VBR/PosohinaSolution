using PosohinaApp.ViewModels;
using PosohinaLibrary;
using PosohinaLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.Globalization;

namespace PosohinaApp
{
    public partial class MainWindow : Window
    {
        private AppDbContext _context;
        private List<PartnerViewModel> _partners;

        public MainWindow()
        {
            InitializeComponent();

            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU");
            System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru-RU");

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                _context = new AppDbContext();

                var partners = _context.Partners
                    .Include(p => p.PartnerType)
                    .Include(p => p.Sales)
                        .ThenInclude(s => s.Product)
                    .ToList();

                _partners = partners.Select(p => new PartnerViewModel(p)).ToList();
                PartnersListBox.ItemsSource = _partners;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadPartnerSales(int partnerId)
        {
            try
            {
                var sales = _context.Sales
                    .Include(s => s.Product)
                    .Where(s => s.PartnerId == partnerId)
                    .OrderByDescending(s => s.SaleDate)
                    .ToList();

                SalesDataGrid.ItemsSource = sales.Select(s => new SaleViewModel(s));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки продаж: {ex.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PartnersListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (PartnersListBox.SelectedItem is PartnerViewModel selectedPartner)
            {
                LoadPartnerSales(selectedPartner.Id);
            }
        }

        private void SaveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _context.SaveChanges();
                MessageBox.Show("Данные сохранены", "Информация",
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SaveAsTxtMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Текстовые файлы (*.txt)|*.txt",
                DefaultExt = "txt",
                FileName = $"partners_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using (var writer = new StreamWriter(dialog.FileName, false, Encoding.UTF8))
                    {
                        writer.WriteLine("=== ПАРТНЕРЫ ===");
                        writer.WriteLine($"Дата: {DateTime.Now:dd.MM.yyyy}");
                        writer.WriteLine(new string('-', 50));

                        foreach (var partner in _partners)
                        {
                            writer.WriteLine($"Партнер: {partner.DisplayName}");
                            writer.WriteLine($"Директор: {partner.Director}");
                            writer.WriteLine($"Телефон: {partner.Phone}");
                            writer.WriteLine($"Скидка: {partner.Discount}%");
                            writer.WriteLine(new string('-', 50));
                        }
                    }

                    MessageBox.Show("Файл сохранен", "Успех",
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveAsExcelMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция в разработке", "Информация",
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SaveAsJsonMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция в разработке", "Информация",
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция в разработке", "Информация",
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ImportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция в разработке", "Информация",
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddPartnerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new PartnerDialog(_context);
            if (dialog.ShowDialog() == true)
            {
                LoadData();
            }
        }

        private void EditPartnerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (PartnersListBox.SelectedItem is PartnerViewModel selectedPartner)
            {
                var dialog = new PartnerDialog(_context, selectedPartner.Partner);
                if (dialog.ShowDialog() == true)
                {
                    LoadData();
                    LoadPartnerSales(selectedPartner.Id);
                }
            }
            else
            {
                MessageBox.Show("Выберите партнера", "Предупреждение",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeletePartnerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (PartnersListBox.SelectedItem is PartnerViewModel selectedPartner)
            {
                var result = MessageBox.Show($"Удалить {selectedPartner.DisplayName}?",
                                            "Подтверждение",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _context.Partners.Remove(selectedPartner.Partner);
                        _context.SaveChanges();
                        LoadData();
                        SalesDataGrid.ItemsSource = null;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                                       MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите партнера", "Предупреждение",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void AddSaleMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (PartnersListBox.SelectedItem is PartnerViewModel selectedPartner)
            {
                var dialog = new SaleDialog(_context, selectedPartner.Id);
                if (dialog.ShowDialog() == true)
                {
                    LoadPartnerSales(selectedPartner.Id);
                    LoadData();
                }
            }
            else
            {
                MessageBox.Show("Выберите партнера", "Предупреждение",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void EditSaleMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (SalesDataGrid.SelectedItem is SaleViewModel selectedSale &&
                PartnersListBox.SelectedItem is PartnerViewModel selectedPartner)
            {
                var sale = _context.Sales.Find(selectedSale.Id);
                if (sale != null)
                {
                    var dialog = new SaleDialog(_context, selectedPartner.Id, sale);
                    if (dialog.ShowDialog() == true)
                    {
                        LoadPartnerSales(selectedPartner.Id);
                        LoadData();
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите продажу", "Предупреждение",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteSaleMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (SalesDataGrid.SelectedItem is SaleViewModel selectedSale &&
                PartnersListBox.SelectedItem is PartnerViewModel selectedPartner)
            {
                var result = MessageBox.Show("Удалить продажу?", "Подтверждение",
                                            MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var sale = _context.Sales.Find(selectedSale.Id);
                        if (sale != null)
                        {
                            _context.Sales.Remove(sale);
                            _context.SaveChanges();
                            LoadPartnerSales(selectedPartner.Id);
                            LoadData();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                                       MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите продажу", "Предупреждение",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SalesDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditSaleMenuItem_Click(sender, null);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var result = MessageBox.Show("Выйти из программы?",
                                        "Подтверждение",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                LoadData();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            base.OnClosed(e);
        }
    }
}