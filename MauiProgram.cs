using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Grocery.Core.Data;
using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Data.Repositories;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        // Required for MAUI app to wire up the App class
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                // keep existing fonts or leave empty if none configured
            });

        // Register DbContext and repository
        builder.Services.AddDbContext<GroceryDbContext>(options =>
            options.UseInMemoryDatabase("GroceryDb"));

        builder.Services.AddTransient<IGroceryListItemsRepository, GroceryListItemsRepository>();

        var app = builder.Build();

        // Seed DB
        DbInitializer.Seed(app.Services);

        // TEMP: verify repository reads seeded items (remove afterwards)
        using var scope = app.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IGroceryListItemsRepository>();
        var items = repo.GetAll();
        Debug.WriteLine($"GroceryListItems count: {items.Count}");
        foreach (var it in items)
        {
            Debug.WriteLine($"Id={it.Id}, GroceryListId={it.GroceryListId}, ProductId={it.ProductId}, Amount={it.Amount}");
        }

        return app;
    }
}