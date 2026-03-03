using Lisere.Application.Common;
using Lisere.Application.DTOs;
using Lisere.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lisere.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ArticlesController : ControllerBase
{
    private readonly IArticleService _articleService;

    public ArticlesController(IArticleService articleService)
    {
        _articleService = articleService;
    }

    /// <summary>Recherche paginée d'articles depuis Lisere.StockApi.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ArticleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ArticleDto>>> Search(
        [FromQuery] string? query = null,
        [FromQuery] string? family = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _articleService.SearchAsync(query, family, page, pageSize, cancellationToken);
        return Ok(result);
    }

    /// <summary>Récupère un article par son code-barres EAN-13.</summary>
    [HttpGet("{barcode}")]
    [ProducesResponseType(typeof(ArticleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ArticleDto>> GetByBarcode(
        string barcode,
        CancellationToken cancellationToken = default)
    {
        var article = await _articleService.GetByBarcodeAsync(barcode, cancellationToken);
        if (article is null)
            return NotFound();

        return Ok(article);
    }
}
