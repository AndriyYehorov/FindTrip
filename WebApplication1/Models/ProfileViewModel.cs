using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class ProfileViewModel
    {
        public ProfileViewModel() { }
        public ProfileViewModel(string login, string email)
        {
            Login = login;            
            Email = email;
        }        
        public string? Login { get; set; }

        public string? Email { get; set; }

		[Required(ErrorMessage = "Дане поле обов'язкове")]
		[StringLength(25, ErrorMessage = "Довжина {0} має бути у проміжку від {2} до {1}.", MinimumLength = 6)]
		public string? Password { get; set; }

        [Required(ErrorMessage = "Дане поле обов'язкове")]
        [Compare("Password", ErrorMessage = "Паролі не співпадають")]
        public string? RepeatPassword { get; set; }        
    }
}
