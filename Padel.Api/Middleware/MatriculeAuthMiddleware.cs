using Padel.Application.Interfaces;

namespace Padel.Api.Middleware;

public class MatriculeAuthMiddleware
{
    private readonly RequestDelegate _next;

    public MatriculeAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IMembreService membreService,
        IAdminService adminService)
    {
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("X-Matricule", out var matriculeValues)
            || string.IsNullOrWhiteSpace(matriculeValues.FirstOrDefault()))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { erreur = "Header X-Matricule manquant." });
            return;
        }

        var matricule = matriculeValues.First()!;

        // CF-RS-037 : le préfixe du matricule détermine directement quelle table interroger,
        // évitant toute ambiguïté entre Membre et Administrateur.
        if (matricule.StartsWith('A'))
        {
            var admin = await adminService.ObtenirAdministrateurAsync(matricule);
            if (admin is not null)
            {
                context.Items["Identite"] = new IdentiteAppelant
                {
                    Matricule = admin.Matricule,
                    TypeRole = $"Admin{admin.Type}",
                    SiteId = admin.SiteId
                };
                await _next(context);
                return;
            }
        }
        else
        {
            var membre = await membreService.ObtenirMembreAsync(matricule);
            if (membre is not null)
            {
                context.Items["Identite"] = new IdentiteAppelant
                {
                    Matricule = membre.Matricule,
                    TypeRole = $"Membre{membre.Type}",
                    SiteId = membre.SiteId
                };
                await _next(context);
                return;
            }
        }

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsJsonAsync(new { erreur = "Matricule inconnu." });
    }
}