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
    [HttpHead("{userId}")]
    public ActionResult<UserDto> GetUserById([FromRoute] Guid userId)
    {
        var user = _userRepository.FindById(userId);
        if (user == null)
            return NotFound();
        var userDto = _mapper.Map<UserDto>(user);
        if (HttpMethods.IsHead(Request.Method))
        {
            Response.ContentType = $"{Request.Headers.Accept}; charset=utf-8";
            return Ok();
        }

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
            ModelState.AddModelError(nameof(user.Login), "Login should contain only letters or digits");
            return UnprocessableEntity(ModelState);
        }
        
        var userEntity = _mapper.Map<UserEntity>(user);
        var userEntityWithId = _userRepository.Insert(userEntity);
        return CreatedAtRoute(
            nameof(GetUserById),
            new { userId = userEntityWithId.Id },
            userEntityWithId.Id);
    }

    [HttpPut("{userId}")]
    public IActionResult UpdateUser([FromRoute] Guid userId, [FromBody] UserPut? updateDto)
    {
        if (updateDto is null || userId == Guid.Empty)
            return BadRequest();
        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);
        var isNew = false;
        var userEntity = _userRepository.FindById(userId);
        if (userEntity == null)
        {
            isNew = true;
            userEntity = new UserEntity(userId);
        }
        
        userEntity.Login = updateDto.Login;
        userEntity.FirstName = updateDto.FirstName;
        userEntity.LastName = updateDto.LastName;

        _userRepository.UpdateOrInsert(userEntity, out var success);

        return isNew ? Created("user", userEntity) : NoContent();
    }
    
    [HttpPatch("{userId}")]
    public IActionResult PartiallyUpdateUser([FromRoute] Guid userId, [FromBody] JsonPatchDocument<UserPut>? patchDoc)
    {
        if (patchDoc is null)
            return BadRequest();
        
        var user = _userRepository.FindById(userId);
        if (user is null)
            return NotFound();
        
        var updateDto = _mapper.Map<UserPut>(user);
        patchDoc.ApplyTo(updateDto, ModelState);
        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);
        if (!TryValidateModel(updateDto))
            return UnprocessableEntity(ModelState);
        
        user.Login = updateDto.Login;
        user.FirstName = updateDto.FirstName;
        user.LastName = updateDto.LastName;
        _userRepository.Update(user);

        return NoContent();
    }

    [HttpDelete("{userId}")]
    public IActionResult DeleteUser([FromRoute] Guid userId)
    {
        var user = _userRepository.FindById(userId);
        if (user == null)
            return NotFound();
        _userRepository.Delete(userId);
        return NoContent();
    }
}