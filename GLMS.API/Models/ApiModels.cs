using System.ComponentModel.DataAnnotations;

namespace GLMS.API.Models;

public enum ContractStatus
{
    Draft,
    Active,
    Expired,
    OnHold
}

public enum ServiceRequestStatus
{
    Pending,
    InProgress,
    Approved,
    Rejected
}

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

public class Contract
{
    public int Id { get; set; }
    public int? ClientId { get; set; }
    public Client? Client { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    public ContractStatus Status { get; set; } = ContractStatus.Draft;

    [Required]
    [StringLength(50)]
    public string ServiceLevel { get; set; } = string.Empty;

    public string? SignedAgreementFilePath { get; set; }

    public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
}

public class ServiceRequest
{
    public int Id { get; set; }

    [Required]
    public int ContractId { get; set; }
    public Contract? Contract { get; set; }

    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public decimal USDValue { get; set; }

    [Required]
    public decimal ZARCost { get; set; }

    public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Pending;
}

public class CreateContractDto
{
    [Required]
    public int ClientId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    public ContractStatus Status { get; set; }

    [Required]
    [StringLength(50)]
    public string ServiceLevel { get; set; } = string.Empty;

    
    public string? SignedAgreementFilePath { get; set; }
}
public class UpdateContractStatusDto
{
    [Required]
    public ContractStatus Status { get; set; }
}

public class CreateServiceRequestDto
{
    [Required]
    public int ContractId { get; set; }

    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal USDValue { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal ZARCost { get; set; }
}

public class UpdateServiceRequestDto
{
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

public class LoginDto
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}