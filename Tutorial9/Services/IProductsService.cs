namespace Tutorial9.Services;

public interface IProductsService
{
    Task<bool> IsProductExistAsync(int productId,CancellationToken ct);
}