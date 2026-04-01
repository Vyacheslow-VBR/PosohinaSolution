using PosohinaLibrary.Models;
using System;

namespace PosohinaApp.ViewModels
{
    public class SaleViewModel
    {
        private Sale _sale;

        public SaleViewModel(Sale sale)
        {
            _sale = sale;
        }

        public int Id => _sale.Id;
        public string ProductName => _sale.Product?.Name ?? "Неизвестно";
        public int Quantity => _sale.Quantity;
        public DateTime SaleDate => _sale.SaleDate;
        public decimal TotalSum => _sale.TotalSum;
    }
}