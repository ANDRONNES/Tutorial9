using System.Transactions;
using Microsoft.Data.SqlClient;

namespace Tutorial9.Services;

public class OrdersService : IOrdersService
{
    private readonly string _connectionString;

    public OrdersService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default");
    }

    public async Task<int> IsProductInOrderExistsAsync(int IdProduct, int Amount, DateTime PutIntoWarehouse,
        CancellationToken ct)
    {
        string query =
            "Select IdOrder,CreatedAt from [Order] where IdProduct = @IdProduct and Amount = @Amount";
        
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync(ct);
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@IdProduct", IdProduct);
                cmd.Parameters.AddWithValue("@Amount", Amount);
                using (var reader = await cmd.ExecuteReaderAsync(ct))
                {
                   if (await reader.ReadAsync(ct))
                    {
                        int IdOrder = (int) reader.GetInt32(0);
                        if (!reader.IsDBNull(1))
                        {
                            return IdOrder;
                        }
                        else
                        {
                            DateTime CreatedAt = reader.GetDateTime(1);
                            if (CreatedAt < PutIntoWarehouse)
                            {
                                return IdOrder;
                            }
                            else
                            {
                                return 0;
                            }
                        }
                    }
                   else
                   {
                       return 0;
                   }
                }
            }
        }
    }

    public async Task<bool> IsOrderFulfilledAsync(int orderId, CancellationToken ct)
    {
        DateTime? fulfilledAt = null;
        string query =
            "Select FulfilledAt from [Order] where IdOrder = @id";
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(query, conn))
        {
            cmd.Parameters.AddWithValue("@id", orderId);

            await conn.OpenAsync(ct);
            using (var reader = await cmd.ExecuteReaderAsync(ct))
            {
                if (await reader.ReadAsync(ct))
                {
                    if (!reader.IsDBNull(0))
                    {
                        fulfilledAt = reader.GetDateTime(0);
                    }
                }
            }

            return fulfilledAt.HasValue;
        }
    }

    public async Task UpdateFulfillDateAsync(int orderId,DateTime CreatedAt,CancellationToken ct,SqlConnection conn,SqlTransaction transaction)
    {
        string query = "Update [Order] set FulfilledAt = @fulfilledAt where IdOrder = @orderId";
        using (SqlCommand cmd = new SqlCommand(query, conn,transaction))
        {
            cmd.Parameters.AddWithValue("@fulfilledAt", CreatedAt);
            cmd.Parameters.AddWithValue("@orderId", orderId);
            await cmd.ExecuteNonQueryAsync(ct);
        }
    }
}