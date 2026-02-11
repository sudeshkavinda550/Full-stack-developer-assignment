using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

[Route("api/[controller]")]
[ApiController]
public class ExternalApiController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ExternalApiController> _logger;

    public ExternalApiController(AppDbContext context, IConfiguration configuration, ILogger<ExternalApiController> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("POS_Api/Invoke")]
    public async Task<IActionResult> Invoke([FromBody] PosApiRequest request)
    {
        try
        {
            _logger.LogInformation($"Received request - API_Action: {request?.API_Action}");

            if (request == null || string.IsNullOrEmpty(request.API_Action))
            {
                return BadRequest(new { success = false, message = "Invalid request format" });
            }

            switch (request.API_Action)
            {
                case "GetLoginData":
                    return await HandleLogin(request);

                case "GetLocations":
                    return await HandleGetLocations(request);

                case "SavePurchaseBill":
                    return await HandleSavePurchaseBill(request);

                default:
                    return BadRequest(new { success = false, message = $"Unknown API_Action: {request.API_Action}" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in Invoke: {ex.Message}");
            _logger.LogError($"Stack trace: {ex.StackTrace}");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    private async Task<IActionResult> HandleLogin(PosApiRequest request)
    {
        try
        {
            var jsonElement = (JsonElement)request.API_Body;
            var username = jsonElement.GetProperty("Username").GetString();
            var password = jsonElement.GetProperty("Pw").GetString();

            _logger.LogInformation($"Attempting login for: {username}");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.Password == password);

            if (user == null)
            {
                _logger.LogWarning($"Login failed for: {username}");
                return Ok(new
                {
                    success = false,
                    message = "Invalid username or password",
                    data = (object)null
                });
            }

            _logger.LogInformation($"Login successful for: {username}");

            // Get user locations - UserLocation already has Location_Code and Location_Name
            var userLocations = await _context.UserLocations
                .Where(ul => ul.UserId == user.Id)
                .Select(ul => new
                {
                    Location_Code = ul.Location_Code,
                    Location_Name = ul.Location_Name
                })
                .ToListAsync();

            // If no locations found, return all available locations
            if (!userLocations.Any())
            {
                userLocations = await _context.LocationDetails
                    .Select(ld => new
                    {
                        Location_Code = ld.Location_Code,
                        Location_Name = ld.Location_Name
                    }).ToListAsync();
            }

            // Generate a simple token
            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            return Ok(new
            {
                success = true,
                message = "Login successful",
                token = token,
                user = new
                {
                    Id = user.Id,
                    Username = user.Username
                },
                User_Locations = userLocations,
                data = new
                {
                    userId = user.Id,
                    username = user.Username,
                    locations = userLocations
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in HandleLogin: {ex.Message}");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    private async Task<IActionResult> HandleGetLocations(PosApiRequest request)
    {
        var locations = await _context.LocationDetails
            .Select(l => new
            {
                Location_Code = l.Location_Code,
                Location_Name = l.Location_Name
            })
            .ToListAsync();

        return Ok(new
        {
            success = true,
            data = locations
        });
    }

    private async Task<IActionResult> HandleSavePurchaseBill(PosApiRequest request)
    {
        var jsonElement = (JsonElement)request.API_Body;
        var billData = JsonSerializer.Deserialize<PurchaseBillData>(jsonElement.GetRawText());

        // Calculate totals
        var totalItems = billData.Items.Count;
        var totalQuantity = billData.Items.Sum(i => i.Quantity);
        var totalCost = billData.Items.Sum(i => i.TotalCost);
        var totalSelling = billData.Items.Sum(i => i.TotalSelling);

        // Create purchase bill
        var purchaseBill = new PurchaseBill
        {
            UserId = billData.UserId,
            BatchLocation = billData.BatchLocation,
            CreatedDate = DateTime.Now,
            TotalItems = totalItems,
            TotalQuantity = totalQuantity,
            TotalCost = totalCost,
            TotalSelling = totalSelling,
            Items = new List<PurchaseBillItem>()
        };

        _context.PurchaseBills.Add(purchaseBill);
        await _context.SaveChangesAsync();

        // Create purchase bill items
        foreach (var item in billData.Items)
        {
            var billItem = new PurchaseBillItem
            {
                PurchaseBillId = purchaseBill.Id,
                Item = item.Item,
                StandardCost = item.StandardCost,
                StandardPrice = item.StandardPrice,
                Quantity = item.Quantity,
                Discount = item.Discount,
                TotalCost = item.TotalCost,
                TotalSelling = item.TotalSelling
            };

            _context.PurchaseBillItems.Add(billItem);
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Purchase bill saved successfully",
            data = new
            {
                billId = purchaseBill.Id,
                totalItems = purchaseBill.TotalItems,
                totalCost = purchaseBill.TotalCost,
                totalSelling = purchaseBill.TotalSelling
            }
        });
    }
}

// Request Models
public class PosApiRequest
{
    public string API_Action { get; set; }
    public string Device_Id { get; set; }
    public string Sync_Time { get; set; }
    public string Company_Code { get; set; }
    public object API_Body { get; set; }
}

public class PurchaseBillData
{
    public int UserId { get; set; }
    public string BatchLocation { get; set; }
    public List<PurchaseBillItemData> Items { get; set; }
}

public class PurchaseBillItemData
{
    public string Item { get; set; }
    public decimal StandardCost { get; set; }
    public decimal StandardPrice { get; set; }
    public int Quantity { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalSelling { get; set; }
}