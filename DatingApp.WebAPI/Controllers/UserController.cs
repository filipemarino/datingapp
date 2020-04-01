using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.WebAPI.Context;
using DatingApp.WebAPI.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace DatingApp.WebAPI.Controllers {
    [Route ("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase {
        private readonly IDatingRepository _datingRepository;
        private readonly IMapper _mapper;
        public UserController (IDatingRepository datingRepository, IMapper mapper) {
            _mapper = mapper;
            _datingRepository = datingRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers ()
        {
            var users = await _datingRepository.GetUsers ();
            var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);
            return Ok (usersToReturn);
        }

        [HttpGet ("{id}")]
        public async Task<IActionResult> GetUser (int id)
        {
            var user = await _datingRepository.GetUser (id);
            var userToReturn = _mapper.Map<UserForDetailDto>(user);

            return Ok (userToReturn);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody]UserForUpdateDto userForUpdateDto)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var userFromRepo = await _datingRepository.GetUser(id);

            _mapper.Map(userForUpdateDto, userFromRepo);

            if (await _datingRepository.SaveAll())
            {
                return NoContent();
            }

            throw new Exception($"Updating user {id} failed on save");
        }
    }
}