using Lisere.StockApi.Application.Common;
using Lisere.StockApi.Application.DTOs;
using Lisere.StockApi.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lisere.StockApi.API.Controllers;

[ApiController]
[Route("api/articles")]
public class ArticlesController : ControllerBase
{
    private readonly IStockService _stockService;

    public ArticlesController(IStockService stockService)
    {
        _stockService = stockService;
    }

    /// <summary>GET /api/articles?page=1&amp;pageSize=50 — Liste paginée des articles.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ArticleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _stockService.GetArticlesAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }

    /// <summary>GET /api/articles/{barcode} — Article par code-barres EAN-13.</summary>
    [HttpGet("{barcode}")]
    [ProducesResponseType(typeof(ArticleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
