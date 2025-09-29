using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApi.MinimalApi.Domain;
using WebApi.MinimalApi.Models;

namespace WebApi.MinimalApi.Controllers;

[Route("api/[controller]")]
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
    public IActionResult CreateUser([FromBody] UserPost user)
    {
        var userEntity = _mapper.Map<UserEntity>(user);
        return CreatedAtRoute(
            nameof(GetUserById),
            new { userId = userEntity.Id },
            userEntity);
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
}