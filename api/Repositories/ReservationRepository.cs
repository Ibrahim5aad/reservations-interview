using System.Data;
using Dapper;
using Models;
using Contracts;
using Models.Errors;

namespace Repositories
{
    public class ReservationRepository
    {
        private IDbConnection _db { get; set; }

        public ReservationRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Reservation>> GetReservations()
        {
            var reservations = await _db.QueryAsync<Reservation>("SELECT * FROM Reservations");

            if (reservations == null)
            {
                return [];
            }

            return reservations;
        }

        /// <summary>
        /// Find a reservation by its Guid ID, throwing if not found
        /// </summary>
        /// <exception cref="NotFoundException"></exception>
        public async Task<Reservation> GetReservation(Guid reservationId)
        {
            var reservation = await _db.QueryFirstOrDefaultAsync<Reservation>(
                "SELECT * FROM Reservations WHERE Id = @reservationId;",
                new { reservationId }
            );

            if (reservation == null)
            {
                throw new NotFoundException(nameof(Reservation), reservationId.ToString());
            }

            return reservation;
        }

        public async Task<Reservation> CreateReservation(ReservationRequest request)
        {

            ArgumentNullException.ThrowIfNull(request);

            var room = await _db.QueryFirstOrDefaultAsync<Room>(
                "SELECT * FROM Rooms WHERE Number = @RoomNumber;",
                new { request.RoomNumber }
            ) ?? throw new NotFoundException(nameof(Room), request.RoomNumber);

            await _db.ExecuteAsync(
                "INSERT OR IGNORE INTO Guests(Email, Name) VALUES(@GuestEmail, @GuestEmail)",
                new { request.GuestEmail }
            );

            var reservation = new Reservation
            {
                Id = Guid.NewGuid(),
                RoomNumber = request.RoomNumber,
                GuestEmail = request.GuestEmail,
                Start = request.Start,
                End = request.End
            };

            var created = await _db.QuerySingleAsync<Reservation>(
                @"INSERT INTO Reservations(Id, GuestEmail, RoomNumber, Start, End)
                  VALUES(@Id, @GuestEmail, @RoomNumber, @Start, @End)
                  RETURNING *",
                reservation
            );

            return created;
        }

        public async Task DeleteReservation(Guid reservationId)
        {
            var deleted = await _db.ExecuteAsync(
                "DELETE FROM Reservations WHERE Id = @reservationId;",
                new { reservationId }
            );

            if (deleted == 0)
            {
                throw new NotFoundException(nameof(Reservation), reservationId.ToString());
            }
        }
    }
}
