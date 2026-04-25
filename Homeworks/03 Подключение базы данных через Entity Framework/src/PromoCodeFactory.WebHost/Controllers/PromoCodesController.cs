using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Mapping;
using PromoCodeFactory.WebHost.Models.PromoCodes;
using System.Linq.Expressions;

namespace PromoCodeFactory.WebHost.Controllers;

/// <summary>
/// Промокоды
/// </summary>
public class PromoCodesController(IRepository<PromoCode> promoCodeRepository,
    IRepository<Preference> preferenceRepository, IRepository<Employee> employeeRepository,
    IRepository<CustomerPromoCode> customerPromoCodeRepo, IUnitOfWork unitOfWork) : BaseController
{
    /// <summary>
    /// Получить все промокоды
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PromoCodeShortResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PromoCodeShortResponse>>> Get(CancellationToken ct)
    {
        var promoCodes = await promoCodeRepository.GetAll(withIncludes: true, ct);

        var responses = promoCodes.Select(PromoCodesMapper.ToPromoCodeShortResponse).ToList();

        return Ok(responses);
    }

    /// <summary>
    /// Получить промокод по id
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PromoCodeShortResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PromoCodeShortResponse>> GetById(Guid id, CancellationToken ct)
    {
        var promoCode = await promoCodeRepository.GetById(id, withIncludes: true, ct);

        if (promoCode is null)
            return NotFound(new ProblemDetails
            {
                Title = "GetById PromoCode request canceled",
                Detail = $"PromoCode with Id {id} not found."
            });

        return Ok(PromoCodesMapper.ToPromoCodeShortResponse(promoCode));
    }

    /// <summary>
    /// Создать промокод и выдать его клиентам с указанным предпочтением
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PromoCodeShortResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PromoCodeShortResponse>> Create(PromoCodeCreateRequest request, CancellationToken ct)
    {
        var preference = await preferenceRepository.GetById(request.PreferenceId, withIncludes: true, ct);

        if (preference is null)
            return BadRequest(new ProblemDetails
            {
                Title = "Create PromoCode request canceled",
                Detail = $"No Preference with Id {request.PreferenceId}."
            });

        var customerIdsWithPreference = preference.Customers.Select(c => c.Id).ToList();

        if (customerIdsWithPreference.Count == 0)
            return NotFound(new ProblemDetails
            {
                Title = "Create PromoCode request canceled",
                Detail = $"Customers with Preference Id {request.PreferenceId} not found."
            });

        var partnerManager = await employeeRepository.GetById(request.PartnerManagerId, withIncludes: true, ct);

        if (partnerManager is null)
            return NotFound(new ProblemDetails
            {
                Title = "Create PromoCode request canceled",
                Detail = $"PartnerManager with Id {request.PartnerManagerId} not found."
            });

        var promoNewCode = PromoCodesMapper.ToPromoCode(request, partnerManager, preference);

        promoCodeRepository.Add(promoNewCode);
        //await unitOfWork.SaveChangesAsync(ct);

        var entities = CustomerPromoCodeMapper.ToListCustomerPromoCode(promoNewCode.Id, customerIdsWithPreference,
            DateTimeOffset.Now);

        customerPromoCodeRepo.AddRange(entities);

        await unitOfWork.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = promoNewCode.Id }, PromoCodesMapper.ToPromoCodeShortResponse(promoNewCode));
    }

    /// <summary>
    /// Зарегистрировать использование промокода клиентом
    /// </summary>
    [HttpPost("{id:guid}/usages")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegisterUsage(
        [FromRoute] Guid id,
        [FromBody] PromoCodeApplyRequest request,
        CancellationToken ct)
    {
        Expression<Func<CustomerPromoCode, bool>> predicate = (rec) =>
            rec.PromoCodeId == id && rec.CustomerId == request.CustomerId;

        var response = await customerPromoCodeRepo.GetWhere(predicate, ct: ct);

        if (response.Count == 0)
        {
            return NotFound(new ProblemDetails
            {
                Title = "RegisterUsage request failed.",
                Detail = "Usage for PromoCode [{id}] and Customer [{request.CustomerId}] not found."
            });
        }

        if (response.Count > 1)
            return BadRequest(new ProblemDetails
            {
                Title = "RegisterUsage request failed.",
                Detail = $"Multiple usages found for PromoCode [{id}] and Customer [{request.CustomerId}]."
            });

        response.ElementAt(0).AppliedAt = DateTimeOffset.Now;

        await unitOfWork.SaveChangesAsync(ct);

        return NoContent();
    }
}
