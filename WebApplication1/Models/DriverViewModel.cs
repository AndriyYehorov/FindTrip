using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class DriverViewModel
    {
        public DriverViewModel()
        {
        }
        public DriverViewModel(string name, string surname, string phoneNumber)
        {
            Name = name;
            Surname = surname;
            PhoneNumber = phoneNumber;
        }

        [Required(ErrorMessage = "Дане поле обов'язкове")]        
        public IFormFile DriverLicense { get; set; }

        [Required(ErrorMessage = "Дане поле обов'язкове")]
        [StringLength(25)]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Дане поле обов'язкове")]
        [StringLength(25)]
        public string? Surname { get; set; }

        [Required(ErrorMessage = "Дане поле обов'язкове")]
        [RegularExpression("\\d+", ErrorMessage = "В {0} можна використовувати тільки цифри")]
        [StringLength(10, ErrorMessage = "Довжина {0} має бути {1} символів.", MinimumLength = 10)]
        public string? PhoneNumber { get; set; }

    }
}
