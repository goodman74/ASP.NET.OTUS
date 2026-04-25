using Bogus;
using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using Soenneker.Utils.AutoBogus;

namespace PromoCodeFactory.UnitTests.WebHost.Controllers.Partners.Shared;

internal static class PartnerCreater
{
    internal static Partner CreateWithOneActiveLimit(Guid partnerId, Guid limitId, bool isActive)
    {
        var limits = new List<PartnerPromoCodeLimit>();
        var partner = Create(partnerId, isActive);
        partner.PartnerLimits = limits;

        var limit = new AutoFaker<PartnerPromoCodeLimit>()
            .RuleFor(l => l.Id, _ => limitId)
            .RuleFor(l => l.Partner, partner)
            .RuleFor(l => l.CanceledAt, _ => null)
            .RuleFor(l => l.CreatedAt, _ => DateTimeOffset.UtcNow.AddDays(-1))
            .RuleFor(l => l.EndAt, _ => DateTimeOffset.UtcNow.AddDays(30))
            .RuleFor(l => l.Limit, _ => new Randomizer().Number(100,200))
            .RuleFor(l => l.IssuedCount, _ => 0)
            .Generate();

        limits.Add(limit);
        return partner;
    }

    internal static Partner CreateWithCanceledLimit(Guid partnerId, Guid limitId, bool isActive, DateTimeOffset? canceledAt)
    {
        var partner = CreateWithOneActiveLimit(partnerId, limitId, isActive);

        partner.PartnerLimits.ElementAt(0).CanceledAt = canceledAt;

        return partner;
    }


    internal static Partner Create(Guid partnerId, bool isActive)
    {
        var role = new AutoFaker<Role>()
            .RuleFor(r => r.Id, _ => Guid.NewGuid())
            .Generate();

        var employee = new AutoFaker<Employee>()
            .RuleFor(e => e.Id, _ => Guid.NewGuid())
            .RuleFor(e => e.Role, role)
            .Generate();

        var partner = new AutoFaker<Partner>()
            .RuleFor(p => p.Id, _ => partnerId)
            .RuleFor(p => p.IsActive, _ => isActive)
            .RuleFor(p => p.Manager, employee)
            .RuleFor(p => p.PartnerLimits, [])
            .Generate();

        return partner;
    }

    internal static Partner? CreateWithLimitExceeded(Guid partnerId, Guid limitId)
    {
        var partner = CreateWithOneActiveLimit(partnerId, limitId, true);
        var partnerPromoCodeLimit = partner.PartnerLimits.ElementAt(0);

        var limit = partnerPromoCodeLimit.Limit;
        partnerPromoCodeLimit.IssuedCount = limit;

        return partner;
    }
}
