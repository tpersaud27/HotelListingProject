using HotelListing.API.Models.Hotel;

namespace HotelListing.API.Models.Country
{
    public class GetCountryDetailsDto
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public List<GetHotelDto> Hotels { get; set; }
    }
}
