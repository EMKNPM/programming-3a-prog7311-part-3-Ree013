using Xunit;
using TechMovePrototype.Models;
using TechMovePrototype.Services;

namespace TechMovePrototype.Tests;

public class ValidationServiceTests
{
    private readonly IValidationService _service = new ValidationService();

    [Fact]
    public void ValidateContract_ValidData_ReturnsTrue()
    {
        var contract = new Contract
        {
            ClientId = 1,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(30)
        };

        var result = _service.ValidateContract(contract, out var errors);

        Assert.True(result);
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateContract_MissingClient_ReturnsFalse()
    {
        var contract = new Contract { ClientId = null, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30) };

        var result = _service.ValidateContract(contract, out var errors);

        Assert.False(result);
        Assert.Contains("Client is required.", errors);
    }

    [Fact]
    public void ValidateServiceRequest_ZeroUSD_ReturnsFalse()
    {
        var request = new ServiceRequest { ContractId = 1, Description = "Test", USDValue = 0 };

        var result = _service.ValidateServiceRequest(request, out var errors);

        Assert.False(result);
        Assert.Contains("USD Amount must be greater than 0", errors);
    }

    [Fact]
    public void ValidateServiceRequest_NullDescription_ReturnsFalse()
    {
        var request = new ServiceRequest { ContractId = 1, Description = null!, USDValue = 100 };

        var result = _service.ValidateServiceRequest(request, out var errors);

        Assert.False(result);
    }
}