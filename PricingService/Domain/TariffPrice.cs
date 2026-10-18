namespace PricingService.Domain;

public class CoverPrice
{
    public CoverPrice(string code, decimal price)
    {
        Code = code;
        Price = price;
    }

    public string Code { get; }
    public decimal Price { get; }
}

public class TariffPrice
{
    public TariffPrice(IReadOnlyDictionary<string, CoverPrice> covers)
    {
        Covers = covers;
        TotalPremium = covers.Values.Sum(c => c.Price);
    }

    public decimal TotalPremium { get; }
    public IReadOnlyDictionary<string, CoverPrice> Covers { get; }
}
