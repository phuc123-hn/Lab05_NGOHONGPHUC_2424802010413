namespace PricingService.Domain;

/// <summary>Luật tăng/giảm giá theo phần trăm khi điều kiện thỏa.</summary>
public class DiscountMarkupRules
{
    private readonly List<PercentMarkup> rules = new();

    public void AddPercentMarkup(string condition, decimal percent)
    {
        rules.Add(new PercentMarkup(condition, percent));
    }

    public decimal Apply(decimal price, IDictionary<string, object> subject)
    {
        foreach (var rule in rules)
        {
            if (ExpressionEvaluator.EvaluateBool(rule.Condition, subject))
                price += price * rule.Percent / 100M;
        }

        return price;
    }

    private sealed record PercentMarkup(string Condition, decimal Percent);
}
