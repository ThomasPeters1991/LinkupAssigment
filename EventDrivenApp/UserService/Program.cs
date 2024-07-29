using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UserService.Services.Configurations;
using UserService.Services.Listeners;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();






builder.Services.AddSingleton<IUserRepository, UserRepository>();

builder.Services.AddDbContext<UserContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddSingleton<RabbitMQService>(sp =>
{
    var options = sp.GetRequiredService<IOptions<RabbitMQSettings>>().Value;
    var logger = sp.GetRequiredService<ILogger<RabbitMQService>>();
    return new RabbitMQService(options.HostName, options.UserName, options.Password, logger);
});
builder.Services.AddSingleton<UserInfoListener>();

// Add DbContext and other services

var app = builder.Build();
Thread.Sleep(15000);
// Start the UserServiceListener after application starts
var rabbitMqService = app.Services.GetRequiredService<RabbitMQService>();
var userServiceListener = app.Services.GetRequiredService<UserInfoListener>();
userServiceListener.Start();


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
