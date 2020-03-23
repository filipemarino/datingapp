using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.WebAPI.Context;
using DatingApp.WebAPI.DTO;
using DatingApp.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.WebAPI.Controllers {
    [Route ("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;

        public AuthController (IAuthRepository authRepository, IConfiguration configuration) {
            _configuration = configuration;
            _authRepository = authRepository;

        }

        [HttpPost ("register")]
        public async Task<IActionResult> Register ([FromBody] UserForRegisterDto userData) {
            userData.Username = userData.Username.ToLower ();

            if (await _authRepository.UserExists (userData.Username))
                return BadRequest ("Username already exists");

            var userToCreate = new User {
                Username = userData.Username
            };

            var createdUser = await _authRepository.Register (userToCreate, userData.Password);

            return StatusCode (201);
        }

        [HttpPost ("login")]
        public async Task<IActionResult> Login ([FromBody] UserForLoginDto userForLoginDto) {
            var userFromRepo = await _authRepository.Login(userForLoginDto.Username.ToLower(), 
                            userForLoginDto.Password);

            if (userFromRepo == null)
                return Unauthorized ();

            var claims = new [] {
                new Claim (ClaimTypes.NameIdentifier, userFromRepo.Id.ToString ()),
                new Claim (ClaimTypes.Name, userFromRepo.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.ASCII
                    .GetBytes(_configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new 
            {
                token = tokenHandler.WriteToken(token)
            });
        }
    }
}