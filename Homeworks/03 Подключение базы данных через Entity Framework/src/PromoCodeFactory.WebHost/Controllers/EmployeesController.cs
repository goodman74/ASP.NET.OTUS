using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.DataAccess.Repositories;
using PromoCodeFactory.WebHost.Mapping;
using PromoCodeFactory.WebHost.Models.Employees;

namespace PromoCodeFactory.WebHost.Controllers;

/// <summary>
/// Сотрудники
/// </summary>
public class EmployeesController(IRepository<Employee> employeeRepository, IRepository<Role> roleRepository,
    IUnitOfWork unitOfWork) : BaseController
{
    /// <summary>
    /// Получить данные всех сотрудников
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EmployeeShortResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EmployeeShortResponse>>> Get(CancellationToken ct)
    {
        var employees = await employeeRepository.GetAll(ct: ct);

        var employeesModels = employees.Select(EmployeesMapper.ToEmployeeShortResponse).ToList();

        return Ok(employeesModels);
    }

    /// <summary>
    /// Получить данные сотрудника по Id
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeResponse>> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var employee = await employeeRepository.GetById(id, true, ct);

        if (employee is null)
            return NotFound(new ProblemDetails
            {
                Title = "GetById Employee request canceled",
                Detail = $"Employee with Id {id} not found."
            });

        return Ok(EmployeesMapper.ToEmployeeResponse(employee));
    }

    /// <summary>
    /// Создать сотрудника
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EmployeeResponse>> Create([FromBody] EmployeeCreateRequest request,
        CancellationToken ct)
    {
        var role = await roleRepository.GetById(request.RoleId, ct: ct);

        if (role is null)
            return BadRequest(new ProblemDetails
            {
                Title = "Create Employee request canceled",
                Detail = $"Role with Id {request.RoleId} not found."
            });

        var employee = EmployeesMapper.ToEmployee(request, role);
        employeeRepository.Add(employee);
        await unitOfWork.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
    }

    /// <summary>
    /// Обновить сотрудника
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] EmployeeUpdateRequest request,
        CancellationToken ct)
    {
        var employee = await employeeRepository.GetById(id, ct: ct);
        if (employee is null)
            return NotFound(new ProblemDetails
            {
                Title = "Update Employee request canceled",
                Detail = $"Employee with Id {id} not found."
            });

        var role = await roleRepository.GetById(request.RoleId, ct: ct);
        if (role is null)
            return BadRequest(new ProblemDetails
            {
                Title = "Update Employee request canceled",
                Detail = $"Role with Id {request.RoleId} not found."
            });

        employee.FirstName = request.FirstName;
        employee.LastName = request.LastName;
        employee.Email = request.Email;
        employee.Role = role;

        await unitOfWork.SaveChangesAsync(ct);

        return Ok(EmployeesMapper.ToEmployeeResponse(employee));
    }

    /// <summary>
    /// Удалить сотрудника
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        try
        {
            await employeeRepository.Delete(id, ct);
        }
        catch (EntityNotFoundException)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Delete Employee request canceled",
                Detail = $"Employee with Id {id} not found."
            });
        }

        return NoContent();
    }
}
