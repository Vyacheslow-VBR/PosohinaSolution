using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosohinaLibrary.Models
{
    [Table("products_posohina", Schema = "app")]
    public class Product
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        [MaxLength(200)]
        public string Name { get; set; }

        [Column("code")]
        [MaxLength(50)]
        public string Code { get; set; }

        [Column("cost")]
        public decimal Cost { get; set; }
    }
}