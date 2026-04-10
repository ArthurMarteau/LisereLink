using Lisere.StockApi.Application.Common;
using Lisere.StockApi.Application.DTOs;
using Lisere.StockApi.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Lisere.StockApi.API.Controllers;

[ApiController]
[Route("api/articles")]
[EnableRateLimiting("fixed")]
public class ArticlesController : ControllerBase
{
    private readonly IStockService _stockService;

    public ArticlesController(IStockService stockService)
    {
        _stockService = stockService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? query = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _stockService.GetArticlesAsync(page, pageSize, query, cancellationToken);
        return Ok(result);
    }
    
    [HttpGet("{barcode}")]
    public async Task<IActionResult> GetByBarcode(
        string barcode,
        CancellationToken cancellationToken = default)
    {
        var article = await _stockService.GetArticleByBarcodeAsync(barcode, cancellationToken);

        if (article is null)
            return NotFound();

        return Ok(article);
    }
}
