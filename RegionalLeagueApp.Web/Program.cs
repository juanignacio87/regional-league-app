using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RegionalLeagueApp.Web.Components;
using RegionalLeagueApp.Application;
using RegionalLeagueApp.Application.Abstractions.Identity;
using RegionalLeagueApp.Infrastructure;
using RegionalLeagueApp.Infrastructure.Identity;
using RegionalLeagueApp.Infrastructure.Seed;
using RegionalLeagueApp.Web.Hubs;
using RegionalLeagueApp.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<MatchUpdatesClient>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddSignalR();
builder.Services.AddHealthChecks();

builder.Services
    .AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequireAdmin", policy => policy.RequireRole(AppRoles.Admin))
    .AddPolicy("RequireModerator", policy => policy.RequireRole(AppRoles.Admin, AppRoles.Moderator))
    .AddPolicy("RequireContributor", policy => policy.RequireRole(AppRoles.Admin, AppRoles.Moderator, AppRoles.Contributor));

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/access-denied";
    options.Cookie.Name = "RegionalLeagueApp.Auth";
    options.SlidingExpiration = true;
});

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

await app.Services.InitializeDatabaseAsync();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapPost("/account/login", async (
    [FromForm] LoginRequest request,
    SignInManager<ApplicationIdentityUser> signInManager) =>
{
    var result = await signInManager.PasswordSignInAsync(
        request.Email,
        request.Password,
        request.RememberMe,
        lockoutOnFailure: false);

    return result.Succeeded
        ? Results.LocalRedirect(string.IsNullOrWhiteSpace(request.ReturnUrl) ? "/" : request.ReturnUrl)
        : Results.LocalRedirect($"/login?error=1&returnUrl={Uri.EscapeDataString(request.ReturnUrl ?? string.Empty)}");
}).DisableAntiforgery();

app.MapPost("/account/logout", async (SignInManager<ApplicationIdentityUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.LocalRedirect("/");
}).DisableAntiforgery();

app.MapHub<MatchUpdatesHub>(MatchUpdatesHub.Route);
app.MapHealthChecks("/health");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

public sealed class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
}
