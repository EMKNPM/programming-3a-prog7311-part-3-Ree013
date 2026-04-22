using TechMovePrototype.Models;
using TechMovePrototype.Models.Enums;

namespace TechMovePrototype.Observers;

public class ServiceRequestBlockerObserver : IContractObserver
{
    public void Update(Contract contract)
    {
        if (contract.Status == ContractStatus.Expired || contract.Status == ContractStatus.OnHold)
        {
            Console.WriteLine($"[Observer] Contract {contract.Id} changed to {contract.Status}. Blocking new Service Requests.");
            
        }
    }
}