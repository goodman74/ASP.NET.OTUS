using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Models.Customers;
using PromoCodeFactory.WebHost.Models.PromoCodes;

namespace PromoCodeFactory.WebHost.Mapping;

public static class CustomerMapper
{
    public static CustomerResponse ToCustomerResponse(Customer customer, Dictionary<Guid, PromoCode> promoCodesById)
    {
        var preferences = customer.Preferences
            .Select(pref => PreferencesMapper.ToPreferenceShortResponse(pref))
            .ToList();

        var promoCodes = customer.CustomerPromoCodes
            .Select(custPrCode => ToCustomerPromoCodeResponse(custPrCode, promoCodesById[custPrCode.PromoCodeId]))
            .ToList();

        return new CustomerResponse(
            customer.Id,
            customer.FirstName,
            customer.LastName,
            customer.Email,
            preferences,
            promoCodes);
    }

    public static CustomerShortResponse ToCustomerShortResponse(Customer customer)
    {
        var preferences = customer.Preferences
                .Select(pref => PreferencesMapper.ToPreferenceShortResponse(pref))
                .ToList();

        return new CustomerShortResponse(
            customer.Id,
            customer.FirstName,
            customer.LastName,
            customer.Email,
            preferences);
    }

    public static CustomerPromoCodeResponse ToCustomerPromoCodeResponse(CustomerPromoCode custPrCode, PromoCode prCode)
    {
        return new CustomerPromoCodeResponse(
            prCode.Id,
            prCode.Code,
            prCode.ServiceInfo,
            prCode.PartnerName,
            prCode.BeginDate,
            prCode.EndDate,
            prCode.PartnerManager.Id,
            prCode.Preference.Id,
            custPrCode.CreatedAt,
            custPrCode.AppliedAt);
    }

    public static Customer ToCustomer(CustomerCreateRequest request, ICollection<Preference> preferences)
    {
        return new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Preferences = preferences
        };
    }
}
