using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PurchaseBillController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PurchaseBillController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<PurchaseBill>> CreatePurchaseBill([FromBody] PurchaseBill purchaseBill)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();
            }

            purchaseBill.UserId = int.Parse(userIdClaim);
            purchaseBill.CreatedDate = DateTime.Now;

            // Save to User_Locations table
            var userLocation = new UserLocation
            {
                UserId = purchaseBill.UserId,
                Location_Code = purchaseBill.BatchLocation.Split('-')[0].Trim(),
                Location_Name = purchaseBill.BatchLocation.Split('-')[1].Trim()
            };

            var existingLocation = await _context.UserLocations
                .FirstOrDefaultAsync(ul => ul.UserId == userLocation.UserId && 
                                          ul.Location_Code == userLocation.Location_Code);

            if (existingLocation == null)
            {
                _context.UserLocations.Add(userLocation);
            }

            _context.PurchaseBills.Add(purchaseBill);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPurchaseBill), new { id = purchaseBill.Id }, purchaseBill);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PurchaseBill>> GetPurchaseBill(int id)
        {
            var purchaseBill = await _context.PurchaseBills
                .Include(pb => pb.Items)
                .FirstOrDefaultAsync(pb => pb.Id == id);

            if (purchaseBill == null)
            {
                return NotFound();
            }

            return purchaseBill;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PurchaseBill>>> GetPurchaseBills()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim);
            return await _context.PurchaseBills
                .Include(pb => pb.Items)
                .Where(pb => pb.UserId == userId)
                .ToListAsync();
        }
    }
}