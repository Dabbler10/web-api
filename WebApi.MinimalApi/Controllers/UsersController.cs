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
    [HttpGet("{userId}")]
    public ActionResult<UserDto> GetUserById([FromRoute] Guid userId)
    {
        var user = _userRepository.FindById(userId);
        if (user == null)
            return NotFound();
        var userDto = _mapper.Map<UserDto>(user);
        return Ok(userDto);
     
    }

    [HttpPost]
    public IActionResult CreateUser([FromBody] object user)
    {
        throw new NotImplementedException();
    }
}