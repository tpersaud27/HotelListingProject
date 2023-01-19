using AutoMapper;
using HotelListing.API.Contracts;
using HotelListing.API.Data;
using HotelListing.API.Models.Users;
using Microsoft.AspNetCore.Identity;

namespace HotelListing.API.Repository
{
    public class AuthManager : IAuthManager
    {
        private readonly IMapper _mapper;
        private readonly UserManager<ApiUser> _userManager;

        public AuthManager(IMapper mapper, UserManager<ApiUser> userManager)
        {
            this._mapper = mapper;
            this._userManager = userManager;
        }

        /// <summary>
        /// Allows registration to User role
        /// </summary>
        /// <param name="userDto"></param>
        /// <returns>List of errors</returns>
        public async Task<IEnumerable<IdentityError>> Register(ApiUserDto userDto)
        {
            // User object of type ApiUser
            var user = _mapper.Map<ApiUser>(userDto);

            user.UserName = user.Email;

            // Create user
            // userManager has some built in methods that we can use
            var result = await _userManager.CreateAsync(user, userDto.Password);

            // If the creation was successful we want to add the user to the role User
            if(result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
            }

            // Success returns nothing, if not return errors
            return result.Errors;
        }
    }
}
