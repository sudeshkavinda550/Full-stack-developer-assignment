using System.ComponentModel.DataAnnotations;

public class UserLocation
{
    [Key]
    public int Id { get; set; }
    
    public int UserId { get; set; }
    
    [Required]
    public string Location_Code { get; set; }
    
    [Required]
    public string Location_Name { get; set; }
}