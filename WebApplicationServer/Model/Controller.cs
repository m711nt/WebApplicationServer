using System.ComponentModel.DataAnnotations;

namespace WebApplicationMatrix.Model;

public class DeviceController
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(15)]
    public string IpAddress { get; set; }

    public string Name { get; set; }

    public bool IsActive { get; set; }
}