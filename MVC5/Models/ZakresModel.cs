using System.ComponentModel.DataAnnotations;

namespace MVC5.Models
{
    public class ZakresModel
    {
        [Required]
        [MinLength(1)]
        [MaxLength(100)]
        public int? Poczatek { get; set; }
        [Required]
        [MinLength(1)]
        [MaxLength(100)]
        public int? Koniec { get; set; }
    }
}