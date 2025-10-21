using MudBlazor.Services;
using MyApp.BlazorUI.Components;
using MyApp.BlazorUI.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using MyApp.BlazorUI.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add Razor Components and MudBlazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

// ✅ HttpClient didefinisikan sebelum build()
builder.Services.AddScoped(sp => new HttpClient
{
  BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!)
});

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<InvoiceService>();


// Blazored LocalStorage (for token storage)
builder.Services.AddBlazoredLocalStorage();

// Authentication & Authorization
builder.Services.AddAuthentication("Identity.Application")  // 🔹 Needed for [Authorize]
    .AddCookie("Identity.Application", options =>
    {
      options.LoginPath = "/login";
      options.LogoutPath = "/logout";
      options.AccessDeniedPath = "/access-denied";
    });

builder.Services.AddAuthorizationCore(options =>
{
  // Set fallback policy to not require authentication for HTTP pipeline
  options.FallbackPolicy = null;
});

builder.Services.AddCascadingAuthenticationState();

// Register the AuthTokenHandler
builder.Services.AddScoped<AuthTokenHandler>();

// Register HttpClient for AuthClient + attach AuthTokenHandler
builder.Services.AddHttpClient<IAuthClient, AuthClient>(client =>
{
  // Make sure this URL matches your backend API base
  client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]
      ?? "http://localhost:5099/");
})
.AddHttpMessageHandler<AuthTokenHandler>();

// Register other services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

// Build the app
var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Error", createScopeForErrors: true);
  app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
