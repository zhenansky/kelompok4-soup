using MudBlazor.Services;
using MyApp.BlazorUI.Components;
using MyApp.BlazorUI.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using MyApp.BlazorUI.Services.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
  options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
  options.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
  options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Add Razor Components and MudBlazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

// ✅ HttpClient default (backup)
builder.Services.AddScoped(sp => new HttpClient
{
  BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!)
});

// Register local services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<InvoiceService>();
builder.Services.AddScoped<DashboardService>();

// Blazored LocalStorage (for token storage)
builder.Services.AddBlazoredLocalStorage();

// Authentication & Authorization
builder.Services.AddAuthentication("Identity.Application")
    .AddCookie("Identity.Application", options =>
    {
      options.LoginPath = "/login";
      options.LogoutPath = "/logout";
      options.AccessDeniedPath = "/access-denied";
    });

builder.Services.AddAuthorizationCore(options =>
{
  options.FallbackPolicy = null; // tidak wajib auth di semua halaman
});

builder.Services.AddCascadingAuthenticationState();

// Register AuthTokenHandler
builder.Services.AddTransient<AuthTokenHandler>();

builder.Services.AddHttpClient<UserService>(client =>
{
  client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]
      ?? "http://localhost:5099/");
});
builder.Services.AddHttpClient<PaymentService>(client =>
{
  client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]
      ?? "http://localhost:5099/");
});

// ✅ HttpClient untuk AuthClient (pakai token)
builder.Services.AddHttpClient<IAuthClient, AuthClient>(client =>
{
  client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]
      ?? "http://localhost:5099/");
})
.AddHttpMessageHandler<AuthTokenHandler>();

// ✅ HttpClient untuk InvoiceService (butuh token)
builder.Services.AddHttpClient<InvoiceService>(client =>
{
  client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]
      ?? "http://localhost:5099/");
});

// 🚫 DashboardService tidak perlu token
builder.Services.AddHttpClient<DashboardService>(client =>
{
  client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]
      ?? "http://localhost:5099/");
});

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

// ✅ Pastikan wwwroot bisa diakses
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode(); // 🔹 Hapus opsi Prerender, biarkan default

app.Run();
