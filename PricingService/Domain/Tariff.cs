namespace PricingService.Domain;

public class Tariff
{
    public Tariff(string code)
    {
        Code = code;
    }

    public string Code { get; }
    public BasePremiumRules BasePremiumRules { get; } = new();
    public DiscountMarkupRules DiscountMarkupRules { get; } = new();

    public TariffPrice CalculatePrice(Calculation calculation)
    {
        var covers = new Dictionary<string, CoverPrice>();

        foreach (var coverCode in calculation.SelectedCovers)
        {
            var basePrice = BasePremiumRules.Calculate(coverCode, calculation.Subject);
            var finalPrice = DiscountMarkupRules.Apply(basePrice, calculation.Subject);
            covers[coverCode] = new CoverPrice(coverCode, Math.Round(finalPrice, 2, MidpointRounding.AwayFromZero));
        }

        return new TariffPrice(covers);
    }
}
