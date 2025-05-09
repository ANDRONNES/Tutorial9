namespace Tutorial9.Services;

public interface IOrdersService
{
    Task<int> IsProductInOrderExists(int IdProduct,int Amount,DateTime CreatedAt,CancellationToken ct);
    Task<bool> IsOrderFulfilled(int orderId,CancellationToken ct);
    Task<bool> UpdateFulfillDate(int orderId,CancellationToken ct);
}