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
public class StoresController : ControllerBase
{
    private readonly IStoreService _storeService;

    public StoresController(IStoreService storeService)
    {
        _storeService = storeService;
    }

    /// <summary>Retourne la liste de tous les magasins.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StoreDto>>> GetAll(
        CancellationToken cancellationToken = default)
    {
        var stores = await _storeService.GetAllAsync(cancellationToken);
        return Ok(stores);
    }
}
