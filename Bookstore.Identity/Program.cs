using Bookstore.Identity.Configuration;
using Bookstore.Identity.Users;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

builder.Services.AddControllersWithViews();

builder.Services.AddIdentityServer(options =>
{
    options.IssuerUri =
        builder.Configuration["IdentityServer:IssuerUri"];

    if (builder.Environment.IsDevelopment())
    {
        options.Authentication.CookieSameSiteMode = SameSiteMode.Lax;
        options.Authentication.CheckSessionCookieSameSiteMode = SameSiteMode.Lax;
    }
})
    .AddInMemoryApiScopes(IdentityServerConfig.ApiScopes)
    .AddInMemoryApiResources(IdentityServerConfig.ApiResources)
    .AddInMemoryClients(
        IdentityServerConfig.GetClients(
            builder.Configuration["Swagger:ApiUrl"]!.TrimEnd('/')))
    .AddTestUsers(TestUsers.Users);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();

app.UseIdentityServer();

app.MapDefaultControllerRoute();

app.Run();
