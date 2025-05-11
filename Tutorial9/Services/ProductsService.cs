using Microsoft.Data.SqlClient;

namespace Tutorial9.Services;

public class ProductsService : IProductsService
{
    private readonly string _connectionString;

    public ProductsService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default");
    }
    
    public async Task<bool> IsProductExistAsync(int productId,CancellationToken ct)
    {
        string query = "Select count(*) from Product where IdProduct = @productId";
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                await conn.OpenAsync(ct);
                cmd.Parameters.AddWithValue("@productId", productId);
                int count = (int) await cmd.ExecuteScalarAsync(ct);
                return count > 0;
            }
        }
    }
}