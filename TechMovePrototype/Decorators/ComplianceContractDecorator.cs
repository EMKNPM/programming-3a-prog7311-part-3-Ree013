using TechMovePrototype.Models;
using TechMovePrototype.Models.Enums;

namespace TechMovePrototype.Decorators;

public class ComplianceContractDecorator : IContractValidator
{
    private readonly IContractValidator _validator;

    public ComplianceContractDecorator(IContractValidator validator)
    {
        _validator = validator;
    }

    public bool Validate(Contract contract, out List<string> errors)
    {
        bool isValid = _validator.Validate(contract, out errors);

        
        if (contract.ServiceLevel == "Premium" && contract.Status != ContractStatus.Active)
        {
            errors.Add("Premium contracts must be in Active status.");
            isValid = false;
        }

        return isValid;
    }
}