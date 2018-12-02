using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
    [Route("api/users/{userId}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private Cloudinary _cloudinary;

        public PhotosController(IDatingRepository repo, IMapper mapper,
            IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _cloudinaryConfig = cloudinaryConfig;
            _mapper = mapper;
            _repo = repo;

            Account acc = new Account(
                _cloudinaryConfig.Value.CloudName,
                _cloudinaryConfig.Value.ApiKey,
                _cloudinaryConfig.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(acc);
        }

        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photoFromRepo = await _repo.GetPhoto(id);

            var photo = _mapper.Map<PhotoForReturnDto>(photoFromRepo);

            return Ok(photo); 
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId, 
            [FromForm]PhotoForCreationDto photoForCreationDto)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var userFromRepo = await _repo.GetUser(userId, true);
             
            // photo from upload form
            var file = photoForCreationDto.File;
            
            var uploadResult = new ImageUploadResult(); 
            
            if (file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    // prepare image object upload to cloudinary
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation()
                            .Width(500).Height(500).Crop("fill").Gravity("face")

                    };
                    
                    // upload image and get the result back (e.g. Uri and publicId)
                    uploadResult = _cloudinary.Upload(uploadParams);
                }
            }

            // add Uri and PublicId to photo creation object
            photoForCreationDto.Url = uploadResult.Uri.ToString();
            photoForCreationDto.PublicId = uploadResult.PublicId;

            // map photo creation object to photo object (databse)
            var photo = _mapper.Map<Photo>(photoForCreationDto);

            // mark photo as main, if it is the first upload/photo
            if (!userFromRepo.Photos.Any(u => u.IsMain))
                photo.IsMain = true;

            // add photo to user object
            userFromRepo.Photos.Add(photo); 

            // save back to database
            // if succeed, we have an (photo) id
            if(await _repo.SaveAll())
            {
                // prepare photo object for return without user object (relationship)
                var photoForReturn = _mapper.Map<PhotoForReturnDto>(photo);
                // return a "201 Created" by execute a HttpGet with routename "GetPhoto" 
                // This task will get the photo(ForReturn) object by its id
                return CreatedAtRoute("GetPhoto", new { id = photo.Id }, photoForReturn);
            }

            return BadRequest("Could add the photo");
        }

        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var user = await _repo.GetUser(userId, true);

            if (!user.Photos.Any(p => p.Id == id))
                return Unauthorized();

            var photoFromRepo = await _repo.GetPhoto(id);
            if (photoFromRepo.IsMain)
                return BadRequest("This is already the main photo!");

            var currentMainPhoto = await _repo.GetMainPhotoForUser(userId);
            // remove main attribute from old one
            currentMainPhoto.IsMain = false;
            // set main attribute to new photo 
            photoFromRepo.IsMain = true;
            
            if(await _repo.SaveAll())
                return NoContent();

            return BadRequest("Could not set photo to main!");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int id) 
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var user = await _repo.GetUser(userId, true);

            if (!user.Photos.Any(p => p.Id == id))
                return Unauthorized();

            var photoFromRepo = await _repo.GetPhoto(id);

            if (photoFromRepo.IsMain) 
                return BadRequest("You cannot delete your main photo!");

            // check, whether the photo is store on cloud storage 
            if (photoFromRepo.PublicId != null) {
                var deleteParams = new DeletionParams(photoFromRepo.PublicId);
                var result = _cloudinary.Destroy(deleteParams);

                if (result.Result == "ok") {
                    _repo.Delete(photoFromRepo);
                }
            } else {
                _repo.Delete(photoFromRepo);
            }
            
            if(await _repo.SaveAll())
                return Ok();

            return BadRequest("Failed to delete the photo!");
        }
    }
}