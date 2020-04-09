using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.WebAPI.Context;
using DatingApp.WebAPI.DTO;
using DatingApp.WebAPI.Helpers;
using DatingApp.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.WebAPI.Controllers {
    [Route ("api/users/{userId}/photos")]
    [ApiController]
    [Authorize]
    public class PhotosController : ControllerBase {
        private readonly IDatingRepository _datingRepository;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfigs;
        private Cloudinary _cloudinary;

        public PhotosController (IDatingRepository datingRepository, IMapper mapper, 
        IOptions<CloudinarySettings> cloudinaryConfigs) {
            _cloudinaryConfigs = cloudinaryConfigs;
            _mapper = mapper;
            _datingRepository = datingRepository;

            Account acc = new Account(
                _cloudinaryConfigs.Value.CloudName,
                _cloudinaryConfigs.Value.API_Key,
                _cloudinaryConfigs.Value.API_Secret
            );

            _cloudinary = new Cloudinary(acc);
        }

        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> Get(int id)
        {
            var photo = await _datingRepository.GetPhoto(id);

            var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo);

            return Ok(photoToReturn);
        }


        [HttpPost]
        public async Task<IActionResult> Post(int userId, [FromForm]PhotoForCreationDto photoForCreationDto)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            var userFromRepo = await _datingRepository.GetUser(userId);
            
            var file = photoForCreationDto.File;

            var uploadResult = new ImageUploadResult();

            if (file.Length > 0)
            {
                using(var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                    };

                    uploadResult = _cloudinary.Upload(uploadParams);
                }
            }

            photoForCreationDto.UrlPhoto = uploadResult.Uri.ToString();
            photoForCreationDto.PublicId = uploadResult.PublicId;

            var photo = _mapper.Map<Photo>(photoForCreationDto);

            if (!userFromRepo.Photos.Any(u => u.IsMain))
            {
                photo.IsMain = true;
            }

            userFromRepo.Photos.Add(photo);

            if (await _datingRepository.SaveAll())
            {
                var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo);
                return CreatedAtRoute("GetPhoto", new { userId = userId, id = photo.Id }, 
                photoToReturn);
            }

            return BadRequest("Could not add the photo");
        }

        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var user = await _datingRepository.GetUser(userId);

            if (!user.Photos.Any(x => x.Id == id))
            {
                return Unauthorized();
            }

            var photo = await _datingRepository.GetPhoto(id);

            if (photo.IsMain)
            {
                return BadRequest("This is already the main photo");
            }

            var currentMainPhoto = await _datingRepository.GetMainPhotoForUser(userId);
            currentMainPhoto.IsMain = false;
            
            photo.IsMain = true;
            
            if (await _datingRepository.SaveAll())
            {
                return NoContent();
            }

            return BadRequest("Could not set photo to main");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var user = await _datingRepository.GetUser(userId);

            if (!user.Photos.Any(x => x.Id == id))
            {
                return Unauthorized();
            }

            var photo = await _datingRepository.GetPhoto(id);

            if (photo.IsMain)
            {
                return BadRequest("You cannot delete you main photo");
            }

            if (!string.IsNullOrEmpty(photo.PublicId))
            {
                var result = _cloudinary.Destroy(new DeletionParams(photo.PublicId));

                if (result.Result == "ok"){
                    _datingRepository.Delete(photo);
                }
            }
            else
            {
                _datingRepository.Delete(photo);
            }

            if (await _datingRepository.SaveAll()) {
                return Ok();
            }

            return BadRequest("Failed to delete the photo");
        }
    }
} 