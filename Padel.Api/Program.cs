using Microsoft.EntityFrameworkCore;
using Padel.Application.Interfaces;
using Padel.Application.Services;
using Padel.Infrastructure;
using Padel.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ===================== Services (DI) =====================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Padel Manager API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Matricule", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "X-Matricule",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Matricule du membre ou administrateur appelant (identification sans mot de passe, cf. CF-AA-005)"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Matricule"
                }
            },
            new string[] { }
        }
    });
});

// DbContext : connecté via UserPadelApi, qui ne peut appeler que des procédures/vues (option B)
builder.Services.AddDbContext<PadelDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PadelDb")));

// Repositories
builder.Services.AddScoped<ISiteRepository, SiteRepository>();
builder.Services.AddScoped<ITerrainRepository, TerrainRepository>();
builder.Services.AddScoped<IMembreRepository, MembreRepository>();
builder.Services.AddScoped<IAdministrateurRepository, AdministrateurRepository>();
builder.Services.AddScoped<IMatchRepository, MatchRepository>();
builder.Services.AddScoped<IPaiementRepository, PaiementRepository>();
builder.Services.AddScoped<IStatistiqueRepository, StatistiqueRepository>();

// Services (Application)
builder.Services.AddScoped<ISiteService, SiteService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IPaiementService, PaiementService>();
builder.Services.AddScoped<IMembreService, MembreService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IStatistiqueService, StatistiqueService>();

var app = builder.Build();

// ===================== Pipeline (Middleware) =====================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<Padel.Api.Middleware.ErrorHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseMiddleware<Padel.Api.Middleware.MatriculeAuthMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }