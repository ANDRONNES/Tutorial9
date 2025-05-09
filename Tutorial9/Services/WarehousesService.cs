using Microsoft.Data.SqlClient;
using Tutorial9.Model.DTOs;

namespace Tutorial9.Services;

public class WarehousesService : IWarehousesService
{
    private readonly string _connectionString;
    private readonly IOrdersService _ordersService;
    private readonly IProductsService _productsService;

    public WarehousesService(IConfiguration configuration, IOrdersService ordersService, IProductsService productsService)
    {
        _connectionString = configuration["ConnectionStrings"];
        _ordersService = ordersService;
        _productsService = productsService;
    }
    
    public async Task<bool> IsWarehouseExistAsync(int warehouseId,CancellationToken ct)
    {
        string query = "Select count(*) from Warehouses where WarehouseId = @warehouseId";
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(query, conn))
        {
            cmd.Parameters.AddWithValue("@warehouseId", warehouseId);
            await conn.OpenAsync(ct);
            int count = (int) await cmd.ExecuteScalarAsync(ct);
            return count > 0;
        }
    }

    public async Task<int> CreateProductToWarehouseAsync(CreateWarehouseProductDTO dto,CancellationToken ct)
    {
        int OrderId = 0;
        if (!await _productsService.IsProductExistAsync(dto.IdProduct, ct))
        {
            return 0;
        }

        if (!await IsWarehouseExistAsync(dto.IdWarehouse, ct))
        {
            return 0;
        }
        
        OrderId = await _ordersService.IsProductInOrderExists(dto.IdProduct, dto.Amount, dto.CreatedAt, ct);
        if (OrderId == 0)
        {
            return 0;
        }
        
        if(await _ordersService.IsOrderFulfilled(OrderId, ct))
        {
            return 0;
        }
        
        if(await IsOrderInWarehouseAsync(OrderId,ct))
        {
            return 0;
        }
        _ordersService.UpdateFulfillDate(OrderId, ct);
        decimal price = 0;
        string takeThePrice = "Select Price from Product where IdProduct = @IdProduct";
        string query = @"Insert into Product_Warehouse(IdWarehouse,IdProduct,IdOrder,Amount,Price,CreatedAt)
                         values(@IdWarehouse,@IdProduct,@IdOrder,@Amount,@Price,@CreatedAt)
                         SELECT SCOPE_IDENTITY()";
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync(ct);
            using (SqlCommand cmd = new SqlCommand(takeThePrice, conn))
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
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@IdWarehouse", dto.IdWarehouse);
                cmd.Parameters.AddWithValue("@IdProduct", dto.IdProduct);
                cmd.Parameters.AddWithValue("@IdOrder", OrderId);
                cmd.Parameters.AddWithValue("@Amount", dto.Amount);
                cmd.Parameters.AddWithValue("@Price",price*dto.Amount);
                var newId =await cmd.ExecuteScalarAsync(ct);
                return Convert.ToInt32(newId);
            }
        }
        
        
    }

    public async Task<bool> IsOrderInWarehouseAsync(int orderId, CancellationToken ct)
    {
        string query = "Select count(*) from Product_Warehouse where OrderId = @orderId";
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(query, conn))
        {
            cmd.Parameters.AddWithValue("@orderId",orderId);
            await conn.OpenAsync(ct);
            int count = (int) await cmd.ExecuteScalarAsync(ct);
            return count > 0;
        }
    }
}