
using AutoMapper;
using HotelListing.API.Data;
using HotelListing.API.Models.Country;
using HotelListing.API.Models.Hotel;

namespace HotelListing.API.Configurations
{
    public class AutomapperConfig: Profile
    {
        // This constructor allows us to create maps between our data types
        public AutomapperConfig() 
        {

            // Reverse map allows us to map in either direction
            CreateMap<Country, CreateCountryDto>().ReverseMap();
            CreateMap<Country, GetCountryDto>().ReverseMap();
            CreateMap<Country, GetCountryDetailsDto>().ReverseMap();
            CreateMap<Country, UpdateCountryDto>().ReverseMap();


            CreateMap<Hotel, GetHotelDto>().ReverseMap();
            CreateMap<Hotel, UpdateHotelDto>().ReverseMap();
            CreateMap<Hotel, CreateCountryDto>().ReverseMap();


        }
    }
}
