using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Models.PromoCodes;

namespace PromoCodeFactory.WebHost.Mapping;

public static class PromoCodesMapper
{
    public static PromoCodeShortResponse ToPromoCodeShortResponse(PromoCode promoCode)
    {
        return new PromoCodeShortResponse(
            promoCode.Id,
            promoCode.Code,
            promoCode.ServiceInfo,
            promoCode.PartnerName,
            promoCode.BeginDate,
            promoCode.EndDate,
            promoCode.PartnerManager.Id,
            promoCode.Preference.Id);
    }

    public static PromoCode ToPromoCode(PromoCodeCreateRequest createRequest, Employee partnerManager,
        Preference preference)
    {
        return new PromoCode() {
            Id = Guid.NewGuid(),
            Code = createRequest.Code,
            ServiceInfo = createRequest.ServiceInfo,
            PartnerName = createRequest.PartnerName,
            BeginDate = createRequest.BeginDate,
            EndDate = createRequest.EndDate,
            PartnerManager = partnerManager,
            Preference = preference
        };
    }
}
