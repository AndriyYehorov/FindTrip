using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Report
    {
        public int Id { get; set; }

        public string? Text { get; set; }

        public int Rating { get; set; }
        public int CreatorId { get; set; }
        
        public int ReportedId { get; set; }

        public int TripId { get; set; }        

        [ForeignKey("CreatorId")]
        public User Creator { get; set; }

        [ForeignKey("ReportedId")]
        public User Reporter { get; set; }

		[ForeignKey("TripId")]
		public Trip Trip { get; set; }
	}
}
