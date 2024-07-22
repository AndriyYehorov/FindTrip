using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class UserViewModel
    {
        public LoginViewModel LoginViewModel { get; set; }

        public RegisterViewModel RegisterViewModel { get; set; }    
    }    
    public class LoginViewModel
    {        
        [Required(ErrorMessage = "Дане поле обов'язкове")]        
        public string Login { get; set; }

        [Required(ErrorMessage = "Дане поле обов'язкове")]
        public string Password { get; set; }       
    }
    public class RegisterViewModel
    {
		[Required(ErrorMessage = "Дане поле обов'язкове")]		
		[EmailAddress(ErrorMessage = "Неправильна адреса електронної пошти.")]
		public string Email { get; set; }

        [Required(ErrorMessage = "Дане поле обов'язкове")]
        [StringLength(15, ErrorMessage = "Довжина {0} має бути у проміжку від {2} до {1}.", MinimumLength = 6)]
        [RegularExpression("[a-zA-Z\\d]+", ErrorMessage = "В {0} можна використовувати тільки цифри та латинські літери")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Дане поле обов'язкове")]
        [StringLength(25, ErrorMessage = "Довжина {0} має бути у проміжку від {2} до {1}.", MinimumLength = 6)]        
        public string Password { get; set; }

        [Required(ErrorMessage = "Дане поле обов'язкове")]
        [Compare("Password", ErrorMessage = "Паролі не співпадають")]
        public string RepeatPassword { get; set; }
    }   
}
