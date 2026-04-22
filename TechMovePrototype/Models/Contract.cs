using System.ComponentModel.DataAnnotations;
using TechMovePrototype.Models.Enums;
using TechMovePrototype.Observers;

namespace TechMovePrototype.Models;

public class Contract
{
    public int Id { get; set; }

    public int? ClientId { get; set; }

    public Client? Client { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    [Required]
    public ContractStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            Notify();   // Observer Pattern 
        }
    }

    private ContractStatus _status = ContractStatus.Draft;  

    [Required]
    [StringLength(50)]
    public string ServiceLevel { get; set; } = string.Empty;

    public string? SignedAgreementFilePath { get; set; }

    public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    private readonly List<IContractObserver> _observers = new List<IContractObserver>();

    public void Attach(IContractObserver observer) => _observers.Add(observer);

    public void Detach(IContractObserver observer) => _observers.Remove(observer);

    public void Notify()
    {
        foreach (var observer in _observers)
        {
            observer.Update(this);
        }
    }
}