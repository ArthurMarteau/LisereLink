using Lisere.StockApi.Application.Common;
using Lisere.StockApi.Application.DTOs;
using Lisere.StockApi.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Lisere.StockApi.API.Controllers;

[ApiController]
[Route("api/stock")]
[EnableRateLimiting("fixed")]
public class StockController : ControllerBase
{
    private readonly IStockService _stockService;

    public StockController(IStockService stockService)
    {
        _stockService = stockService;
    }
    
    [HttpGet("{articleId:guid}")]
    public async Task<IActionResult> GetByArticle(
        Guid articleId,
        [FromQuery] string storeId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storeId))
            return BadRequest(new ProblemDetails
            {
                Title = "Paramètre manquant",
                Detail = "Le paramètre storeId est requis.",
                Status = 400
            });

        var stock = await _stockService.GetStockAsync(articleId, storeId, cancellationToken);
        return Ok(stock);
    }
    
    [HttpGet("articles")]
    public async Task<IActionResult> GetByStore(
        [FromQuery] string storeId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storeId))
            return BadRequest(new ProblemDetails
            {
                Title = "Paramètre manquant",
                Detail = "Le paramètre storeId est requis.",
                Status = 400
            });

        var result = await _stockService.GetAllArticlesWithStockAsync(storeId, page, pageSize, cancellationToken);
        return Ok(result);
    }
}
