using TechMovePrototype.Models;

namespace TechMovePrototype.Observers;

public interface IContractObserver
{
    void Update(Contract contract);
}