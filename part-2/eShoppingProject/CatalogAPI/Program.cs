using Asp.Versioning;
using CatalogAPI.Data;
using CatalogAPI.Filters;
using CatalogAPI.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

var appName = "catalogapi";
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddApiVersioning(ops =>
{
    ops.AssumeDefaultVersionWhenUnspecified = true;
    ops.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);

    //Default Media Type Versioning
    //ops.ApiVersionReader = new MediaTypeApiVersionReader();

    //Media Type Versioning with Custom Media Type
    //var builder = new MediaTypeApiVersionReaderBuilder();
    //ops.ApiVersionReader = builder.Template("application/vnd.my.app.{version}+json")
    //                        .Build();
   
}).AddApiExplorer();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen(ops => ops.OperationFilter<SwaggerDefaultValues>());

builder.Services.AddDbContext<CatalogContext>(ops =>
{
    ops.UseSqlServer(builder.Configuration.GetConnectionString("Default"),
        sqlServerOptionsAction: sqlOps =>
        {
            sqlOps.MigrationsAssembly(
                typeof(Program).GetTypeInfo().Assembly.GetName().Name);
            sqlOps.EnableRetryOnFailure(maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(ops =>
    {
        foreach (var description in app.DescribeApiVersions())
        {
            ops.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                $"Catalog API {description.GroupName}");
        }
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

//app.Run();

try
{
    app.Logger.LogInformation("Configuring web api {AppName}", appName);
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<CatalogContext>();
    var logger = scope.ServiceProvider.GetService<ILogger<CatalogContextSeed>>();
    await context.Database.MigrateAsync();

    await new CatalogContextSeed().SeedAsync(context, logger);
    app.Logger.LogInformation("Starting web api {AppName}", appName);

    await app.RunAsync();

    return 0;
}
catch (Exception ex)
{
    app.Logger.LogCritical(ex, "Program terminated {AppName}", appName);
    return 1;
}
