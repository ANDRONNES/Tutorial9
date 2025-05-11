using Microsoft.AspNetCore.Mvc;
using Tutorial9.Model.DTOs;
using Tutorial9.Services;

namespace Tutorial9.Controllers;

[ApiController]
[Route("[controller]")]
public class WarehouseController : ControllerBase
{
    private readonly IWarehousesService _warehousesService;

    public WarehouseController(IWarehousesService warehouseService)
    {
        _warehousesService = warehouseService;
    }
    
    [HttpPost()]
    public async Task<IActionResult> PostProductToWarehouse([FromBody] CreateWarehouseProductDTO warehouseProductDto,CancellationToken ct)
    {
        var newId = await _warehousesService.CreateProductToWarehouseAsync(warehouseProductDto, ct);
        return Created("",new { messege = "Order added into warehouse.",Id = newId });
    }
}