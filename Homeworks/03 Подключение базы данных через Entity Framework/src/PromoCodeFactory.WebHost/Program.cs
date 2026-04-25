using PromoCodeFactory.DataAccess;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder();

        var services = builder.Services;
        var environment = builder.Environment;
        
        services.AddEfDataAccess(environment.ContentRootPath);

        services.AddProblemDetails();
        services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
            });
        services.AddControllers();


        services.AddOpenApi(environment);

        var app = builder.Build();

        app.UseExceptionHandler();

        app.MapOpenApi();
        app.MapSwaggerUI();

        app.UseHttpsRedirection();

        app.MapControllers();

        app.MigrateDatabase();

        if (app.Environment.IsDevelopment())
            await app.SeedDatabase();

        app.Run();
    }
}
