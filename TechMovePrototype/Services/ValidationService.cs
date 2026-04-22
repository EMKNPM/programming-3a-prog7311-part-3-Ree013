using TechMovePrototype.Decorators;
using TechMovePrototype.Models;

namespace TechMovePrototype.Services;

public class ValidationService : IValidationService
{
    private readonly IContractValidator _contractValidator;

    public ValidationService()
    {
       
        _contractValidator = new ComplianceContractDecorator(new BasicContractValidator());
    }

    public bool ValidateContract(Contract contract, out List<string> errors)
    {
        return _contractValidator.Validate(contract, out errors);
    }

    public bool ValidateServiceRequest(ServiceRequest request, out List<string> errors)
    {
        errors = new List<string>();

        if (request.ContractId <= 0)
            errors.Add("The Contract field is required.");

        if (request.USDValue <= 0)
            errors.Add("USD Amount must be greater than 0");

        if (string.IsNullOrWhiteSpace(request.Description))
            errors.Add("Description is required.");

        return errors.Count == 0;
    }
}