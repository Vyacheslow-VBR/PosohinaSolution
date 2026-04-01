using Xunit;
using PosohinaLibrary.Models;

namespace PosohinaTests
{
    public class PartnerTests
    {
        [Fact]
        public void Partner_Discount_DefaultIsZero()
        {
            // Arrange
            var partner = new Partner();

            // Act
            var discount = partner.Discount;

            // Assert
            Assert.Equal(0, discount);
        }

        [Fact]
        public void Partner_CanSetDiscount()
        {
            // Arrange
            var partner = new Partner();

            // Act
            partner.Discount = 15;

            // Assert
            Assert.Equal(15, partner.Discount);
        }

        [Fact]
        public void Partner_Rating_CanBeNull()
        {
            // Arrange
            var partner = new Partner();

            // Act
            partner.Rating = null;

            // Assert
            Assert.Null(partner.Rating);
        }

        [Fact]
        public void Partner_Rating_CanBeSet()
        {
            // Arrange
            var partner = new Partner();

            // Act
            partner.Rating = 5;

            // Assert
            Assert.Equal(5, partner.Rating);
        }
    }
}