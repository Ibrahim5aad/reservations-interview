using Microsoft.AspNetCore.Mvc;
using Models;
using Repositories;

namespace Controllers
{
    [Tags("Guests"), Route("guests")]
    public class GuestsController : Controller
    {
        private GuestRepository _repo;

        public GuestsController(GuestRepository guestRepository)
        {
            _repo = guestRepository;
        }

        [HttpGet, Produces("application/json"), Route("")]
        public async Task<ActionResult<Guest>> GetGuests()
        {
            var guests = await _repo.GetGuests();

            return Json(guests);
        }
    }
}
