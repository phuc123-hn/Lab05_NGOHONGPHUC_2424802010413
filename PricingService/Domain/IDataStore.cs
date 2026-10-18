namespace PricingService.Domain;

public interface ITariffRepository
{
    Task<Tariff> this[string productCode] { get; }
}

public interface IDataStore
{
    ITariffRepository Tariffs { get; }
}

public class InMemoryDataStore : IDataStore
{
    public InMemoryDataStore()
    {
        Tariffs = new InMemoryTariffRepository(DefaultTariffs.All());
    }

    public ITariffRepository Tariffs { get; }

    private sealed class InMemoryTariffRepository : ITariffRepository
    {
        private readonly Dictionary<string, Tariff> tariffs;

        public InMemoryTariffRepository(IEnumerable<Tariff> tariffs)
        {
            this.tariffs = tariffs.ToDictionary(t => t.Code);
        }

        public Task<Tariff> this[string productCode] =>
            tariffs.TryGetValue(productCode, out var tariff)
                ? Task.FromResult(tariff)
                : Task.FromException<Tariff>(new KeyNotFoundException($"Không có bảng giá cho sản phẩm '{productCode}'."));
    }
}
