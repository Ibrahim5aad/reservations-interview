using Microsoft.AspNetCore.Mvc;
using Contracts;
using FluentValidation;
using Models;
using Repositories;

namespace Controllers
{
    [ApiController]
    [Tags("Reservations"), Route("reservations")]
    public class ReservationsController : ControllerBase
    {
        private ReservationRepository _repo { get; set; }
        private IValidator<ReservationRequest> _validator { get; set; }

        public ReservationsController(ReservationRepository reservationRepository, IValidator<ReservationRequest> validator)
        {
            _repo = reservationRepository;
            _validator = validator;
        }

        [HttpGet, Produces("application/json"), Route("")]
        [ProducesResponseType(typeof(IEnumerable<Reservation>), StatusCodes.Status200OK)]
        public async Task<ActionResult<Reservation>> GetReservations()
        {
            var reservations = await _repo.GetReservations();

            return Ok(reservations);
        }

        [HttpGet, Produces("application/json"), Route("{reservationId}")]
        [ProducesResponseType(typeof(Reservation), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Reservation>> GetRoom(Guid reservationId)
        {
            var reservation = await _repo.GetReservation(reservationId);
            
            return Ok(reservation);
        }

        /// <summary>
        /// Create a new reservation, to generate the GUID ID on the server, send an Empty GUID (all 0s)
        /// </summary>
        /// <param name="request"> The request/booking data. </param>
        /// <returns> The created reservation. </returns>
        [HttpPost, Produces("application/json"), Route("")]
        [ProducesResponseType(typeof(Reservation), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Reservation>> BookReservation([FromBody] ReservationRequest request)
        {
            await _validator.ValidateAndThrowAsync(request);

            var createdReservation = await _repo.CreateReservation(request);
            return Created($"/reservations/{createdReservation.Id}", createdReservation);
        }

        [HttpDelete, Produces("application/json"), Route("{reservationId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteReservation(Guid reservationId)
        {
            await _repo.DeleteReservation(reservationId);

            return NoContent();
        }
    }
}
