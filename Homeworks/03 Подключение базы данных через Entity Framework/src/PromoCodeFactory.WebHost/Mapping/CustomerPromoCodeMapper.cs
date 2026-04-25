using PromoCodeFactory.Core.Domain.PromoCodeManagement;

namespace PromoCodeFactory.WebHost.Mapping;

public static class CustomerPromoCodeMapper
{
    public static IReadOnlyList<CustomerPromoCode> ToListCustomerPromoCode(Guid promoCodeId, IReadOnlyList<Guid> customerIds,
        DateTimeOffset createdAt)
    {
        return customerIds.Select(customerId => new CustomerPromoCode
        {
            CustomerId = customerId,
            PromoCodeId = promoCodeId,
            CreatedAt = createdAt
        }).ToList();
    }
}
