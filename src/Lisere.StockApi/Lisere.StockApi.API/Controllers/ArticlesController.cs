using Microsoft.AspNetCore.Mvc;

namespace Lisere.StockApi.API.Controllers;

[ApiController]
[Route("api/articles")]
public class ArticlesController : ControllerBase
{
    private readonly Lisere.StockApi.Domain.Interfaces.IArticleRepository _articleRepository;

    public ArticlesController(Lisere.StockApi.Domain.Interfaces.IArticleRepository articleRepository)
    {
        _articleRepository = articleRepository;
    }

    /// <summary>GET /api/articles?page=1&amp;pageSize=50</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _articleRepository.GetAllAsync(page, pageSize, cancellationToken);
        return Ok(new { items, totalCount, page, pageSize });
    }

    /// <summary>GET /api/articles/{barcode}</summary>
    [HttpGet("{barcode}")]
    public async Task<IActionResult> GetByBarcode(
        string barcode,
        CancellationToken cancellationToken = default)
    {
        var article = await _articleRepository.GetByBarcodeAsync(barcode, cancellationToken);

        if (article is null)
            return NotFound();

        return Ok(article);
    }
}
