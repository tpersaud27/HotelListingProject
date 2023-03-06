using AutoMapper;
using AutoMapper.QueryableExtensions;
using HotelListing.API.Contracts;
using HotelListing.API.Data;
using HotelListing.API.Exceptions;
using HotelListing.API.Models.Country;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.API.Repository
{
    public class CountriesRepository : GenericRepository<Country>, ICountriesRepository
    {
        private readonly HotelListingDbContext _context;
        private readonly IMapper _mapper;

        public CountriesRepository(HotelListingDbContext context, IMapper mapper) : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        /// <summary>
        /// We will return the GetCountryDetailsDto instead of the Country entity to avoid exposing unnecessary data
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<GetCountryDetailsDto> GetDetails(int id)
        {
            // FirstOrDefault will return null if the value can't be found
            // Find the country including the hotel while fetching first result with matching id
            //return await _context.Countries.Include(q => q.Hotels)
            //    .FirstOrDefaultAsync(q => q.Id == id);
            

            // Here we are using eager loading to ensure we have the hotels associated to each country
            var country = await _context.Countries.Include(q => q.Hotels)
                .ProjectTo<GetCountryDetailsDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(q => q.Id == id);

            if(country is null)
            {
                throw new NotFoundException(nameof(GetDetails), id);
            }

            return country;

        }
    }
}
