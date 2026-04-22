using Xunit;
using TechMovePrototype.Builders;
using TechMovePrototype.Models.Enums;

namespace TechMovePrototype.Tests;

public class ContractBuilderTests
{
    [Fact]
    public void Builder_BuildsCompleteContract()
    {
        var contract = new ContractBuilder()
            .WithClient(10)
            .WithDates(DateTime.Now, DateTime.Now.AddDays(60))
            .WithStatus(ContractStatus.Active)
            .WithServiceLevel("Premium")
            .Build();

        Assert.Equal(10, contract.ClientId);
        Assert.Equal(ContractStatus.Active, contract.Status);
        Assert.Equal("Premium", contract.ServiceLevel);
        Assert.NotEqual(default, contract.StartDate);
        Assert.NotEqual(default, contract.EndDate);
    }
}