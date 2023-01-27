using System.ComponentModel.DataAnnotations.Schema;

namespace HotelListing.API.Data
{
    // This class represents a entity, this entity is used to model a table in the database
    public class Hotel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public double Rating { get; set; }

        // This will be a foreign key for the table Country
        [ForeignKey(nameof(CountryId))]
        public int CountryId { get; set; }
        public Country Country { get; set; }
    }
}
