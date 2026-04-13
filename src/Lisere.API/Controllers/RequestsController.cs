using System.ComponentModel.DataAnnotations;
using Lisere.Application.Common;
using Lisere.Application.DTOs;
using Lisere.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Lisere.API.Controllers;

public record TakeRequestDto([Required] Guid StockistId);

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("fixed")]
public class RequestsController : ControllerBase
{
    private readonly IRequestService _requestService;

    public RequestsController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    /// <summary>Liste paginée des demandes, filtrable par magasin et zone.</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<RequestDto>>> GetAll(
        [FromQuery] string? storeId = null,
        [FromQuery] string? zone = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _requestService.GetAllAsync(page, pageSize, storeId, zone, cancellationToken);
        return Ok(result);
    }

    /// <summary>Récupère une demande par son identifiant.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequestDto>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var request = await _requestService.GetByIdAsync(id, cancellationToken);
        if (request is null)
            return NotFound();

        return Ok(request);
    }

    /// <summary>Crée une nouvelle demande.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(RequestDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RequestDto>> Create(
        [FromBody] CreateRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var request = await _requestService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = request.Id }, request);
    }

    /// <summary>Met à jour une demande (uniquement si statut = Pending).</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RequestDto>> Update(
        Guid id,
        [FromBody] UpdateRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var request = await _requestService.UpdateAsync(id, dto, cancellationToken);
        return Ok(request);
    }

    /// <summary>Annule une demande (suppression logique).</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _requestService.CancelAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>Le stockiste prend la demande en charge (Pending → InProgress).</summary>
    [HttpPost("{id:guid}/take")]
    [ProducesResponseType(typeof(RequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RequestDto>> Take(
        Guid id,
        [FromBody] TakeRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var request = await _requestService.TakeInProgressAsync(id, dto.StockistId, cancellationToken);
        return Ok(request);
    }

    /// <summary>Le stockiste marque une ligne comme trouvée.</summary>
    [HttpPost("{id:guid}/lines/{lineId:guid}/found")]
    [ProducesResponseType(typeof(RequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RequestDto>> MarkLineFound(
        Guid id,
        Guid lineId,
        CancellationToken cancellationToken = default)
    {
        var request = await _requestService.MarkLineFoundAsync(id, lineId, cancellationToken);
        return Ok(request);
    }

    /// <summary>Le stockiste marque une ligne comme non trouvée.</summary>
    [HttpPost("{id:guid}/lines/{lineId:guid}/not-found")]
    [ProducesResponseType(typeof(RequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RequestDto>> MarkLineNotFound(
        Guid id,
        Guid lineId,
        CancellationToken cancellationToken = default)
    {
        var request = await _requestService.MarkLineNotFoundAsync(id, lineId, cancellationToken);
        return Ok(request);
    }

    /// <summary>Le stockiste propose des alternatives au vendeur.</summary>
    [HttpPost("{id:guid}/propose-alternatives")]
    [ProducesResponseType(typeof(RequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RequestDto>> ProposeAlternatives(
        Guid id,
        [FromBody] ProposeAlternativesDto dto,
        CancellationToken cancellationToken = default)
    {
        var request = await _requestService.ProposeAlternativesAsync(id, dto, cancellationToken);
        return Ok(request);
    }

    /// <summary>Le vendeur accepte ou refuse les alternatives proposées.</summary>
    [HttpPost("{id:guid}/respond-alternatives")]
    [ProducesResponseType(typeof(RequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RequestDto>> RespondToAlternatives(
        Guid id,
        [FromBody] RespondToAlternativesDto dto,
        CancellationToken cancellationToken = default)
    {
        var request = await _requestService.RespondToAlternativesAsync(id, dto, cancellationToken);
        return Ok(request);
    }
}
