using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExternalApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public ExternalApiController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("POS_Api/Invoke")]
        public async Task<IActionResult> Invoke([FromBody] LoginRequest request)
        {
            if (request.API_Action != "GetLoginData")
            {
                return BadRequest(new { message = "Invalid API Action" });
            }

            // Validate credentials
            var user = await _context.Users.FirstOrDefaultAsync(u => 
                u.Username == request.API_Body.Username && 
                u.Password == request.API_Body.Pw);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            // Get user locations
            var userLocations = await _context.UserLocations
                .Where(ul => ul.UserId == user.Id)
                .Select(ul => new { ul.Location_Code, ul.Location_Name })
                .ToListAsync();

            // Generate JWT token
            var token = GenerateJwtToken(user);

            return Ok(new
            {
                success = true,
                token = token,
                user = new { user.Id, user.Username },
                User_Locations = userLocations
            });
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Username)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginRequest
    {
        public string API_Action { get; set; }
        public string Device_Id { get; set; }
        public string Sync_Time { get; set; }
        public string Company_Code { get; set; }
        public LoginBody API_Body { get; set; }
    }

    public class LoginBody
    {
        public string Username { get; set; }
        public string Pw { get; set; }
    }
}