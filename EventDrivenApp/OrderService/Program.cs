using Microsoft.EntityFrameworkCore;
using OrderService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<OrderContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();

Thread.Sleep(20000);
builder.Services.AddSingleton<IRabbitConnection> (new RabbitConnection());
builder.Services.AddScoped<IMessageProducer,RabbitMQPublisherService>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

var dbContext = app.Services.CreateScope().ServiceProvider.GetRequiredService<OrderContext>();
dbContext.Database.Migrate();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrdersService API V1");
        c.RoutePrefix = string.Empty; // To serve the Swagger UI at the app's root
    });

}


app.UseAuthorization();

app.MapControllers();

app.Run();
