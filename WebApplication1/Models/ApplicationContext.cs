using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Trip> Trips { get; set; }        
        public DbSet<City> Cities { get; set; }
        public DbSet<DriverRequest> Requests { get; set; }
        public DbSet<TripRegistration> TripRegistrations { get; set; }
        public DbSet<Report> Reports { get; set; }
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
            try
            {
                //Database.EnsureCreated();
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Cannot connect to database");                
            }
        }
    }
}
