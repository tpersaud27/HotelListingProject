﻿using System;
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

namespace HotelListing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {

        private readonly IMapper _mapper;
        private readonly ICountriesRepository _countriesRepository;


        // Injecting Dependencies 
        public CountriesController(IMapper mapper, ICountriesRepository countriesRepository)
        {
            _mapper = mapper;
            _countriesRepository = countriesRepository;
        }

        // GET: api/Countries
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetCountryDto>>> GetCountries()
        {
            // Getting the data from the DB
            var countries = await _countriesRepository.GetAllAsync();

            // Map countries to GetCountryDTO
            var records = _mapper.Map<List<GetCountryDto>>(countries);
            // Return 200 response code
            return Ok(records);
        }

        // GET: api/Countries/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GetCountryDetailsDto>> GetCountry(int id)
        {
            var country = await _countriesRepository.GetDetails(id);
            
            // If the country does not exist
            if (country == null)
            {
                // Notify the user the country is not found
                // This is a 404 status code
                return NotFound();
            }

            // Convert to countryDTO
            var countryDTO = _mapper.Map<GetCountryDetailsDto>(country);

            // Return the country found
            return Ok(countryDTO);
        }

        // PUT: api/Countries/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCountry(int id, UpdateCountryDto updateCountryDto)
        {
            if (id != updateCountryDto.Id)
            {
                return BadRequest("Invalid Record Id");
            }

            // This will change the state to modified
            //_context.Entry(updateCountryDto).State = EntityState.Modified;

            // Fetch record for Db
            var country = await _countriesRepository.GetAsync(id);
            if (country == null)
            {
                return NotFound();
            }

            // Take all the fields that maps from updateCountryDto and update them in country object
            // This mapper line is telling entity framework that this is being modified
            // Takes all of the info from updateCountryDto and updates the matching fields in country
            _mapper.Map(updateCountryDto, country);

            // If two users are trying to update the record at the same time we should be checking for that. (One user editing record, while one is deleting that same record)
            try
            {
                await _countriesRepository.UpdateAsync(country);
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
        public async Task<IActionResult> DeleteCountry(int id)
        {
            var country = await _countriesRepository.GetAsync(id);
            if (country == null)
            {
                return NotFound("Record does not exist");
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
