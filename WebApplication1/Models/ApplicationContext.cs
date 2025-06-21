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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Trip>().HasOne(t => t.ArrivalPoint).WithMany(c => c.ArrivalPoints).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Trip>().HasOne(t => t.DeparturePoint).WithMany(c => c.DeparturePoints).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Report>().HasOne(r => r.Reporter).WithMany(u => u.Reporteds).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Report>().HasOne(r => r.Creator).WithMany(u => u.Reporters).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TripRegistration>().HasOne(tr => tr.User).WithMany(u => u.Registrations).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
