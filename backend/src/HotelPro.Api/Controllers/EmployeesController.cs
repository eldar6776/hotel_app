using Asp.Versioning;
using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Core.Helpers;
using HotelPro.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelPro.Api.Controllers;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/employees")]
[Authorize(Roles = "Admin,Manager")]
public class EmployeesController : ControllerBase
{
    private readonly HotelProDbContext _dbContext;

    public EmployeesController(HotelProDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<List<EmployeeDto>>> GetEmployees()
    {
        var employees = await _dbContext.Employees
            .IgnoreQueryFilters()
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .Select(x => new EmployeeDto(
                x.Id,
                x.FirstName,
                x.LastName,
                x.Email,
                x.Phone,
                x.Role.ToString(),
                x.IsActive,
                x.CanLogin,
                x.CreatedAt))
            .ToListAsync();

        return Ok(employees);
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> CreateEmployee(CreateEmployeeRequest request)
    {
        if (!Enum.TryParse<EmployeeRole>(request.Role, true, out var role))
            return BadRequest(new { error = "Invalid employee role." });

        if (await _dbContext.Employees.IgnoreQueryFilters().AnyAsync(x => x.Email == request.Email))
            return BadRequest(new { error = "Employee email already exists." });

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Role = role,
            PinHash = PinHelper.HashPin("000000"),
            PasswordHash = string.IsNullOrWhiteSpace(request.Password)
                ? null
                : BCrypt.Net.BCrypt.HashPassword(request.Password),
            IsActive = request.IsActive,
            CanLogin = request.CanLogin,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetEmployees), new { version = "2.0" }, ToDto(employee));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EmployeeDto>> UpdateEmployee(Guid id, UpdateEmployeeRequest request)
    {
        var employee = await _dbContext.Employees.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id);
        if (employee == null) return NotFound();

        if (!Enum.TryParse<EmployeeRole>(request.Role, true, out var role))
            return BadRequest(new { error = "Invalid employee role." });

        var emailExists = await _dbContext.Employees
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id != id && x.Email == request.Email);
        if (emailExists) return BadRequest(new { error = "Employee email already exists." });

        employee.FirstName = request.FirstName;
        employee.LastName = request.LastName;
        employee.Email = request.Email;
        employee.Phone = request.Phone;
        employee.Role = role;
        employee.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Password))
            employee.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        await _dbContext.SaveChangesAsync();
        return Ok(ToDto(employee));
    }

    [HttpPatch("{id:guid}/toggle-active")]
    public async Task<ActionResult<EmployeeDto>> ToggleActive(Guid id, ToggleEmployeeActiveRequest request)
    {
        var employee = await _dbContext.Employees.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id);
        if (employee == null) return NotFound();

        employee.IsActive = request.IsActive;
        employee.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return Ok(ToDto(employee));
    }

    private static EmployeeDto ToDto(Employee employee) => new(
        employee.Id,
        employee.FirstName,
        employee.LastName,
        employee.Email,
        employee.Phone,
        employee.Role.ToString(),
        employee.IsActive,
        employee.CanLogin,
        employee.CreatedAt);
}

public record EmployeeDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string Role,
    bool IsActive,
    bool CanLogin,
    DateTime CreatedAt);

public record CreateEmployeeRequest(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string Role,
    string? Password,
    bool IsActive,
    bool CanLogin);

public record UpdateEmployeeRequest(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string Role,
    string? Password);

public record ToggleEmployeeActiveRequest(bool IsActive);
