using Tutorial9.Model.DTOs;

namespace Tutorial9.Services;

public interface IWarehousesService
{
    public Task<bool> IsWarehouseExistAsync(int warehouseId,CancellationToken ct);
    public Task<int> CreateProductToWarehouseAsync(CreateWarehouseProductDTO dto,CancellationToken ct);
    public Task<bool> IsOrderInWarehouseAsync(int orderId,CancellationToken ct);
    public Task<int> CreateProductToWarehouseProcedureAsync(CreateWarehouseProductDTO dto,CancellationToken ct);
}