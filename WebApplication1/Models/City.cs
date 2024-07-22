using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class City
    {
        public int Id {  get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        public string Coordinates { get; set; }
    }
}
