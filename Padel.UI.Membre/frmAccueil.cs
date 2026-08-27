using Padel.UI.Membre.Models;
using Padel.UI.Membre.Services;

namespace Padel.UI.Membre;

public partial class frmAccueil : Form
{
    private readonly ApiClient _apiClient;
    private readonly MembreDto _membreConnecte;
    private List<SiteDto> _sitesDisponibles = new();

    public frmAccueil(ApiClient apiClient, MembreDto membreConnecte)
    {
        InitializeComponent();
        _apiClient = apiClient;
        _membreConnecte = membreConnecte;

        Text = $"Padel Manager — Connecté : {_membreConnecte.Matricule} ({_membreConnecte.Type})";
    }

    private async void frmAccueil_Load(object sender, EventArgs e)
    {
        await ChargerSitesAsync();
        await ChargerMatchsPublicsAsync();
    }

    private async Task ChargerSitesAsync()
    {
        try
        {
            _sitesDisponibles = await _apiClient.GetAsync<List<SiteDto>>("api/Sites") ?? new List<SiteDto>();

            var sitesAffiches = _membreConnecte.Type == "Site"
                ? _sitesDisponibles.Where(s => s.SiteId == _membreConnecte.SiteId).ToList()
                : _sitesDisponibles;

            cboSiteReservation.DataSource = sitesAffiches;
            cboSiteReservation.DisplayMember = nameof(SiteDto.Nom);
            cboSiteReservation.ValueMember = nameof(SiteDto.SiteId);
        }
        catch (ApiException ex)
        {
            MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnRechercherCreneaux_Click(object sender, EventArgs e)
    {
        if (cboSiteReservation.SelectedValue is not int siteId)
        {
            MessageBox.Show("Veuillez sélectionner un site.", "Champ requis",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var date = dtpDateReservation.Value.Date;

        try
        {
            var chemin = $"api/Reservations/creneaux-disponibles?siteId={siteId}&date={date:yyyy-MM-dd}";
            var creneaux = await _apiClient.GetAsync<List<CreneauDisponibleDto>>(chemin) ?? new();

            lstCreneauxDisponibles.DataSource = creneaux;
            lstCreneauxDisponibles.DisplayMember = nameof(CreneauDisponibleDto.Affichage);
        }
        catch (ApiException ex)
        {
            MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnReserver_Click(object sender, EventArgs e)
    {
        if (lstCreneauxDisponibles.SelectedItem is not CreneauDisponibleDto creneauSelectionne)
        {
            MessageBox.Show("Veuillez sélectionner un créneau.", "Champ requis",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var estPrive = radioMatchPrive.Checked;

        try
        {
            var dto = new CreerReservationDto
            {
                TerrainId = creneauSelectionne.TerrainId,
                DateHeureDebut = creneauSelectionne.DateHeureDebut,
                EstPrive = estPrive
            };

            var match = await _apiClient.PostAsync<CreerReservationDto, MatchDto>("api/Reservations", dto);

            MessageBox.Show(
                $"Réservation créée avec succès (Match #{match?.MatchId}, statut : {match?.Statut}).",
                "Réservation confirmée", MessageBoxButtons.OK, MessageBoxIcon.Information);

            btnRechercherCreneaux_Click(sender, e);
        }
        catch (ApiException ex)
        {
            MessageBox.Show(ex.Message, "Réservation impossible",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async void btnRafraichirMatchsPublics_Click(object sender, EventArgs e)
    {
        await ChargerMatchsPublicsAsync();
    }

    private async Task ChargerMatchsPublicsAsync()
    {
        try
        {
            int? siteId = _membreConnecte.Type == "Site" ? _membreConnecte.SiteId : null;
            var chemin = siteId.HasValue
                ? $"api/Reservations/matchs-publics?siteId={siteId}"
                : "api/Reservations/matchs-publics";

            var matchs = await _apiClient.GetAsync<List<MatchPublicDto>>(chemin) ?? new();

            lstMatchsPublics.DataSource = matchs;
            lstMatchsPublics.DisplayMember = nameof(MatchPublicDto.Affichage);
        }
        catch (ApiException ex)
        {
            MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnSInscrire_Click(object sender, EventArgs e)
    {
        if (lstMatchsPublics.SelectedItem is not MatchPublicDto matchSelectionne)
        {
            MessageBox.Show("Veuillez sélectionner un match.", "Champ requis",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            var dto = new InscrireJoueurRequestDto { MembreMatricule = _membreConnecte.Matricule };

            var inscription = await _apiClient.PostAsync<InscrireJoueurRequestDto, InscriptionDto>(
                $"api/Reservations/{matchSelectionne.MatchId}/inscriptions", dto);

            MessageBox.Show(
                $"Inscription réussie (Inscription #{inscription?.InscriptionId}). N'oubliez pas de payer avant la veille du match.",
                "Inscription confirmée", MessageBoxButtons.OK, MessageBoxIcon.Information);

            await ChargerMatchsPublicsAsync();
        }
        catch (ApiException ex)
        {
            MessageBox.Show(ex.Message, "Inscription impossible",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}