using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVC5.ViewModels
{
    public class NagrodyViewModel
    {
        [Required]
        public HttpPostedFileBase Obrazek { get; set; }
        [Required]
        public string Tytul { get; set; }
        [Required]
        public string Opis { get; set; }
        [Required]
        public decimal Cena { get; set; }
    }
}