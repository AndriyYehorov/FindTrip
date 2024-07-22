using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace WebApplication1.Models
{
    public class TripRegistration
    {
        public TripRegistration()
        {
        }
        public TripRegistration(int userId,int tripId )
        {
            UserId = userId;
            TripId = tripId;
        }

        public int Id { get; set; }
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        public int TripId { get; set; }

        [ForeignKey("TripId")]
        public Trip Trip { get; set; }

    }
}
