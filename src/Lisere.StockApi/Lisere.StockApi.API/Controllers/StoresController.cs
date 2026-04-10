using Lisere.StockApi.Application.DTOs;
using Lisere.StockApi.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Lisere.StockApi.API.Controllers;

[ApiController]
[Route("api/stores")]
[EnableRateLimiting("fixed")]
public class StoresController : ControllerBase
{
    private readonly IStoreRepository _storeRepository;

    public StoresController(IStoreRepository storeRepository)
    {
        _storeRepository = storeRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StoreDto>>> GetAll(
        CancellationToken cancellationToken = default)
    {
        var stores = await _storeRepository.GetAllAsync(cancellationToken);

        var dtos = stores.Select(s => new StoreDto
        {
            Id = s.Id,
            Name = s.Name,
            Code = s.Code,
            Type = s.Type,
        });

        return Ok(dtos);
    }
}
