using Lisere.Application.Common;
using Lisere.Application.DTOs;
using Lisere.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lisere.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RequestsController : ControllerBase
{
    private readonly IRequestService _requestService;

    public RequestsController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    /// <summary>Liste paginée des demandes, filtrable par zone et statut.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<RequestDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<RequestDto>>> GetAll(
        [FromQuery] string? zone = null,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _requestService.GetAllAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }

    /// <summary>Récupère une demande par son identifiant.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
}
