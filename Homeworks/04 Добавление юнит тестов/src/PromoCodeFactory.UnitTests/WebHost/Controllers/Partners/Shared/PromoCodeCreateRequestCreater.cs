using PromoCodeFactory.WebHost.Models.PromoCodes;
using Soenneker.Utils.AutoBogus;

namespace PromoCodeFactory.UnitTests.WebHost.Controllers.Partners.Shared;

internal static class PromoCodeCreateRequestCreater
{
    internal static PromoCodeCreateRequest Create(Guid partnerId, Guid preferenceId)
    {
        var request = new AutoFaker<PromoCodeCreateRequest>()
            .RuleFor(r => r.PartnerId, _ => partnerId)
            .RuleFor(r => r.PreferenceId, _ => preferenceId)
            .RuleFor(x => x.BeginDate, f => DateTimeOffset.UtcNow.AddDays(f.Random.Int(1, 30)))
            .RuleFor(x => x.EndDate, (f, x) => x.BeginDate.AddDays(f.Random.Int(1, 30)))
            .Generate();

        return request;
    }
}
