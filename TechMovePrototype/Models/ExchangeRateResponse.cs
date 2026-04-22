namespace TechMovePrototype.Models;

public class ExchangeRateResponse
{
    public string? Result { get; set; }
    public Dictionary<string, decimal>? Rates { get; set; }
}