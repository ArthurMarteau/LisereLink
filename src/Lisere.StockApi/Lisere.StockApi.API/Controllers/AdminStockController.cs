using Lisere.StockApi.Application.DTOs;
using Lisere.StockApi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Lisere.StockApi.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
[EnableRateLimiting("fixed")]
public class AdminStockController : ControllerBase
{
    private readonly IStockService _stockService;

    public AdminStockController(IStockService stockService)
    {
        _stockService = stockService;
    }

    /// <summary>
    /// PUT /api/admin/stock — Met à jour la quantité d'un article/taille/magasin.
    /// JWT + rôle Admin requis.
    /// </summary>
    [HttpPut("stock")]
    public async Task<IActionResult> UpdateStock(
        [FromBody] UpdateStockDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        await _stockService.UpdateStockAsync(dto, cancellationToken);
        return NoContent();
    }
}
