using Xunit;
using PosohinaLibrary.Models;

namespace PosohinaTests
{
    public class ProductTests
    {
        [Fact]
        public void Product_Properties_Work()
        {
            // Arrange
            var product = new Product();

            // Act
            product.Id = 7;
            product.Name = "Ноутбук";
            product.Code = "NB001";
            product.Cost = 50000m;

            // Assert
            Assert.Equal(7, product.Id);
            Assert.Equal("Ноутбук", product.Name);
            Assert.Equal("NB001", product.Code);
            Assert.Equal(50000m, product.Cost);
        }
    }
}