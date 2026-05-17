using Microsoft.EntityFrameworkCore;
using GLMS.Web.Models;

namespace GLMS.Web.Data
{
    public class GLMSDbContext : DbContext
    {
        public GLMSDbContext(DbContextOptions<GLMSDbContext> options) : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed some test data so the app works right away
            modelBuilder.Entity<Client>().HasData(
                new Client { Id = 1, Name = "Acme Shipping Co", ContactDetails = "info@acme.com | +27 11 123 4567", Region = "Africa" },
                new Client { Id = 2, Name = "Global Freight Ltd", ContactDetails = "ops@globalfreight.com | +1 212 555 0100", Region = "North America" }
            );

            modelBuilder.Entity<Contract>().HasData(
                new Contract
                {
                    Id = 1, Title = "Acme Annual Freight Agreement", ClientId = 1,
                    StartDate = new DateTime(2024, 1, 1), EndDate = new DateTime(2025, 12, 31),
                    Status = ContractStatus.Active, ServiceLevel = "Premium"
                },
                new Contract
                {
                    Id = 2, Title = "Global Freight Q1 SLA", ClientId = 2,
                    StartDate = new DateTime(2024, 3, 1), EndDate = new DateTime(2024, 6, 30),
                    Status = ContractStatus.Expired, ServiceLevel = "Standard"
                }
            );
        }
    }
}
