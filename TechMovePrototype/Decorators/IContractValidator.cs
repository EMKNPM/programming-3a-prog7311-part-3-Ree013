using TechMovePrototype.Models;

namespace TechMovePrototype.Decorators;

public interface IContractValidator
{
    bool Validate(Contract contract, out List<string> errors);
}