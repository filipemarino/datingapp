using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.WebAPI.Context;
using DatingApp.WebAPI.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using DatingApp.WebAPI.Helpers;
using DatingApp.WebAPI.Models;

namespace DatingApp.WebAPI.Controllers {
    
    [ServiceFilter(typeof(LogUserActivity))]
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
        public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userFromRepo = await _datingRepository.GetUser(currentUserId);
            userParams.UserId = currentUserId;

            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = userFromRepo.Gender == "male" ? "female" : "male";
            }

            var users = await _datingRepository.GetUsers(userParams);
            var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);

            Response.AddPagination(users.CurrentPage, users.PageSize, 
                users.TotalCount, users.TotalPages);

            return Ok (usersToReturn);
        }

        [HttpGet ("{id}", Name = "GetUser")]
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

        [HttpPost("{id}/like/{recepientId}")]
        public async Task<IActionResult> LikeUser(int id, int recepientId)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var like = await _datingRepository.GetLike(id, recepientId);

            if (like != null)
            {
                return BadRequest("You already liked this user");
            }

            if (await _datingRepository.GetUser(recepientId) == null)
            {
                return NotFound("This user does not exist");
            }

            like = new Like
            {
                LikerId = id,
                LikeeId = recepientId
            };

            _datingRepository.Add<Like>(like);

            if (await _datingRepository.SaveAll())
            {
                return Ok();
            }

            return BadRequest("Failed to like user");
        }
    }
}