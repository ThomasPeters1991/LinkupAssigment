using Microsoft.Extensions.Options;
using MQManager.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<RabbitMQSettingsDTO>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddSingleton<RabbitMQSetupService>(sp =>
{
    var config = sp.GetRequiredService<IOptions<RabbitMQSettingsDTO>>().Value;
    var logger = sp.GetRequiredService<ILogger<RabbitMQSetupService>>();
    return new RabbitMQSetupService(sp.GetRequiredService<IOptions<RabbitMQSettingsDTO>>(), logger);
});
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var rabbitMqSetupService = scope.ServiceProvider.GetRequiredService<RabbitMQSetupService>();
    await rabbitMqSetupService.InitializeAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MQ Manager API V1");
        c.RoutePrefix = string.Empty; // To serve the Swagger UI at the app's root
    });

}

app.UseAuthorization();

app.MapControllers();

app.Run();
