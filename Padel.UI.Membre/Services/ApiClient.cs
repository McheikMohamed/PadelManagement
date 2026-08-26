using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Padel.UI.Membre.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    public string MatriculeConnecte { get; }

    public ApiClient(string baseUrl, string matricule)
    {
        MatriculeConnecte = matricule;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };
        _httpClient.DefaultRequestHeaders.Add("X-Matricule", matricule);
    }

    public async Task<T?> GetAsync<T>(string chemin)
    {
        var reponse = await _httpClient.GetAsync(chemin);
        await VerifierErreurAsync(reponse);
        return await reponse.Content.ReadFromJsonAsync<T>();
    }

    public async Task<TResultat?> PostAsync<TCorps, TResultat>(string chemin, TCorps corps)
    {
        var reponse = await _httpClient.PostAsJsonAsync(chemin, corps);
        await VerifierErreurAsync(reponse);
        return await reponse.Content.ReadFromJsonAsync<TResultat>();
    }

    public async Task PostSansRetourAsync(string chemin)
    {
        var reponse = await _httpClient.PostAsync(chemin, null);
        await VerifierErreurAsync(reponse);
    }

    public async Task DeleteAsync(string chemin)
    {
        var reponse = await _httpClient.DeleteAsync(chemin);
        await VerifierErreurAsync(reponse);
    }

    private static async Task VerifierErreurAsync(HttpResponseMessage reponse)
    {
        if (reponse.IsSuccessStatusCode)
        {
            return;
        }

        string message;
        try
        {
            var contenu = await reponse.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(contenu);
            message = document.RootElement.TryGetProperty("message", out var prop)
                ? prop.GetString() ?? "Erreur inconnue."
                : contenu;
        }
        catch
        {
            message = $"Erreur HTTP {(int)reponse.StatusCode}.";
        }

        throw new ApiException((int)reponse.StatusCode, message);
    }
}

public class ApiException : Exception
{
    public int StatusCode { get; }

    public ApiException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }
}