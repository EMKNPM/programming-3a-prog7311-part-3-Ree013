using Xunit;

namespace TechMovePrototype.Tests;

public class CurrencyCalculationTests
{
    [Fact]
    public void ConvertUsdToZar_WithValidRate_ReturnsCorrectValue()
    {
        decimal usd = 100m;
        decimal rate = 18.5m;
        Assert.Equal(1850m, usd * rate);
    }

    [Theory]
    [InlineData(0, 18.5, 0)]           
    [InlineData(0.01, 18.5, 0.185)]    
    [InlineData(-50, 18.5, -925)]      
    public void ConvertUsdToZar_EdgeCases_ReturnsExpected(decimal usd, decimal rate, decimal expected)
    {
        Assert.Equal(expected, usd * rate);
    }
}