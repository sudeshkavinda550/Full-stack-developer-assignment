using System.ComponentModel.DataAnnotations;

public class LocationDetail
{
    [Key]
    public string Location_Code { get; set; }
    
    [Required]
    public string Location_Name { get; set; }
}