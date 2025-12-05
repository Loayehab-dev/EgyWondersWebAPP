using System.ComponentModel.DataAnnotations;

namespace EgyWonders.DTO
{
    public class UpdateTourDTO
    {
        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string Title { get; set; } = null!;

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? Category { get; set; }

        [StringLength(100)]
        public string? CityName { get; set; }

        [Range(-90, 90)]
        public decimal? CityLatitude { get; set; }

        [Range(-180, 180)]
        public decimal? CityLongitude { get; set; }

        [Range(0, 1000000)]
        public decimal? BasePrice { get; set; }

        [Required]
       
        public int UserId { get; set; }
    }
}
