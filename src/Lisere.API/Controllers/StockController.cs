using Lisere.Application.DTOs;
using Lisere.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Lisere.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("fixed")]
public class StockController : ControllerBase
{
    private readonly IStockService _stockService;

    public StockController(IStockService stockService)
    {
        _stockService = stockService;
    }

    /// <summary>Retourne le stock disponible par taille pour un article dans un magasin donné.</summary>
    [HttpGet("{articleId:guid}")]
    public async Task<ActionResult<IEnumerable<StockDto>>> GetStock(
        Guid articleId,
        [FromQuery] string storeId,
        CancellationToken cancellationToken = default)
    {
        var stock = await _stockService.GetStockForStoreAsync(articleId, storeId, cancellationToken);
        return Ok(stock);
    }
}
