using CodingChallenge.Web.ApiClients;
using CodingChallenge.Web.Components;

namespace CodingChallenge.Web;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddRazorComponents()
            .AddInteractiveServerComponents();

        var apiBaseUrl = Configuration["ApiBaseUrl"] ?? "http://localhost:5220";

        services.AddSingleton<UserSession>();
        services.AddTransient<AuthTokenHandler>();

        services.AddHttpClient<AuthApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl));

        services.AddHttpClient<OrdersApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl))
            .AddHttpMessageHandler<AuthTokenHandler>();
        services.AddHttpClient<BoardsApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl))
            .AddHttpMessageHandler<AuthTokenHandler>();
        services.AddHttpClient<ComponentsApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl))
            .AddHttpMessageHandler<AuthTokenHandler>();
        services.AddHttpClient<AdminApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl))
            .AddHttpMessageHandler<AuthTokenHandler>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (!env.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

        app.UseRouting();
        app.UseAntiforgery();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapStaticAssets();
            endpoints.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();
        });
    }
}
