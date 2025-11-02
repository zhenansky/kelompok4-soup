using MudBlazor.Services;
using MyApp.BlazorUI.Components;
using MyApp.BlazorUI.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using MyApp.BlazorUI.Services.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ==============================================
// 🔹 JSON Serializer Configuration
// ==============================================
builder.Services.ConfigureHttpJsonOptions(options =>
{
  options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
  options.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
  options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// ==============================================
// 🔹 Razor Components + MudBlazor UI
// ==============================================
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

// ==============================================
// 🔹 Default HttpClient (Backup Client)
// ==============================================
builder.Services.AddScoped(sp => new HttpClient
{
  BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5099/")
});

// ==============================================
// 🔹 Register Local Services
// ==============================================
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<InvoiceService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<MyClassServices>();
builder.Services.AddScoped<CourseService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<ScheduleService>();
builder.Services.AddScoped<ProfileTokenService>();

// ==============================================
// 🔹 LocalStorage (for token storage)
// ==============================================
builder.Services.AddBlazoredLocalStorage();

// ==============================================
// 🔹 Authentication & Authorization
// ==============================================
builder.Services.AddAuthentication("Identity.Application")
    .AddCookie("Identity.Application", options =>
    {
      options.LoginPath = "/login";
      options.LogoutPath = "/logout";
      options.AccessDeniedPath = "/access-denied";
    });

builder.Services.AddAuthorizationCore(options =>
{
  options.FallbackPolicy = null; // Tidak wajib login di semua halaman
});

builder.Services.AddCascadingAuthenticationState();

// ==============================================
// 🔹 Token Handler (untuk otomatis kirim Bearer Token)
// ==============================================
builder.Services.AddTransient<AuthTokenHandler>();

// ==============================================
// 🔹 HttpClient Registrations per Service
// ==============================================
builder.Services.AddHttpClient<UserService>(client =>
{
  client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5099/");
});

builder.Services.AddHttpClient<PaymentService>(client =>
{
  client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5099/");
});

builder.Services.AddHttpClient<IAuthClient, AuthClient>(client =>
{
  client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5099/");
}).AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddHttpClient<InvoiceService>(client =>
{
  client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5099/");
}).AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddHttpClient<DashboardService>(client =>
{
  client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5099/");
}).AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddHttpClient<MyClassServices>(client =>
{
  client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5099/");
}).AddHttpMessageHandler<AuthTokenHandler>();

// ==============================================
// 🔹 Auth & Cart Services
// ==============================================
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<MyApp.BlazorUI.Services.CartService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

// ==============================================
// 🔹 Build & Middleware Pipeline
// ==============================================
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Error", createScopeForErrors: true);
  app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
