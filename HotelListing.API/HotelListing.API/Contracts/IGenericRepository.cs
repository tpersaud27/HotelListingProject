using HotelListing.API.Models;
using System.Runtime.CompilerServices;

namespace HotelListing.API.Contracts
{
    public interface IGenericRepository<T> where T: class
    {
        // Note:
        // The methods using TResult allow us to minimize mapping done in the controller
        // Allows us to directly map to the DTO here instead

        Task<T> GetAsync(int? id);

        // Generic method to get using ID using DTO
        Task<TResult?> GetAsync<TResult>(int? id);

        Task<List<T>> GetAllAsync();

        // Generic method to get all using DTO
        Task<List<TResult>> GetAllAsync<TResult>();

        Task<PagedResult<TResult>> GetAllAsync<TResult>(QueryParameters queryParameters);

        Task<T> AddAsync(T entity);

        // Generic method to add using DTO
        Task<TResult> AddAsync<TSource, TResult>(TResult source);

        Task DeleteAsync(int id);

        Task UpdateAsync(T entity);

        // Generic method to update using DTO
        Task UpdateAsync<TSource>(int id, TSource source);

        Task<bool> Exists(int id);

    }
}
