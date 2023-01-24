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
        private ApiUser _user;

        private const string _loginProvider = "HotelListingApi";
        private const string _refreshToken = "RefreshToken";

        public AuthManager(IMapper mapper, UserManager<ApiUser> userManager, IConfiguration configuration)
        {
            this._mapper = mapper;
            this._userManager = userManager;
            this._configuration = configuration;
        }

        /// <summary>
        /// Generates a new token. First removing existing token in db and then adding a newly generated token
        /// </summary>
        /// <returns></returns>
        public async Task<string> CreateRefreshToken()
        {
            // First we remove the existing token
            await _userManager.RemoveAuthenticationTokenAsync(_user, _loginProvider, _refreshToken);
            // Generate new token
            var newRefreshToken = await _userManager.GenerateUserTokenAsync(_user, _loginProvider, _refreshToken);
            // Sets the token in the database
            var result = await _userManager.SetAuthenticationTokenAsync(_user, _loginProvider, _refreshToken, newRefreshToken);
            // Return the token
            return newRefreshToken;

        }

        /// <summary>
        /// Validating the User exists in the system. This is more validation than loggin in persei
        /// </summary>
        /// <param name="userDto"></param>
        /// <returns></returns>
        public async Task<AuthResponseDto> Login(LoginDto loginDto)
        {
            // First lets check if the user exists 
            _user = await _userManager.FindByEmailAsync(loginDto.Email);

            // userManager will check if the user password is valid for the user
            bool isValidUser = await _userManager.CheckPasswordAsync(_user, loginDto.Password);

            if(_user == null || isValidUser == false)
            {
                return null;
            }

            // Token string
            var token = await GenerateToken();

            // Sending back information when logging in
            return new AuthResponseDto
            {
                Token = token,
                UserId = _user.Id,
                RefreshToken = await CreateRefreshToken()
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
            _user = _mapper.Map<ApiUser>(userDto);

            _user.UserName = _user.Email;

            // Create user
            // userManager has some built in methods that we can use
            var result = await _userManager.CreateAsync(_user, userDto.Password);

            // If the creation was successful we want to add the user to the role User
            if(result.Succeeded)
            {
                await _userManager.AddToRoleAsync(_user, "User");
            }

            // Success returns nothing, if not return errors
            return result.Errors;
        }

        public async Task<AuthResponseDto> VerifyRefreshToken(AuthResponseDto request)
        {
            // Get the token that came in from the request
            var jwtSecurityHandler = new JwtSecurityTokenHandler();
            var tokenContent = jwtSecurityHandler.ReadJwtToken(request.Token);

            // Get the username from the token
            var userName = tokenContent.Claims.ToList().FirstOrDefault(q => q.Type == JwtRegisteredClaimNames.Email)?.Value;
            // Find the user based on the username
            _user = await _userManager.FindByEmailAsync(userName);

            if(userName != null || _user.Id != request.UserId )
            {
                return null;
            }

            // Check if the token is in the db
            var isValidRefreshToken = await _userManager.VerifyUserTokenAsync(_user, _loginProvider, _refreshToken, request.RefreshToken);

            if(isValidRefreshToken)
            {
                var token = await GenerateToken();
                return new AuthResponseDto()
                {
                    Token = token,
                    UserId = _user.Id,
                    RefreshToken = await CreateRefreshToken()
                };
            }

            await _userManager.UpdateSecurityStampAsync(_user);
            return null;

        }

        private async Task<string> GenerateToken()
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // This will go to the DB and get the user role
            var roles = await _userManager.GetRolesAsync(_user);

            // Give the name of the claim and add the value from the Db which is x
            var roleClaims = roles.Select(x => new Claim(ClaimTypes.Role, x)).ToList();

            // Fetching claims in the DB, if they are stored on the DB level
            var userClaims = await _userManager.GetClaimsAsync(_user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, _user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, _user.Email),
                new Claim("uid", _user.Id),
            }
            .Union(userClaims).Union(roleClaims);

            // Create the token
            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["JwtSettings:DurationInMinutes"])),
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        } 
    }
}
