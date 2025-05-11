using Microsoft.Data.SqlClient;

namespace Tutorial9.Services;

public interface IOrdersService
{
    Task<int> IsProductInOrderExistsAsync(int IdProduct,int Amount,DateTime CreatedAt,CancellationToken ct);
    Task<bool> IsOrderFulfilledAsync(int orderId,CancellationToken ct);
    Task UpdateFulfillDateAsync(int orderId,DateTime CreatedAt,CancellationToken ct,SqlConnection conn,SqlTransaction transaction);
}