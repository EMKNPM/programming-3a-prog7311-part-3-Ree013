using TechMovePrototype.Models;
using TechMovePrototype.Models.Enums;

namespace TechMovePrototype.Builders;

public class ContractBuilder
{
    private readonly Contract _contract = new Contract();

    public ContractBuilder WithClient(int clientId)
    {
        _contract.ClientId = clientId;
        return this;
    }

    public ContractBuilder WithDates(DateTime startDate, DateTime endDate)
    {
        _contract.StartDate = startDate;
        _contract.EndDate = endDate;
        return this;
    }

    public ContractBuilder WithStatus(ContractStatus status)
    {
        _contract.Status = status;
        return this;
    }

    public ContractBuilder WithServiceLevel(string serviceLevel)
    {
        _contract.ServiceLevel = serviceLevel;
        return this;
    }

    public Contract Build()
    {
        return _contract;
    }
}