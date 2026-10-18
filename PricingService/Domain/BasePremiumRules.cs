namespace PricingService.Domain;

/// <summary>Luật tính phí cơ bản cho từng gói bảo hiểm (cover).</summary>
public class BasePremiumRules
{
    private readonly List<BasePriceRule> rules = new();

    public void AddBasePriceRule(string coverCode, string? condition, string priceExpression)
    {
        rules.Add(new BasePriceRule(coverCode, condition, priceExpression));
    }

    public decimal Calculate(string coverCode, IDictionary<string, object> subject)
    {
        var rule = rules.FirstOrDefault(r => r.CoverCode == coverCode && r.Matches(subject));
        if (rule is null)
            throw new InvalidOperationException($"Không có luật giá nào áp dụng cho gói '{coverCode}'.");

        return ExpressionEvaluator.EvaluateDecimal(rule.PriceExpression, subject);
    }

    private sealed record BasePriceRule(string CoverCode, string? Condition, string PriceExpression)
    {
        public bool Matches(IDictionary<string, object> subject) =>
            string.IsNullOrWhiteSpace(Condition) || ExpressionEvaluator.EvaluateBool(Condition, subject);
    }
}
