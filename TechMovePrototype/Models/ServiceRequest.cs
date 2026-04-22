using System.ComponentModel.DataAnnotations;
using TechMovePrototype.Models.Enums;

namespace TechMovePrototype.Models;

public class ServiceRequest
{
    public int Id { get; set; }

    [Required]
    public int ContractId { get; set; }
    public Contract Contract { get; set; } = null!;

    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal USDValue { get; set; }   

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal ZARCost { get; set; }   

    public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Pending;
}