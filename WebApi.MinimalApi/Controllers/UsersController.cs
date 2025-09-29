using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using WebApi.MinimalApi.Domain;
using WebApi.MinimalApi.Models;

namespace WebApi.MinimalApi.Controllers;

[Route("api/[controller]")]
[Produces("application/json", "application/xml")]
[ApiController]
public class UsersController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    public UsersController(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }
    
    [Produces("application/json", "application/xml")]
    [HttpGet("{userId}", Name = nameof(GetUserById))]
    public ActionResult<UserDto> GetUserById([FromRoute] Guid userId)
    {
        var user = _userRepository.FindById(userId);
        if (user == null)
            return NotFound();
        var userDto = _mapper.Map<UserDto>(user);
        return Ok(userDto);
     
    }

    [HttpPost]
    public IActionResult CreateUser([FromBody] UserPost? user)
    {
        if (user is null)
            return BadRequest();
        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);

        if (!user.Login.All(char.IsLetterOrDigit))
        {
            ModelState.AddModelError(nameof(user.Login), "Сообщение об ошибке");
            return UnprocessableEntity(ModelState);
        }
        
        var userEntity = _mapper.Map<UserEntity>(user);
        _userRepository.Insert(userEntity);
        return CreatedAtRoute(
            nameof(GetUserById),
            new { userId = userEntity.Id },
             userEntity.Id);
    }

    [HttpPut("users/{userId}")]
    public IActionResult UpdateUser([FromRoute] Guid userId, [FromBody] UserPut updateDto)
    {
        var userEntity = _userRepository.FindById(userId);

        if (userEntity == null)
            return NotFound();
        if (updateDto.Login != null)
            userEntity.Login = updateDto.Login;
        if (updateDto.FirstName != null)
            userEntity.FirstName = updateDto.FirstName;
        if (updateDto.LastName != null)
            userEntity.LastName = updateDto.LastName;

        _userRepository.UpdateOrInsert(userEntity, out var success);

        return Ok(success);
    }
    
    // [HttpPatch("{userId}")]
    // public IActionResult PartiallyUpdateUser([FromRoute] Guid userId, [FromBody] JsonPatchDocument<UpdateDto> patchDoc)
    // {
    //     var user = _userRepository.FindById(userId);
    //     
    //     patchDoc.ApplyTo(updateDto, ModelState);
    //     TryValidateModel(user);
    // }
}