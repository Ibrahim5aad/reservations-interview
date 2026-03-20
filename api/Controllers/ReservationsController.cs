using Microsoft.AspNetCore.Mvc;
using Models;
using Repositories;

namespace Controllers
{
    [Tags("Reservations"), Route("reservations")]
    public class ReservationsController : Controller
    {
        private ReservationRepository _repo { get; set; }

        public ReservationsController(ReservationRepository reservationRepository)
        {
            _repo = reservationRepository;
        }

        [HttpGet, Produces("application/json"), Route("")]
        public async Task<ActionResult<Reservation>> GetReservations()
        {
            var reservations = await _repo.GetReservations();

            return Json(reservations);
        }

        [HttpGet, Produces("application/json"), Route("{reservationId}")]
        public async Task<ActionResult<Reservation>> GetRoom(Guid reservationId)
        {
            var reservation = await _repo.GetReservation(reservationId);
            
            return Json(reservation);
        }

        /// <summary>
        /// Create a new reservation, to generate the GUID ID on the server, send an Empty GUID (all 0s)
        /// </summary>
        /// <param name="newBooking"></param>
        /// <returns></returns>
        [HttpPost, Produces("application/json"), Route("")]
        public async Task<ActionResult<Reservation>> BookReservation(
            [FromBody] Reservation newBooking
        )
        {
            // Provide a real ID if one is not provided
            if (newBooking.Id == Guid.Empty)
            {
                newBooking.Id = Guid.NewGuid();
            }

            var createdReservation = await _repo.CreateReservation(newBooking);
            return Created($"/reservation/${createdReservation.Id}", createdReservation);
        }

        [HttpDelete, Produces("application/json"), Route("{reservationId}")]
        public async Task<IActionResult> DeleteReservation(Guid reservationId)
        {
            var result = await _repo.DeleteReservation(reservationId);

            return result ? NoContent() : NotFound();
        }
    }
}
