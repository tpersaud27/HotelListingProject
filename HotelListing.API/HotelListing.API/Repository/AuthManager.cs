using AutoMapper;
using HotelListing.API.Contracts;
using HotelListing.API.Data;
using HotelListing.API.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization.Metadata;

namespace HotelListing.API.Repository
{
    public class AuthManager : IAuthManager
    {
        private readonly IMapper _mapper;
        private readonly UserManager<ApiUser> _userManager;
        // Adding the configuration to access the Jwt Key
        private readonly IConfiguration _configuration;

        public AuthManager(IMapper mapper, UserManager<ApiUser> userManager, IConfiguration configuration)
        {
            this._mapper = mapper;
            this._userManager = userManager;
            this._configuration = configuration;
        }

        /// <summary>
        /// Validating the User exists in the system. This is more validation than loggin in persei
        /// </summary>
        /// <param name="userDto"></param>
        /// <returns></returns>
        public async Task<AuthResponseDto> Login(LoginDto loginDto)
        {
            // First lets check if the user exists 
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            // userManager will check if the user password is valid for the user
            bool isValidUser = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if(user == null || isValidUser == false)
            {
                return null;
            }

            // Token string
            var token = await GenerateToken(user);

            // Sending back information when logging in
            return new AuthResponseDto
            {
                Token = token,
                UserId = user.Id
            };
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

        private async Task<string> GenerateToken(ApiUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // This will go to the DB and get the user role
            var roles = await _userManager.GetRolesAsync(user);

            // Give the name of the claim and add the value from the Db which is x
            var roleClaims = roles.Select(x => new Claim(ClaimTypes.Role, x)).ToList();

            // Fetching claims in the DB, if they are stored on the DB level
            var userClaims = await _userManager.GetClaimsAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id),
            }
            .Union(userClaims).Union(roleClaims);

            // Create the token
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["Jwt:DurationInMinutes"])),
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        } 
    }
}
