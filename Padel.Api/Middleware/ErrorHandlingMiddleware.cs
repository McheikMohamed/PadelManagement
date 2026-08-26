using System.Net;
using Padel.Application.Exceptions;

namespace Padel.Api.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (RegleMetierException ex)
        {
            _logger.LogWarning(ex, "Règle métier violée : {Code}", ex.Code);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)MapperCodeVersStatutHttp(ex.Code);

            await context.Response.WriteAsJsonAsync(new
            {
                code = ex.Code,
                message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Argument invalide");

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            await context.Response.WriteAsJsonAsync(new
            {
                code = "ARGUMENT_INVALIDE",
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur non gérée");

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            await context.Response.WriteAsJsonAsync(new
            {
                code = "ERREUR_INTERNE",
                message = "Une erreur inattendue est survenue."
            });
        }
    }

    private static HttpStatusCode MapperCodeVersStatutHttp(string code) => code switch
    {
        "SOLDE_DU" => HttpStatusCode.Forbidden,
        "PENALITE_ACTIVE" => HttpStatusCode.Forbidden,
        "PERIMETRE_SITE" => HttpStatusCode.Forbidden,
        "ACTION_RESERVEE_GLOBAL" => HttpStatusCode.Forbidden,
        "INSCRIPTION_NON_AUTORISEE" => HttpStatusCode.Forbidden,
        "PAIEMENT_NON_AUTORISE" => HttpStatusCode.Forbidden,
        "MEMBRE_INCONNU" => HttpStatusCode.NotFound,
        "TERRAIN_INCONNU" => HttpStatusCode.NotFound,
        "MATCH_INCONNU" => HttpStatusCode.NotFound,
        "APPELANT_INCONNU" => HttpStatusCode.NotFound,
        "INSCRIPTION_INCONNUE" => HttpStatusCode.NotFound,
        "ACTION_RESERVEE_ORGANISATEUR" => HttpStatusCode.Forbidden,
        "DESINSCRIPTION_NON_AUTORISEE" => HttpStatusCode.Forbidden,
        _ => HttpStatusCode.BadRequest
    };
}