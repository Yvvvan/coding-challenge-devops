using CodingChallenge.Api.Data;
using CodingChallenge.Api.Data.Seed;
using CodingChallenge.Api.Managers;
using CodingChallenge.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace CodingChallenge.Api;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(Configuration.GetConnectionString("Default")));

        services.AddScoped<IOrderManager, OrderManager>();
        services.AddScoped<IBoardManager, BoardManager>();
        services.AddScoped<IComponentManager, ComponentManager>();

        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IBoardService, BoardService>();
        services.AddScoped<IComponentService, ComponentService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddLogging(builder => builder.AddLog4Net("log4net.config"));

        services.AddOpenApiDocument(config =>
        {
            config.DocumentName = "v1";
            config.Title = "CodingChallenge API";
            config.Version = "v1";
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        using (var scope = app.ApplicationServices.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.Migrate();
            DbSeeder.SeedSampleData(context);
        }

        if (!env.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseRouting();

        app.UseOpenApi();
        app.UseSwaggerUi(config => config.DocumentTitle = "CodingChallenge API");

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
