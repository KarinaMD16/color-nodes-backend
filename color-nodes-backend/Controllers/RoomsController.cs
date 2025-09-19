using color_nodes_backend.DTOs;
using color_nodes_backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace color_nodes_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public RoomController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        [HttpPost("create")]
        public async Task<ActionResult<RoomResponse>> CreateRoom([FromBody] CreateRoomDto request)
        {
            var result = await _roomService.CreateRoomAsync(request.Username);
            return Ok(result);
        }

        [HttpPost("join/{username}/{room}")]
        public async Task<ActionResult<RoomResponse>> JoinRoom([FromRoute] string username, string room)
        {
            var result = await _roomService.JoinRoomAsync(username, room);
            return Ok(result);
        }

        [HttpPost("leave/{code}")]
        public async Task<ActionResult<string>> LeaveRoom([FromRoute] string code, [FromBody] LeaveRoomDto request)
        {
            var username = await _roomService.LeaveRoomAsync(request.UserId, code);
            return Ok($"{username} ha salido de la sala.");
        }

        [HttpGet("active")]
        public async Task<ActionResult<List<RoomResponse>>> GetActiveRooms()
        {
            var rooms = await _roomService.GetActiveRoomsAsync();
            return Ok(rooms);
        }

        [HttpGet("by-code/{roomCode}")]
        public async Task<IActionResult> GetRoomByCode(string roomCode)
        {
            var result = await _roomService.GetRoomByCodeAsync(roomCode);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(result.Data);
        }

        [HttpGet("{roomCode}/leaderboard")]
        public async Task<ActionResult<List<UserRankDto>>> GetLeaderboard(string roomCode)
        {
            var leaderboard = await _roomService.GetLeaderboardAsync(roomCode);
            return Ok(leaderboard);
        }

    }
}
