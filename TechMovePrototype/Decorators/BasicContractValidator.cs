using TechMovePrototype.Models;

namespace TechMovePrototype.Decorators;

public class BasicContractValidator : IContractValidator
{
    public bool Validate(Contract contract, out List<string> errors)
    {
        errors = new List<string>();

        if (contract.ClientId == null || contract.ClientId <= 0)
            errors.Add("Client is required.");

        if (contract.StartDate >= contract.EndDate)
            errors.Add("End date must be after start date.");

        return errors.Count == 0;
    }
}