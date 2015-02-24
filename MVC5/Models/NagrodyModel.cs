using System.ComponentModel.DataAnnotations;

namespace MVC5.Models
{
    public class NagrodyModel
    {
        public int ID { get; set; }
        public string Obrazek { get; set; }
        public string Tytul { get; set; }
        public string Opis { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:c}")]
        public decimal Cena { get; set; }
    }
}