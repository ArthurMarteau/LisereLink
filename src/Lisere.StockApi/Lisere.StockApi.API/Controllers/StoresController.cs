using Lisere.StockApi.Application.DTOs;
using Lisere.StockApi.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Lisere.StockApi.API.Controllers;

[ApiController]
[Route("api/stores")]
[EnableRateLimiting("fixed")]
public class StoresController : ControllerBase
{
    private readonly IStoreService _storeService;

    public StoresController(IStoreService storeService)
    {
        _storeService = storeService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StoreDto>>> GetAll(
        CancellationToken cancellationToken = default)
    {
        var dtos = await _storeService.GetAllAsync(cancellationToken);
        return Ok(dtos);
    }
}
