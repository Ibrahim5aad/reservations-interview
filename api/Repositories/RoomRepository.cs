using System.Data;
using Dapper;
using Models;
using Models.Errors;

namespace Repositories
{
    public class RoomRepository
    {
        private IDbConnection _db { get; set; }

        public RoomRepository(IDbConnection db)
        {
            _db = db;
        }

        /// <summary>
        /// Find a room by its room number, throwing if not found
        /// </summary>
        /// <exception cref="NotFoundException"></exception>
        public async Task<Room> GetRoom(string roomNumber)
        {
            Room.ValidateRoomNumber(roomNumber);

            var room = await _db.QueryFirstOrDefaultAsync<Room>(
                "SELECT * FROM Rooms WHERE Number = @roomNumber;",
                new { roomNumber }
            );

            if (room == null)
            {
                throw new NotFoundException(nameof(Room), roomNumber);
            }

            return room;
        }

        public async Task<IEnumerable<Room>> GetRooms()
        {
            var rooms = await _db.QueryAsync<Room>("SELECT * FROM Rooms");

            if (rooms == null)
            {
                return [];
            }

            return rooms;
        }

        public async Task<Room> CreateRoom(Room newRoom)
        {
            Room.ValidateRoomNumber(newRoom.Number);

            var createdRoom = await _db.QuerySingleAsync<Room>(
                "INSERT INTO Rooms(Number, State) Values(@Number, @State) RETURNING *",
                newRoom
            );

            return createdRoom;
        }

        public async Task<bool> DeleteRoom(string roomNumber)
        {
            Room.ValidateRoomNumber(roomNumber);

            var deleted = await _db.ExecuteAsync(
                "DELETE FROM Rooms WHERE Number = @roomNumber;",
                new { roomNumber }
            );

            return deleted > 0;
        }
    }
}
