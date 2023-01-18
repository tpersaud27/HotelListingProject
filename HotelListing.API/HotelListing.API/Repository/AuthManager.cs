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

        public async Task<IEnumerable<IdentityError>> Register(ApiUserDto userDto)
        {
            // User object of type ApiUser
            var user = _mapper.Map<ApiUser>(userDto);

            user.UserName = user.Email;

            // Create user
            // userManager has some built in methods that we can use
            var result = await _userManager.CreateAsync(user, userDto.Password);

            // This will have content is it has errors, no content otherwise
            return result.Errors;
        }
    }
}
