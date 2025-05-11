using Microsoft.Data.SqlClient;
using Tutorial8.Exceptions;
using Tutorial9.Model.DTOs;

namespace Tutorial9.Services;

public class WarehousesService : IWarehousesService
{
    private readonly string _connectionString;
    private readonly IOrdersService _ordersService;
    private readonly IProductsService _productsService;

    public WarehousesService(IConfiguration configuration, IOrdersService ordersService,
        IProductsService productsService)
    {
        _connectionString = configuration.GetConnectionString("Default");
        _ordersService = ordersService;
        _productsService = productsService;
    }

    public async Task<bool> IsWarehouseExistAsync(int warehouseId, CancellationToken ct)
    {
        string query = "Select count(*) from Warehouse where IdWarehouse = @warehouseId";
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(query, conn))
        {
            cmd.Parameters.AddWithValue("@warehouseId", warehouseId);
            await conn.OpenAsync(ct);
            int count = (int)await cmd.ExecuteScalarAsync(ct);
            return count > 0;
        }
    }

    public async Task<int> CreateProductToWarehouseAsync(CreateWarehouseProductDTO dto, CancellationToken ct)
    {
        int OrderId = 0;
        if (!await _productsService.IsProductExistAsync(dto.IdProduct, ct))
        {
            throw new NotFoundException("Product not found");
        }

        if (!await IsWarehouseExistAsync(dto.IdWarehouse, ct))
        {
            throw new NotFoundException("Warehouse not found");
        }

        OrderId = await _ordersService.IsProductInOrderExists(dto.IdProduct, dto.Amount, dto.CreatedAt, ct);
        if (OrderId == 0)
        {
            throw new NotFoundException("Order not found");
        }

        if (await _ordersService.IsOrderFulfilled(OrderId, ct))
        {
            throw new ConflictException("Order is already fulfilled");
        }

        if (await IsOrderInWarehouseAsync(OrderId, ct))
        {
            throw new ConflictException("Order is already in warehouse");
        }
        
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync(ct);
            using (SqlTransaction transaction = conn.BeginTransaction())
            {

                try
                {
                    decimal price = 0;
                    string takeThePrice = "Select Price from Product where IdProduct = @IdProduct";
                    using (SqlCommand cmd = new SqlCommand(takeThePrice, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@IdProduct", dto.IdProduct);
                        using (var reader = await cmd.ExecuteReaderAsync(ct))
                        {
                            if (await reader.ReadAsync(ct))
                            {
                                price = reader.GetDecimal(0);
                            }
                        }
                    }

                    await _ordersService.UpdateFulfillDate(OrderId, ct,conn,transaction);

                    string query = @"Insert into Product_Warehouse(IdWarehouse,IdProduct,IdOrder,Amount,Price,CreatedAt)
                         values(@IdWarehouse,@IdProduct,@IdOrder,@Amount,@Price,@CreatedAt)
                         SELECT SCOPE_IDENTITY()";

                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@IdWarehouse", dto.IdWarehouse);
                        cmd.Parameters.AddWithValue("@IdProduct", dto.IdProduct);
                        cmd.Parameters.AddWithValue("@IdOrder", OrderId);
                        cmd.Parameters.AddWithValue("@Amount", dto.Amount);
                        cmd.Parameters.AddWithValue("@Price", price * dto.Amount);
                        cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                        var newId = await cmd.ExecuteScalarAsync(ct);
                        await transaction.CommitAsync(ct);
                        return Convert.ToInt32(newId);
                    }
                }
                catch
                {
                    await transaction.RollbackAsync(ct);
                    throw;
                }
            }
        }
    }

    public async Task<bool> IsOrderInWarehouseAsync(int orderId, CancellationToken ct)
    {
        string query = "Select count(*) from Product_Warehouse where IdOrder = @orderId";
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(query, conn))
        {
            cmd.Parameters.AddWithValue("@orderId", orderId);
            await conn.OpenAsync(ct);
            int count = (int)await cmd.ExecuteScalarAsync(ct);
            return count > 0;
        }
    }
}