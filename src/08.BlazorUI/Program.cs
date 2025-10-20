using MudBlazor.Services;
using MyApp.BlazorUI.Components;
using MyApp.BlazorUI.Services;

var builder = WebApplication.CreateBuilder(args);

// 🧩 Tambahkan semua service di sini sebelum builder.Build()
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


var app = builder.Build();

// ✅ Setelah build, baru konfigurasi pipeline
if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Error", createScopeForErrors: true);
  app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
