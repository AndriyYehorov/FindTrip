using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace WebApplication1.Models
{
    public class Trip
    {
        public Trip() { }

        public Trip(           
            DateTime departureDateTime,
            City departurePoint,
            City arrivalPoint,
            int amountOfFreeSeats,
            int amountOfSeats,
            int price,
            User creator
        )
        {            
            DepartureTime = departureDateTime;
            DeparturePoint = departurePoint;
            ArrivalPoint = arrivalPoint;
            AmountOfFreeSeats = amountOfFreeSeats;
            AmountOfSeats = amountOfSeats;
            Price = price;
            Creator = creator;            
        }

        public int Id { get; set; }

        public DateTime DepartureTime { get; set; }

        [MaxLength(255)]
        public int DeparturePointId { get; set; }

        [ForeignKey("DeparturePointId")]
        public City DeparturePoint{ get; set; }

        [MaxLength(255)]
        public int ArrivalPointId { get; set;}

        [ForeignKey("ArrivalPointId")]
        public City ArrivalPoint { get; set; }

        public int AmountOfFreeSeats { get; set; }
        public int AmountOfSeats { get; set;}

        public int Price {  get; set;}

        public User Creator { get; set; }        
	}

}
