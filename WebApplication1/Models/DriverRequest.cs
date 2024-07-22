using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class DriverRequest
    {
        public int Id { get; set; }
        [MaxLength(255)]
        public string Name { get; set; }
        [MaxLength(255)]
        public string Surname { get; set; }
              
        public byte[] DriverLicense { get; set; }
        [MaxLength(255)]
        public string PhoneNumber { get; set; }
        [MaxLength(255)]
        public string Status { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}
