using System.ComponentModel.DataAnnotations;
using MVC5.Models;

namespace MVC5.ViewModels
{
    public class OstatecznyViewModel
    {
        [Required]
        public UserModel Uzytkownik { get; set; }

        [Required]
        public NagrodyModel Nagroda { get; set; }
    }
}