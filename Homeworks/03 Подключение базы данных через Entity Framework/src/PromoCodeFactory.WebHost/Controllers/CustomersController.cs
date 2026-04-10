using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Mapping;
using PromoCodeFactory.WebHost.Models.Customers;

namespace PromoCodeFactory.WebHost.Controllers;

/// <summary>
/// Клиенты
/// </summary>
public class CustomersController(IRepository<Customer> customerRepository, IUnitOfWork unitOfWork,
    IRepository<PromoCode> promoCodeRepository, IRepository<Preference> preferenceRepository) : BaseController
{
    /// <summary>
    /// Получить данные всех клиентов
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CustomerShortResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CustomerShortResponse>>> Get(CancellationToken ct)
    {
        var customers = await customerRepository.GetAll(withIncludes: true, ct);

        var responses = customers.Select(CustomerMapper.ToCustomerShortResponse).ToList();

        return Ok(responses);
    }

    /// <summary>
    /// Получить данные клиента по Id
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerResponse>> GetById(Guid id, CancellationToken ct)
    {
        var customer = await customerRepository.GetById(id, withIncludes: true, ct);

        if (customer is null)
            return NotFound(new ProblemDetails
            {
                Title = "GetById Customer request canceled",
                Detail = $"Customer with Id {id} not found."
            });

        var promoCodeIds = customer.CustomerPromoCodes
            .Select(x => x.PromoCodeId)
            .ToList();

        var promoCodes = await promoCodeRepository.GetByRangeId(promoCodeIds, withIncludes: true, ct);

        var promoCodesById = promoCodes.ToDictionary(x => x.Id);

        return Ok(CustomerMapper.ToCustomerResponse(customer, promoCodesById));
    }

    /// <summary>
    /// Создать клиента
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CustomerShortResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CustomerShortResponse>> Create([FromBody] CustomerCreateRequest request, CancellationToken ct)
    {
        var preferenceDistinctIds = request.PreferenceIds.Distinct().ToList();

        var preferences = (ICollection<Preference>)await preferenceRepository.GetByRangeId(preferenceDistinctIds, ct: ct);

        if (preferences.Count != preferenceDistinctIds.Count)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Create Customer request canceled",
                Detail = "One or more preference IDs are invalid."
            });
        }

        var customer = CustomerMapper.ToCustomer(request, preferences);
        customerRepository.Add(customer);
        await unitOfWork.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, CustomerMapper.ToCustomerShortResponse(customer));
    }

    /// <summary>
    /// Обновить клиента
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CustomerShortResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerShortResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] CustomerUpdateRequest request,
        CancellationToken ct)
    {
        var customer = await customerRepository.GetById(id, withIncludes: true, ct: ct);
        if (customer is null)
            return NotFound(new ProblemDetails
            {
                Title = "Update Customer request canceled",
                Detail = $"Employee with Id {id} not found."
            });

        var preferenceDistinctIds = request.PreferenceIds.Distinct().ToList();

        var preferences = (ICollection<Preference>)await preferenceRepository.GetByRangeId(preferenceDistinctIds, ct: ct);

        if (preferences.Count != preferenceDistinctIds.Count)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Update Customer request canceled",
                Detail = "One or more preference IDs are invalid."
            });
        }

        customer.FirstName = request.FirstName;
        customer.LastName = request.LastName;
        customer.Email = request.Email;

        // EF expects Add / Remove on navigation collections to track relationship changes correctly.
        customer.Preferences.Clear();
        foreach (var p in preferences)
            customer.Preferences.Add(p);

        await unitOfWork.SaveChangesAsync(ct);

        return Ok(CustomerMapper.ToCustomerShortResponse(customer));
    }

    /// <summary>
    /// Удалить клиента
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await customerRepository.Delete(id, ct);
        }
        catch (EntityNotFoundException)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Delete Customer request canceled",
                Detail = $"Customer with Id {id} not found."
            });
        }

        return NoContent();
    }
}
