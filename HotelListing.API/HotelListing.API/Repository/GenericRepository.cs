using AutoMapper;
using AutoMapper.QueryableExtensions;
using HotelListing.API.Contracts;
using HotelListing.API.Data;
using HotelListing.API.Exceptions;
using HotelListing.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.API.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {

        private readonly HotelListingDbContext _context;
        private readonly IMapper _mapper;

        public GenericRepository(HotelListingDbContext context, IMapper mapper)
        {
            _context= context;
            _mapper = mapper;
        }

        public async Task<T> AddAsync(T entity)
        {
            // The AddSync method will deduce which type the entity is, so it will select the table accordingly
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource">This is the data we want to add.</typeparam>
        /// <typeparam name="TResult">This is the expected return type. Which is the type of DTO</typeparam>
        /// <param name="source">This data to be added is the DTO</param>
        /// <returns></returns>
        public async Task<TResult> AddAsync<TSource, TResult>(TResult source)
        {
            // This will represent a generic mapping for something like this for example
            // Example: var country = _mapper.Map<Country>(createCountryDTO);
            // Where entity is the country, the type we are mapping into is Country and the source of the data is countryDTO
            var entity = _mapper.Map<T>(source);

            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();

            // Now here instead of returning the entity -
            // We will return the DTO so we limit the information being sent back here instead of at the controller level
            // Note: The argument here is that this can lead to more latencies so we must weight
            // Whether we want to risk exposing crucial data or increasing system latency.
            return _mapper.Map<TResult>(entity);
        }

        public async Task DeleteAsync(int id)
        {
            // Find the entity
            var entity = await GetAsync(id);

            if(entity is null)
            {
                throw new NotFoundException(typeof(T).Name, id);
            }

            // Remove the entity from the DbSet
            _context.Set<T>().Remove(entity);
            // Save the changes 
            await _context.SaveChangesAsync();
        }

        public async Task<bool> Exists(int id)
        {
            // Get the entity
            var entity = await GetAsync(id);
            // Returns true if not null, false otherwise
            return entity != null;
        }

        public async Task<List<T>> GetAllAsync()
        {
            // Go to the database and get the DbSet of T and return all records in a list
            return await _context.Set<T>().ToListAsync();
        }

        // T Represents the model, TResult represents the DTO
        public async Task<PagedResult<TResult>> GetAllAsync<TResult>(QueryParameters queryParameters)
        {
            // Get the total number for records for the table in the database
            var totalSize = await _context.Set<T>().CountAsync();
            var items = await _context.Set<T>()
                .Skip(queryParameters.StartIndex)
                // Returns specified number of contiguous elements
                .Take(queryParameters.PageSize)
                // This will help us query the exact columnns we need. This can limit querying time
                .ProjectTo<TResult>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return new PagedResult<TResult>
            {
                Items = items,
                PageNumber = queryParameters.PageNumber,
                RecordNumber = queryParameters.PageSize,
                TotalCount = totalSize
            };

        }

        public async Task<List<TResult>> GetAllAsync<TResult>()
        {
            return await _context.Set<T>()
                .ProjectTo<TResult>(_mapper.ConfigurationProvider).ToListAsync();
        }

        public async Task<T> GetAsync(int? id)
        {
            if (id is null)
            {
                return null;
            }

            return await _context.Set<T>().FindAsync(id);
        }

        /// <summary>
        /// We just return the DTO instead of the entity
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        public async Task<TResult?> GetAsync<TResult>(int? id)
        {
            var result = await _context.Set<T>().FindAsync(id);

            // FindAsync can return a null value so we can check the result here 
            if (result is null)
            {
                throw new NotFoundException(typeof(T).Name, id.HasValue? id: "Not Key Provided");
            }
            
            return _mapper.Map<TResult>(result);
        }

        public async Task UpdateAsync(T entity)
        {
            _context.Update(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Update the entity with information passed in from the user and will return the appropriate DTO
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="id">This is the ID of the record we want to update</param>
        /// <param name="source">This is the data</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task UpdateAsync<TSource>(int id, TSource source)
        {

            // First we get the entity
            var entity = await GetAsync(id);

            // Check if this is a valid entity
            if(entity is null)
            {
                throw new NotFoundException(typeof(T).Name, id);
            }

            // Update the entity with the information passed in
            // Mapping the source with the denstination (entity)
            _mapper.Map(source, entity);
            _context.Update(entity);
            // Save the changes to the database
            await _context.SaveChangesAsync();
        }
    }
}
