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
            int departurePoint,
            int arrivalPoint,
            int amountOfFreeSeats,
            int amountOfSeats,
            int price,
            int userId
        )
        {            
            DepartureTime = departureDateTime;
            DeparturePoint = departurePoint;
            ArrivalPoint = arrivalPoint;
            AmountOfFreeSeats = amountOfFreeSeats;
            AmountOfSeats = amountOfSeats;
            Price = price;
            UserId = userId;            
        }

        public int Id { get; set; }

        public DateTime DepartureTime { get; set; }

        [MaxLength(255)]
        public int DeparturePoint { get; set; }

        [MaxLength(255)]
        public int ArrivalPoint { get; set;}

        public int AmountOfFreeSeats { get; set; }
        public int AmountOfSeats { get; set;}

        public int Price {  get; set;}

        public int UserId { get; set; }

        [ForeignKey("UserId")]            
        public User User {  get; set; }

		[ForeignKey("DeparturePoint")]
		public City City { get; set; }

		[ForeignKey("ArrivalPoint")]
		public City City1 { get; set; }
	}

}
