using Microsoft.Data.SqlClient;

namespace Tutorial9.Services;

public interface IOrdersService
{
    Task<int> IsProductInOrderExists(int IdProduct,int Amount,DateTime CreatedAt,CancellationToken ct);
    Task<bool> IsOrderFulfilled(int orderId,CancellationToken ct);
    Task UpdateFulfillDate(int orderId,CancellationToken ct,SqlConnection conn,SqlTransaction transaction);
}