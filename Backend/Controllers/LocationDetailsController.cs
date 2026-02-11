using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LocationDetailsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LocationDetailsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LocationDetail>>> GetLocationDetails()
        {
            return await _context.LocationDetails.ToListAsync();
        }
    }
}