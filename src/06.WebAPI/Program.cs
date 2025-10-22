using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using MyApp.WebAPI.Data;
using MyApp.WebAPI.Models;
using MyApp.WebAPI.Middlewares;
using MyApp.WebAPI.Services;
using MyApp.WebAPI.Services.Interfaces;
using MyApp.WebAPI.Configuration;
using System.Text;
using MyApp.WebAPI.Services.Implementations;
using MyApp.WebAPI.Extensions;


var builder = WebApplication.CreateBuilder(args);

// ===============================================
// 1️⃣ Connection String & Database Configuration
// ===============================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));


builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
  options.Password.RequiredLength = 6;
  options.Password.RequireDigit = true;
  options.Password.RequireLowercase = true;
  options.Password.RequireUppercase = true;
  options.Password.RequireNonAlphanumeric = false;

  options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
  options.Lockout.MaxFailedAccessAttempts = 5;
  options.Lockout.AllowedForNewUsers = true;

  // User settings
  options.User.RequireUniqueEmail = true;
  options.SignIn.RequireConfirmedEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

var jwtSettings = new JwtSettings();
builder.Configuration.GetSection("JwtSettings").Bind(jwtSettings);
builder.Services.AddSingleton(jwtSettings);
builder.Services.AddAuthentication(options =>
{
  options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
  options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
  options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
  .AddJwtBearer(options =>
  {
    options.TokenValidationParameters = new TokenValidationParameters
    {
      ValidateIssuer = jwtSettings.ValidateIssuer,
      ValidateAudience = jwtSettings.ValidateAudience,
      ValidateLifetime = jwtSettings.ValidateLifetime,
      ValidateIssuerSigningKey = jwtSettings.ValidateIssuerSigningKey,
      ValidIssuer = jwtSettings.Issuer,
      ValidAudience = jwtSettings.Audience,
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
      ClockSkew = TimeSpan.FromMinutes(jwtSettings.ClockSkew)
    };
  });

builder.Services.AddAuthorization(options =>
{
  AuthorizationPolicies.ConfigurePolicies(options);
});

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IMyClassService, MyClassService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// ===============================================
// 3️⃣ Register Services (Dependency Injection)
// ===============================================
// Di sini kita daftarkan semua service yang kita pisahkan ke folder Services/
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddApplicationServices();

// Mendefinisikan kebijakan CORS "AllowAll"
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowAll",
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:5124") 
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

// ===============================================
// 4️⃣ Tambahkan Controller, Swagger, dan Authorization
// ===============================================
builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new()
  {
    Title = "Soup API",
    Version = "v1",
    Description = "API documentation"
  });

  // Add JWT Authentication to Swagger
  c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
  {
    Description = @"JWT Authorization header using the Bearer scheme. 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      Example: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'",
    Name = "Authorization",
    In = ParameterLocation.Header,
    Type = SecuritySchemeType.ApiKey,
    Scheme = "Bearer"
  });

  c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
      {
        new OpenApiSecurityScheme
        {
          Reference = new OpenApiReference
          {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
          },
          Scheme = "oauth2",
          Name = "Bearer",
          In = ParameterLocation.Header,
        },
        new List<string>()
      }
    });
});

// ===============================================
// 5️⃣ Build App
// ===============================================
var app = builder.Build();

// ===============================================
// 6️⃣ Automatic Migration & Seeding
// ===============================================
using (var scope = app.Services.CreateScope())
{
  var services = scope.ServiceProvider;
  var context = services.GetRequiredService<ApplicationDbContext>();
  context.Database.Migrate();

  await SeedData.InitializeAsync(services);
}

// ===============================================
// 7️⃣ Middleware Pipeline
// ===============================================
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
  app.MapOpenApi();
}

app.UseRouting();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.MapControllers();

await SeedData.InitializeAsync(app.Services);

app.Run();
