using Tutorial9.Middlewares;
using Tutorial9.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddScoped<IDbService, DbService>();
builder.Services.AddScoped<IWarehousesService,WarehousesService>();
builder.Services.AddScoped<IProductsService,ProductsService>();
builder.Services.AddScoped<IOrdersService,OrdersService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseGlobalExceptionHandling();

app.UseAuthorization();

app.MapControllers();

app.Run();