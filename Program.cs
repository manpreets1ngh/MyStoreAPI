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
builder.Services.AddDbContext<MyStoreAPIContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyStoreAPIContext") ?? throw new InvalidOperationException("Connection string 'MyStoreAPIContext' not found.")));

builder.Services.AddDbContext<MyStoreAPIIdentityContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyStoreAPIIdentityContextConnection") ?? throw new InvalidOperationException("Connection string 'MyStoreAPIContext' not found.")));
builder.Services.Configure<SquareSettings>(builder.Configuration.GetSection("SquareSettings"));
builder.Services.AddSingleton<SquareSettings>(sp =>
    sp.GetRequiredService<IOptions<SquareSettings>>().Value);


builder.Services.AddIdentity<MyStoreAPIUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<MyStoreAPIIdentityContext>();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

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
    options.AddPolicy("AllowClientOrigin", builder =>
    {
        builder.AllowAnyOrigin() // Replace with your client application's URL
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Add HTTPS redirection
builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
    options.HttpsPort = 5001; // Ensure this matches the HTTPS port
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
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidAudience = builder.Configuration["JWT:ValidAudience"],
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
        ClockSkew = TimeSpan.Zero,
        IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(builder.Configuration["JWT:Secret"]))
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

app.UseHttpsRedirection();
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
