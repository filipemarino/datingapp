using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
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
        private readonly IMapper _mapper;

        public AuthController (IAuthRepository authRepository, IConfiguration configuration,
            IMapper mapper) {
            _mapper = mapper;
            _configuration = configuration;
            _authRepository = authRepository;

        }

        [HttpPost ("register")]
        public async Task<IActionResult> Register ([FromBody] UserForRegisterDto userData) {
            userData.Username = userData.Username.ToLower ();

            if (await _authRepository.UserExists (userData.Username))
                return BadRequest ("Username already exists");

            var userToCreate = _mapper.Map<User>(userData);

            var createdUser = await _authRepository.Register (userToCreate, userData.Password);

            var userToReturn = _mapper.Map<UserForDetailDto>(createdUser);

            return CreatedAtRoute("GetUser", new 
            { 
                controller = "User", id = createdUser.Id 
            }, userToReturn);
        }

        [HttpPost ("login")]
        public async Task<IActionResult> Login ([FromBody] UserForLoginDto userForLoginDto) {
            var userFromRepo = await _authRepository.Login (userForLoginDto.Username.ToLower (),
                userForLoginDto.Password);

            if (userFromRepo == null)
                return Unauthorized ();

            var claims = new [] {
                new Claim (ClaimTypes.NameIdentifier, userFromRepo.Id.ToString ()),
                new Claim (ClaimTypes.Name, userFromRepo.Username)
            };

            var key = new SymmetricSecurityKey (Encoding.ASCII
                .GetBytes (_configuration.GetSection ("AppSettings:Token").Value));

            var creds = new SigningCredentials (key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity (claims),
                Expires = DateTime.Now.AddDays (1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler ();
            var token = tokenHandler.CreateToken (tokenDescriptor);

            var user = _mapper.Map<UserForListDto>(userFromRepo);

            return Ok (new {
                token = tokenHandler.WriteToken (token),
                user
            });
        }
    }
}