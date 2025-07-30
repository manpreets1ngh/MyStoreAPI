using MyStoreAPI.Areas.Identity.Data;
using MyStoreAPI.Data;
using MyStoreAPI.Models;
using MyStoreAPI.Services;
using MyStoreAPI.Services.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MyStoreAPI.Static;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:8080");

builder.Services.AddDbContext<MyStoreAPIContext>(options =>
    options.UseNpgsql(Environment.GetEnvironmentVariable("CONNECTION_STRING_CONTEXT")));

builder.Services.AddDbContext<MyStoreAPIIdentityContext>(options =>
    options.UseNpgsql(Environment.GetEnvironmentVariable("CONNECTION_STRING_IDENTITY")));

builder.Services.Configure<SquareSettings>(options =>
{
    options.AccessToken = Environment.GetEnvironmentVariable("SQUARE_AccessToken");
    options.LocationId = Environment.GetEnvironmentVariable("SQUARE_LocationId");
});
builder.Services.AddSingleton<SquareSettings>(sp =>
    sp.GetRequiredService<IOptions<SquareSettings>>().Value);


builder.Services.AddIdentity<MyStoreAPIUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<MyStoreAPIIdentityContext>();

builder.Services.Configure<EmailSettings>(options =>
{
    options.SmtpHost = Environment.GetEnvironmentVariable("EMAIL_SmtpHost");
    options.SmtpPort = int.Parse(Environment.GetEnvironmentVariable("EMAIL_SmtpPort") ?? "25");
    options.SmtpUser = Environment.GetEnvironmentVariable("EMAIL_SmtpUser");
    options.SmtpPass = Environment.GetEnvironmentVariable("EMAIL_SmtpPass");
    options.FromEmail = Environment.GetEnvironmentVariable("EMAIL_FromEmail");
    options.FromName = Environment.GetEnvironmentVariable("EMAIL_FromName");
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IProductsService, ProductsService>();
builder.Services.AddScoped<IOrdersService, OrdersService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<IStatusService, StatusService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<EmailService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin() // Replace with your client application's URL
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


builder.Services.Configure<IdentityOptions>(options =>
{
    // Default Password settings.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
});

// Adding Authentication
// Add support for env + appsettings
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// Bind JWT section
var jwtSettings = new JwtSettings();
builder.Configuration.Bind("JWT", jwtSettings);

// Override with env vars if present
jwtSettings.ValidIssuer = Environment.GetEnvironmentVariable("JWT_ValidIssuer") ?? jwtSettings.ValidIssuer;
jwtSettings.ValidAudience = Environment.GetEnvironmentVariable("JWT_ValidAudience") ?? jwtSettings.ValidAudience;
jwtSettings.Secret = Environment.GetEnvironmentVariable("JWT_Secret") ?? jwtSettings.Secret;
jwtSettings.TokenExpiryTimeInHour = Environment.GetEnvironmentVariable("JWT_TokenExpiryTimeInHour") ?? jwtSettings.TokenExpiryTimeInHour;

// Register auth with injected values
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidAudience = jwtSettings.ValidAudience,
            ValidIssuer = jwtSettings.ValidIssuer,
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(jwtSettings.Secret))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UserPolicy", policy => policy.RequireRole("User"));
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{   
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    await next();

    if (context.Response.StatusCode == 401) // Unauthorized
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"Message\":\"You are not authorized to access this resource.\"}");
    }
    else if (context.Response.StatusCode == 403) // Forbidden
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"Message\":\"Access denied. Admin permissions required.\"}");
    }
});


app.MapControllers();

app.Run();
