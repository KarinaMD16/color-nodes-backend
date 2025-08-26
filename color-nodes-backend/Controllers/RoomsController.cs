using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class RoomController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    [HttpPost("create/{leaderId}")]
    public async Task<IActionResult> CreateRoom(int leaderId)
    {
        try
        {
            var room = await _roomService.CreateRoomAsync(leaderId);
            return Ok(room);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("join/{code}/{userId}")]
    public async Task<IActionResult> JoinRoom(string code, int userId)
    {
        try
        {
            var room = await _roomService.JoinRoomAsync(code, userId);
            return Ok(room);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("leave/{roomId}/{userId}")]
    public async Task<IActionResult> LeaveRoom(int roomId, int userId)
    {
        try
        {
            await _roomService.LeaveRoomAsync(roomId, userId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetRoomByCode(string code)
    {
        var room = await _roomService.GetRoomByCodeAsync(code);
        if (room == null) return NotFound(new { message = "Sala no encontrada." });
        return Ok(room);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveRooms()
    {
        var rooms = await _roomService.GetActiveRoomsAsync();
        return Ok(rooms);
    }
}
