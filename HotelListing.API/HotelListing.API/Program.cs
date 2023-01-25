using HotelListing.API.Configurations;
using HotelListing.API.Contracts;
using HotelListing.API.Data;
using HotelListing.API.Middleware;
using HotelListing.API.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("HotelListingDbConnectioNString");
builder.Services.AddDbContext<HotelListingDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

// Adding Identity Core
// When adding identity core we need to add it relative to a user type
builder.Services.AddIdentityCore<ApiUser>()
    // Role represents what the user can do. (Who are you and what can you do)
    .AddRoles<IdentityRole>()
    // Token Provider
    .AddTokenProvider<DataProtectorTokenProvider<ApiUser>>("HotelListingApi")
    // Letting IdentityCore know which data store it should use, we can have a seperate db context for just user authentication
    .AddEntityFrameworkStores<HotelListingDbContext>()
    .AddDefaultTokenProviders();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Adding CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", b => b.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());
});


// Serilog Config
// context is a instance of the builder
// When we are running the app we can access the console for logs
builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.WriteTo.Console().ReadFrom.Configuration(context.Configuration));

// Adding AutoMapper service and specifying the config class
builder.Services.AddAutoMapper(typeof(AutomapperConfig));

// Dependencies we will inject
builder.Services.AddScoped(typeof(IGenericRepository<>),typeof(GenericRepository<>));
builder.Services.AddScoped<ICountriesRepository, CountriesRepository>();
builder.Services.AddScoped<IHotelsRepository, HotelsRepository>();
builder.Services.AddScoped<IAuthManager, AuthManager>();

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    // "Bearer"
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        // Validate the key came from us
        ValidateIssuer = true,
        // Validate the user sending it
        ValidateAudience = true,
        // Validate token life time
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        // These are values we want to add into out configuration
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        // Generate a byte representation of the key
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]))
    };
});

// TODO: Swagger Implementation

var app = builder.Build();

// Configure the HTTP request pipeline.
// These are done in the order below

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// TODO: Swagger Implementation

// Adding Exception Middleware
app.UseMiddleware<ExceptionMiddleware>();


// This will start logging the requests coming in and the time it takes to complete
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

// Using CORS Policy
app.UseCors("AllowAll");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
