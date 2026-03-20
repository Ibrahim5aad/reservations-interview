using Microsoft.AspNetCore.Mvc;
using Models;
using Repositories;

namespace Controllers
{
    [ApiController]
    [Tags("Rooms"), Route("rooms")]
    public class RoomsController : ControllerBase
    {
        private RoomRepository _repo { get; set; }

        public RoomsController(RoomRepository roomRepository)
        {
            _repo = roomRepository;
        }

        [HttpGet, Produces("application/json"), Route("")]
        public async Task<ActionResult<Room>> GetRooms()
        {
            var rooms = await _repo.GetRooms();

            if (rooms == null)
            {
                return Ok(Enumerable.Empty<Room>());
            }

            return Ok(rooms);
        }

        [HttpGet, Produces("application/json"), Route("{roomNumber}")]
        public async Task<ActionResult<Room>> GetRoom(string roomNumber)
        {
            var room = await _repo.GetRoom(roomNumber);

            return Ok(room);
        }

        [HttpPost, Produces("application/json"), Route("")]
        public async Task<ActionResult<Room>> CreateRoom([FromBody] Room newRoom)
        {
            var createdRoom = await _repo.CreateRoom(newRoom);

            if (createdRoom == null)
            {
                return NotFound();
            }

            return Ok(createdRoom);
        }

        [HttpDelete, Produces("application/json"), Route("{roomNumber}")]
        public async Task<IActionResult> DeleteRoom(string roomNumber)
        {
            if (roomNumber.Length != 3)
            {
                return BadRequest("Invalid room ID - format is ###, ex 001 / 002 / 101");
            }

            var deleted = await _repo.DeleteRoom(roomNumber);

            return deleted ? NoContent() : NotFound();
        }
    }
}
