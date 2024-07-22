using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{    public class TripViewModel
     {
        [Required(ErrorMessage = "Дане поле обов'язкове")]
        public DateTime DepartureTime { get; set; }

        [Required(ErrorMessage = "Дане поле обов'язкове")]
        public string? DeparturePoint { get; set; }

        [Required(ErrorMessage = "Дане поле обов'язкове")]
        public string? ArrivalPoint { get; set; }        
        public int AmountOfFreeSeats { get; set; }

        [Required(ErrorMessage = "Дане поле обов'язкове")]
        public int AmountOfSeats { get; set; }

        [Required(ErrorMessage = "Дане поле обов'язкове")]
        public int Price { get; set; }        
     }   
}
