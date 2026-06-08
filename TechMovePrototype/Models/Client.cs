using System.ComponentModel.DataAnnotations;

namespace TechMovePrototype.Models;

public class Client
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string ContactEmail { get; set; } = string.Empty;

    [Required]
    public string ContactPhone { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Region { get; set; } = string.Empty;

    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}