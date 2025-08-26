using color_nodes_backend.DTOs;
using color_nodes_backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace color_nodes_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto dto)
        {
            var user = await _userService.CreateUserAsync(dto);
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UserDto>> UpdateUser(int id, UpdateUserDto dto)
        {
            var user = await _userService.UpdateUserAsync(id, dto);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var success = await _userService.DeleteUserAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
