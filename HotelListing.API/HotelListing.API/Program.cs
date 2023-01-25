using HotelListing.API.Configurations;
using HotelListing.API.Contracts;
using HotelListing.API.Data;
using HotelListing.API.Middleware;
using HotelListing.API.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
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

// Allow Versioning
builder.Services.AddApiVersioning(options =>
{
    // Setting a default version 
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new QueryStringApiVersionReader("api-version"),
        new HeaderApiVersionReader("X-Version"),
        new MediaTypeApiVersionReader("ver")
    );
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

builder.Services.AddResponseCaching(options =>
{
    // Allowing up to 1mb of cached data at any point
    options.MaximumBodySize = 1024;
    options.UseCaseSensitivePaths= true;
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

// Enabling Response caching middleware
app.UseResponseCaching();

// Middleware code for response cashing
app.Use(async (context, next) =>
{
    // This is letting the user know that the data is coming from the cache rather than fresh data
    // This is specifying that in the headers
    context.Response.GetTypedHeaders().CacheControl =
        new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
        {
            // This is saying that this is a public cache of data
            Public = true,
            // Keeping the cache alive for 10 seconds. After that fresh data would be available
            MaxAge = TimeSpan.FromSeconds(10)
        };
    // Cache response may vary for the type of the data you can expect
    context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] =
        new string[] { "Accept-Encoding" };

    // Carry out the next operation
    await next();
});


app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
