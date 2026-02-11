using System.ComponentModel.DataAnnotations;

public class PurchaseBill
{
    [Key]
    public int Id { get; set; }
    
    public int UserId { get; set; }
    
    [Required]
    public string BatchLocation { get; set; }
    
    public DateTime CreatedDate { get; set; }
    
    public int TotalItems { get; set; }
    
    public int TotalQuantity { get; set; }
    
    public decimal TotalCost { get; set; }
    
    public decimal TotalSelling { get; set; }
    
    public List<PurchaseBillItem> Items { get; set; }
}

public class PurchaseBillItem
{
    [Key]
    public int Id { get; set; }
    
    public int PurchaseBillId { get; set; }
    
    [Required]
    public string Item { get; set; }
    
    public decimal StandardCost { get; set; }
    
    public decimal StandardPrice { get; set; }
    
    public int Quantity { get; set; }
    
    public decimal Discount { get; set; }
    
    public decimal TotalCost { get; set; }
    
    public decimal TotalSelling { get; set; }
}