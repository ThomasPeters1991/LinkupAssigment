using Microsoft.Extensions.Options;
using NotificationService.Services.Configurations;
using NotificationService.Services.Listeners;
using NotificationService.Services.Publishers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddSingleton<RabbitMQService>(sp =>
{
    var options = sp.GetRequiredService<IOptions<RabbitMQSettings>>().Value;
    var logger = sp.GetRequiredService<ILogger<RabbitMQService>>();
    return new RabbitMQService(options.HostName, options.UserName, options.Password, logger);
});
builder.Services.AddSingleton<OrdersListener>();
builder.Services.AddSingleton<publisher>();

var app = builder.Build();

Thread.Sleep(15000);
// Listen the UserServiceListener after application starts
var rabbitMqService = app.Services.GetRequiredService<RabbitMQService>();
var userServiceListener = app.Services.GetRequiredService<OrdersListener>();
userServiceListener.Listen();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NotificationService API V1");
        c.RoutePrefix = string.Empty; // To serve the Swagger UI at the app's root
    });
}
app.UseAuthorization();

app.MapControllers();

app.Run();
