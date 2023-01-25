using AutoMapper;
using AutoMapper.QueryableExtensions;
using HotelListing.API.Contracts;
using HotelListing.API.Data;
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

        public async Task DeleteAsync(int id)
        {
            // Find the entity
            var entity = await GetAsync(id);
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

        public async Task<T> GetAsync(int? id)
        {
            if (id is null)
            {
                return null;
            }

            return await _context.Set<T>().FindAsync(id);
        }

        public async Task UpdateAsync(T entity)
        {
            _context.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
