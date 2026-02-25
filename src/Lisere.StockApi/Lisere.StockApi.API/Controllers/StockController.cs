using Lisere.StockApi.Application.Common;
using Lisere.StockApi.Application.DTOs;
using Lisere.StockApi.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lisere.StockApi.API.Controllers;

[ApiController]
[Route("api/stock")]
public class StockController : ControllerBase
{
    private readonly IStockService _stockService;

    public StockController(IStockService stockService)
    {
        _stockService = stockService;
    }

    /// <summary>GET /api/stock/{articleId}?storeId=paris-opera — Stocks par taille pour un article.</summary>
    [HttpGet("{articleId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<StockEntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

    /// <summary>GET /api/stock/articles?storeId=paris-opera&amp;page=1&amp;pageSize=50 — Tous articles + stock pour un magasin.</summary>
    [HttpGet("articles")]
    [ProducesResponseType(typeof(PagedResult<ArticleStockDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
