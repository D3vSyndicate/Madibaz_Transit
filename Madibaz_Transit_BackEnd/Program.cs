using Madibaz_Transit_BackEnd.Data;
using Madibaz_Transit_BackEnd.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// ---- UPDATED: this version tells Swagger about JWT bearer auth,
// which is what makes the "Authorize" padlock button actually appear.
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Paste ONLY the raw token here — Swagger adds 'Bearer ' automatically."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Register AppDbContext so it can be injected into any controller's
// constructor (AuthController, ShuttleManagerController, etc.)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register JwtTokenService so AuthController can use it
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddHostedService<BookingExpiryService>();

// Configure how the app validates the JWTs it issues itself. This is
// what makes [Authorize] and [Authorize(Roles = "...")] actually work
// on every controller, not just AuthController.
var jwtSigningKey = builder.Configuration["AppJwt:SigningKey"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["AppJwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["AppJwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Seed the 6 test accounts on startup, if they don't exist yet
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DbSeeder.SeedUsers(db);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// UseAuthentication MUST come before UseAuthorization — authentication
// figures out WHO is making the request (reads the JWT), authorization
// then decides WHAT they're allowed to do with that identity. Wrong
// order = roles never get checked correctly.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();