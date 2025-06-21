using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class City
    {
        public int Id {  get; set; }

        [MaxLength(255)]
        public string Name { get; set; }       

        public List<Trip> ArrivalPoints { get; set; } = new();

        public List<Trip> DeparturePoints { get; set; } = new();
    }
}
