using Basket.API.Infrastructure.Repositories;
using BasketAPI.Extensions;
using BasketAPI.Infrastructure.Contracts;
using BasketAPI.Services;
using BasketAPI.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<IBasketRepository, BasketRedisRespository>();
builder.Services.AddTransient<IIdentityService, IdentityService>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.Configure<BasketSettings>(builder.Configuration);

builder.Services
    .AddRedis()
    .AddIntegrationEventServices()
    .AddEventBus();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.SubscribeEventsAsync();

await app.RunAppAsync();

public partial class Program
{

    public static string? Namespace = typeof(Program).Assembly.GetName().Name;
    public static string? AppName = Namespace;
}