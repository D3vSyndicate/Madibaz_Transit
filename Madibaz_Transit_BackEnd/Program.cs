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
builder.Services.AddSwaggerGen();

// ---- ADDED: register AppDbContext so it can be injected into any
// controller's constructor (AuthController, ShuttleManagerController, etc.)
// Without this line, the app has no way to create AppDbContext at runtime —
// AppDbContextFactory is only used by 'dotnet ef' commands, not by the
// running app itself.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---- ADDED: register JwtTokenService so AuthController can use it
builder.Services.AddScoped<JwtTokenService>();

// ---- ADDED: configure how the app validates the JWTs it issues itself.
// This is what makes [Authorize] and [Authorize(Roles = "...")] actually
// work on every controller, not just AuthController.
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

// ---- ADDED: seed the 6 test accounts on startup, if they don't exist yet
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

// ---- ADDED: UseAuthentication MUST come before UseAuthorization —
// authentication figures out WHO is making the request (reads the JWT),
// authorization then decides WHAT they're allowed to do with that identity.
// Wrong order = roles never get checked correctly.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();