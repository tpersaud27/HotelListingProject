using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelListing.API.Data;
using HotelListing.API.Models.Country;
using AutoMapper;
using HotelListing.API.Contracts;
using Microsoft.AspNetCore.Authorization;
using HotelListing.API.Exceptions;
using HotelListing.API.Models;
using Microsoft.AspNetCore.OData.Query;

namespace HotelListing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {

        private readonly IMapper _mapper;
        private readonly ICountriesRepository _countriesRepository;
        private readonly ILogger<CountriesController> _logger;


        // Injecting Dependencies 
        public CountriesController(IMapper mapper, ICountriesRepository countriesRepository, ILogger<CountriesController> logger)
        {
            _mapper = mapper;
            _countriesRepository = countriesRepository;
            _logger = logger;
        }

        // GET: api/Countries/GetAll
        [HttpGet("GetAll")]
        [EnableQuery]
        public async Task<ActionResult<IEnumerable<GetCountryDto>>> GetCountries()
        {
            // Note: This implementation show below is using the repository methods that return the entity, forcing us to do the mapping here.
            // We can instead use the generic DTO methods
            // Getting the data from the DB
            //var countries = await _countriesRepository.GetAllAsync();
            //// Map countries to GetCountryDTO
            //var records = _mapper.Map<List<GetCountryDto>>(countries);
            //// Return 200 response code
            //return Ok(records);

            // This implementation is using the generic methods that return the DTO at the repository level, allows us to just return what is necessary 
            // No mapping is needed here
            var countries = await _countriesRepository.GetAllAsync<GetCountryDto>();
            return Ok(countries);
        }

        // GET: api/Countries/?StartIndex=0&PageSize-25&PageNumber=1
        [HttpGet]
        public async Task<ActionResult<PagedResult<GetCountryDto>>> GetPagedCountries([FromQuery] QueryParameters queryParameters)
        {
            // Getting the data from the DB
            // Note the mapping is done at the repository level
            var pagedCountriesResult = await _countriesRepository.GetAllAsync<GetCountryDto>(queryParameters);
            // Return 200 response code
            return Ok(pagedCountriesResult);
        }

        // GET: api/Countries/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GetCountryDetailsDto>> GetCountry(int id)
        {
            var country = await _countriesRepository.GetDetails(id);
            
            // Note: This code is not needed as we now do the mapping at the repository level
            // If the country does not exist
            //if (country == null)
            //{
            //    throw new NotFoundException(nameof(GetCountry),id);
            //}
            //// Convert to countryDTO
            //var countryDTO = _mapper.Map<GetCountryDetailsDto>(country);

            // Return the country found
            return Ok(country);

        }

        // PUT: api/Countries/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutCountry(int id, UpdateCountryDto updateCountryDto)
        {

            if (id != updateCountryDto.Id)
            {
                return BadRequest("Invalid Record Id");
            }
            // Note: This commented code can be replaced using the shortened code.
            // The mapping is done at the repository level
            //// This will change the state to modified
            ////_context.Entry(updateCountryDto).State = EntityState.Modified;
            //// Fetch record for Db
            //var country = await _countriesRepository.GetAsync(id);
            //if (country == null)
            //{
            //    throw new NotFoundException(nameof(GetCountry), id);
            //}
            //// Take all the fields that maps from updateCountryDto and update them in country object
            //// This mapper line is telling entity framework that this is being modified
            //// Takes all of the info from updateCountryDto and updates the matching fields in country
            //_mapper.Map(updateCountryDto, country);

            // If two users are trying to update the record at the same time we should be checking for that. (One user editing record, while one is deleting that same record)
            try
            {
                await _countriesRepository.UpdateAsync<UpdateCountryDto>(id, updateCountryDto);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await CountryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            // 204 Status Code
            return NoContent();
        }

        // POST: api/Countries
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Country>> PostCountry(CreateCountryDto createCountryDTO)
        {
            // Creating country object using DTO. This is mapping the DTO model to the Country model
            /*var countryOld = new Country
            {
                Name = createCountryDTO.Name,
                ShortName = createCountryDTO.ShortName
            };*/

            // Mapping the createCountryDTO to a Country object using automapper
            var country = _mapper.Map<Country>(createCountryDTO);

            await _countriesRepository.AddAsync(country);

            return CreatedAtAction("GetCountry", new { id = country.Id }, country);
        }

        // DELETE: api/Countries/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            var country = await _countriesRepository.GetAsync(id);
            if (country == null)
            {
                throw new NotFoundException(nameof(GetCountry), id); ;
            }

            await _countriesRepository.DeleteAsync(id);

            return NoContent();
        }

        private async Task<bool> CountryExists(int id)
        {
            return await _countriesRepository.Exists(id);
        }
    }
}
