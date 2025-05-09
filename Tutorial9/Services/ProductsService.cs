using Microsoft.Data.SqlClient;

namespace Tutorial9.Services;

public class ProductsService : IProductsService
{
    private readonly string _connectionString;

    public ProductsService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ConnectionStrings");
    }
    
    public async Task<bool> IsProductExistAsync(int productId,CancellationToken ct)
    {
        string query = "Select count(*) from Product where Id = @productId";
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@productId", productId);
                await conn.OpenAsync(ct);
                int count = (int) await cmd.ExecuteScalarAsync(ct);
                return count > 0;
            }
        }
    }
}