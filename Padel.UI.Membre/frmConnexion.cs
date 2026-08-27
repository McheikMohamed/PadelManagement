using Padel.UI.Membre.Models;
using Padel.UI.Membre.Services;

namespace Padel.UI.Membre;

public partial class frmConnexion : Form
{
    private const string ApiBaseUrl = "http://localhost:5051/";

    public frmConnexion()
    {
        InitializeComponent();
    }

    private async void btnConnexion_Click(object sender, EventArgs e)
    {
        var matricule = txtMatricule.Text.Trim();

        if (string.IsNullOrWhiteSpace(matricule))
        {
            MessageBox.Show("Veuillez saisir votre matricule.", "Champ requis",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        btnConnexion.Enabled = false;

        try
        {
            var apiClient = new ApiClient(ApiBaseUrl, matricule);

            var membre = await apiClient.GetAsync<MembreDto>($"api/Membres/{matricule}");

            if (membre is null)
            {
                MessageBox.Show("Matricule inconnu.", "Erreur de connexion",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var accueil = new frmAccueil(apiClient, membre);
            accueil.Show();
            Hide();
        }
        catch (ApiException ex)
        {
            MessageBox.Show(ex.Message, "Erreur de connexion",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (HttpRequestException)
        {
            MessageBox.Show(
                "Impossible de contacter le serveur. Vérifiez que l'Api est démarrée.",
                "Erreur réseau", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnConnexion.Enabled = true;
        }
    }
}