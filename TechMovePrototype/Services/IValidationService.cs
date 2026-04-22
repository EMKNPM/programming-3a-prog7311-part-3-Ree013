using TechMovePrototype.Models;

namespace TechMovePrototype.Services;

public interface IValidationService
{
    bool ValidateContract(Contract contract, out List<string> errors);
    bool ValidateServiceRequest(ServiceRequest request, out List<string> errors);
}