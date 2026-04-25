using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Models.PromoCodes;
using Soenneker.Utils.AutoBogus;

namespace PromoCodeFactory.UnitTests.WebHost.Controllers.Partners.Shared;

internal static class PreferenceCreater
{
    internal static Preference CreateWithoutCustomers(Guid preferenceId)
    {
        var obj = new AutoFaker<Preference>()
            .RuleFor(r => r.Id, _ => preferenceId)
            .RuleFor(r => r.Customers, _ => [])
            .Generate();

        return obj;
    }
}
