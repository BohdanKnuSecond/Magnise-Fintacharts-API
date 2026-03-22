using MagniseTest.Application.Interfaces;
using MagniseTest.Infrastructure.Data;
using MagniseTest.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHttpClient<MagniseTest.Application.Interfaces.IFintachartsAuthService, MagniseTest.Infrastructure.Fintacharts.FintachartsAuthService>();
builder.Services.AddHttpClient<MagniseTest.Application.Interfaces.IFintachartsInstrumentService, MagniseTest.Infrastructure.Fintacharts.FintachartsInstrumentService>();
builder.Services.AddScoped<IAssetRepository, AssetRepository>();
builder.Services.AddSingleton<MagniseTest.Application.Interfaces.IPriceStorage, MagniseTest.Infrastructure.Services.PriceStorage>();
builder.Services.AddHostedService<MagniseTest.Infrastructure.WebSockets.FintachartsWebSocketService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var instrumentService = scope.ServiceProvider.GetRequiredService<MagniseTest.Application.Interfaces.IFintachartsInstrumentService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    dbContext.Database.EnsureCreated();

    if (!dbContext.Assets.Any())
    {
        logger.LogInformation("Database is empty");

        try
        {
            var assets = await instrumentService.GetInstrumentsAsync();
            if (assets.Any())
            {
                dbContext.Assets.AddRange(assets);
                await dbContext.SaveChangesAsync();
                logger.LogInformation("Successfully saved {Count} assets to the database", assets.Count);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while fetching assets from Fintacharts");
        }
    }
    else
    {
        logger.LogInformation("Assets already exist in the database");
    }
}
app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Magnise API v1");
  
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();