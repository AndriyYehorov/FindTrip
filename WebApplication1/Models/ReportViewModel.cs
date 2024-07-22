using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class ReportViewModel
    {        
        public string? Text { get; set; }

        [Required(ErrorMessage = "Дане поле обов'язкове")]
        
        public string Rating { get; set; }

        [Required(ErrorMessage = "Дане поле обов'язкове")]
        public int TripId { get; set; }    
        
    }
}
