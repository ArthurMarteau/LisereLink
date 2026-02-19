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

    /// <summary>GET /api/stock/{articleId}?storeId=paris-opera</summary>
    [HttpGet("{articleId:guid}")]
    public async Task<IActionResult> GetByArticle(
        Guid articleId,
        [FromQuery] string storeId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storeId))
            return BadRequest("Le paramètre storeId est requis.");

        var stock = await _stockService.GetStockByArticleAsync(articleId, storeId, cancellationToken);
        return Ok(stock);
    }

    /// <summary>GET /api/stock/articles?storeId=paris-opera&amp;page=1&amp;pageSize=50</summary>
    [HttpGet("articles")]
    public async Task<IActionResult> GetByStore(
        [FromQuery] string storeId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storeId))
            return BadRequest("Le paramètre storeId est requis.");

        var result = await _stockService.GetStockByStoreAsync(storeId, page, pageSize, cancellationToken);
        return Ok(result);
    }
}
