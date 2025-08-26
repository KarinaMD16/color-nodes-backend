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

        [HttpPost("join")]
        public async Task<ActionResult<RoomResponse>> JoinRoom([FromBody] JoinRoomDto request)
        {
            var result = await _roomService.JoinRoomAsync(request.Username, request.RoomCode);
            return Ok(result);
        }

        [HttpPost("leave")]
        public async Task<ActionResult<string>> LeaveRoom([FromBody] LeaveRoomDto request)
        {
            var username = await _roomService.LeaveRoomAsync(request.UserId);
            return Ok($"{username} ha salido de la sala.");
        }

        [HttpGet("active")]
        public async Task<ActionResult<List<RoomResponse>>> GetActiveRooms()
        {
            var rooms = await _roomService.GetActiveRoomsAsync();
            return Ok(rooms);
        }

        [HttpGet("{code}")]
        public async Task<ActionResult<RoomResponse>> GetRoomByCode(string code)
        {
            var room = await _roomService.GetRoomByCodeAsync(code);
            if (room == null) return NotFound();
            return Ok(room);
        }
    }
}
