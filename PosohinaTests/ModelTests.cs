using Xunit;
using PosohinaLibrary.Models;
using System;

namespace PosohinaTests
{
    public class ModelTests
    {
        [Fact]
        public void Partner_Create_ShouldWork()
        {
            // Arrange
            var partner = new Partner();

            // Act
            partner.Id = 1;
            partner.Name = "Тестовый партнер";

            // Assert
            Assert.Equal(1, partner.Id);
            Assert.Equal("Тестовый партнер", partner.Name);
        }

        [Fact]
        public void Product_Create_ShouldWork()
        {
            // Arrange
            var product = new Product();

            // Act
            product.Id = 5;
            product.Name = "Тестовый продукт";
            product.Cost = 1000m;

            // Assert
            Assert.Equal(5, product.Id);
            Assert.Equal("Тестовый продукт", product.Name);
            Assert.Equal(1000m, product.Cost);
        }

        [Fact]
        public void Sale_Create_ShouldWork()
        {
            // Arrange
            var sale = new Sale();
            var now = DateTime.Now;

            // Act
            sale.Id = 10;
            sale.Quantity = 5;
            sale.TotalSum = 5000m;

            // Assert
            Assert.Equal(10, sale.Id);
            Assert.Equal(5, sale.Quantity);
            Assert.Equal(5000m, sale.TotalSum);
        }

        [Fact]
        public void PartnerType_Create_ShouldWork()
        {
            // Arrange
            var type = new PartnerType();

            // Act
            type.Id = 3;
            type.Name = "ООО";

            // Assert
            Assert.Equal(3, type.Id);
            Assert.Equal("ООО", type.Name);
        }
    }
}