using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UserService.Services;
using UserService.Services.Configurations;
using UserService.Services.Listeners;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQ"));

builder.Services.AddDbContext<UserContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddHostedService<RabbitMQConsumerService>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();
Thread.Sleep(15000);

var dbContext = app.Services.CreateScope().ServiceProvider.GetRequiredService<UserContext>();
dbContext.Database.Migrate();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "UserService API V1");
        c.RoutePrefix = string.Empty; // To serve the Swagger UI at the app's root
    });

}

app.UseAuthorization();

app.MapControllers();

app.Run();
