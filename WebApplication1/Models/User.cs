using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class User
    {
        public User() { }
        public User(string log,string pass) {

            Login = log;
            Password = pass;
        }

        public int Id { get; set; }

        [MaxLength(255)]
        public string Email { get; set; }

        [MaxLength(255)]
        public string Login { get; set; }

        [MaxLength(255)]
        public string Password { get; set; }

        [MaxLength(255)]
        public string? Name { get; set; }

        [MaxLength(255)]
        public string? Surname { get; set; }

        [MaxLength(255)]
        public string? PhoneNumber { get; set; }

        [MaxLength(255)]
        public string Role { get; set; }

        public byte[]? DriverLicense { get; set; }
        
        public int? AmountOfBadReports { get; set; }

        public int? AmountOfReports { get; set; }

        public float? Rating { get; set; }

        public bool? isBanned {  get; set; }
    }
}