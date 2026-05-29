using CatalogService.Domain.Common.Exceptions;
using CatalogService.Domain.Errors;

namespace CatalogService.Domain.Common;

public sealed record Price
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Price(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Price Create(decimal amount, string currency)
    {
        if (amount < 0) throw new CatalogDomainException(DomainErrors.InvalidPrice);
        if (string.IsNullOrWhiteSpace(currency)) throw new CatalogDomainException(DomainErrors.InvalidPrice);

        return new Price(amount, currency.Trim().ToUpperInvariant());
    }
}
