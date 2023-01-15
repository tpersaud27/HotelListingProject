namespace HotelListing.API.Data
{
    // One Country can have many hotels
    public class Country
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; } 

        // When entity framework sees this they will know the relationship for country to hotel is one to many
        public virtual IList<Hotel> Hotels{ get; set; }
    }
}