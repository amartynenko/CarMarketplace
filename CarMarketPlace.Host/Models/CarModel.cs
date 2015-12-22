using System.ComponentModel.DataAnnotations;

namespace CarMarketPlace.Models
{
    public class CarModel
    {
        [Required]
        public string Brand { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public decimal Price { get; set; }
    }
}