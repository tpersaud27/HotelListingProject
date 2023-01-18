using HotelListing.API.Data.Configurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.API.Data
{
    public class HotelListingDbContext: IdentityDbContext<ApiUser>
    {
        /// <summary>
        /// Note the options listed here are coming from the configuration
        /// </summary>
        /// <param name="options"></param>
        public HotelListingDbContext(DbContextOptions options): base(options)
        {

        }

        public DbSet<Hotel> Hotels { get; set; }

        public DbSet<Country> Countries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Here we are adding the role configuration (Some initial data)
            modelBuilder.ApplyConfiguration(new RoleConfigration());

            // This is seeding some initial data into the database
            modelBuilder.ApplyConfiguration(new CountryConfiguration());

            // Seeding data for Hotels table
            modelBuilder.ApplyConfiguration(new HotelConfiguration());
        }

    }
}
