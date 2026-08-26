namespace Padel.UI.Membre
{
    partial class frmAccueil
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            tabAccueil = new TabControl();
            tabReserver = new TabPage();
            cboSiteReservation = new ComboBox();
            dtpDateReservation = new DateTimePicker();
            btnRechercherCreneaux = new Button();
            lstCreneauxDisponibles = new ListBox();
            radioMatchPrive = new RadioButton();
            radioMatchPublic = new RadioButton();
            btnReserver = new Button();
            tabMatchsPublics = new TabPage();
            lstMatchsPublics = new ListBox();
            btnRafraichirMatchsPublics = new Button();
            btnSInscrire = new Button();
            tabMesReservations = new TabPage();
            tabMonProfil = new TabPage();
            tabAccueil.SuspendLayout();
            tabReserver.SuspendLayout();
            tabMatchsPublics.SuspendLayout();
            SuspendLayout();
            //
            // tabAccueil
            //
            tabAccueil.Controls.Add(tabReserver);
            tabAccueil.Controls.Add(tabMatchsPublics);
            tabAccueil.Controls.Add(tabMesReservations);
            tabAccueil.Controls.Add(tabMonProfil);
            tabAccueil.Dock = DockStyle.Fill;
            tabAccueil.Location = new Point(0, 0);
            tabAccueil.Name = "tabAccueil";
            tabAccueil.SelectedIndex = 0;
            tabAccueil.Size = new Size(800, 450);
            tabAccueil.TabIndex = 0;
            //
            // tabReserver
            //
            tabReserver.Controls.Add(cboSiteReservation);
            tabReserver.Controls.Add(dtpDateReservation);
            tabReserver.Controls.Add(btnRechercherCreneaux);
            tabReserver.Controls.Add(lstCreneauxDisponibles);
            tabReserver.Controls.Add(radioMatchPrive);
            tabReserver.Controls.Add(radioMatchPublic);
            tabReserver.Controls.Add(btnReserver);
            tabReserver.Location = new Point(4, 24);
            tabReserver.Name = "tabReserver";
            tabReserver.Padding = new Padding(3);
            tabReserver.Size = new Size(792, 422);
            tabReserver.TabIndex = 0;
            tabReserver.Text = "Réserver un match";
            tabReserver.UseVisualStyleBackColor = true;
            //
            // cboSiteReservation
            //
            cboSiteReservation.FormattingEnabled = true;
            cboSiteReservation.Location = new Point(20, 20);
            cboSiteReservation.Name = "cboSiteReservation";
            cboSiteReservation.Size = new Size(200, 23);
            cboSiteReservation.TabIndex = 0;
            //
            // dtpDateReservation
            //
            dtpDateReservation.Location = new Point(240, 20);
            dtpDateReservation.Name = "dtpDateReservation";
            dtpDateReservation.Size = new Size(200, 23);
            dtpDateReservation.TabIndex = 1;
            //
            // btnRechercherCreneaux
            //
            btnRechercherCreneaux.Location = new Point(460, 20);
            btnRechercherCreneaux.Name = "btnRechercherCreneaux";
            btnRechercherCreneaux.Size = new Size(150, 23);
            btnRechercherCreneaux.TabIndex = 2;
            btnRechercherCreneaux.Text = "Rechercher créneaux";
            btnRechercherCreneaux.UseVisualStyleBackColor = true;
            btnRechercherCreneaux.Click += btnRechercherCreneaux_Click;
            //
            // lstCreneauxDisponibles
            //
            lstCreneauxDisponibles.FormattingEnabled = true;
            lstCreneauxDisponibles.Location = new Point(20, 60);
            lstCreneauxDisponibles.Name = "lstCreneauxDisponibles";
            lstCreneauxDisponibles.Size = new Size(400, 244);
            lstCreneauxDisponibles.TabIndex = 3;
            //
            // radioMatchPrive
            //
            radioMatchPrive.AutoSize = true;
            radioMatchPrive.Checked = true;
            radioMatchPrive.Location = new Point(460, 60);
            radioMatchPrive.Name = "radioMatchPrive";
            radioMatchPrive.Size = new Size(90, 19);
            radioMatchPrive.TabIndex = 4;
            radioMatchPrive.TabStop = true;
            radioMatchPrive.Text = "Match privé";
            radioMatchPrive.UseVisualStyleBackColor = true;
            //
            // radioMatchPublic
            //
            radioMatchPublic.AutoSize = true;
            radioMatchPublic.Location = new Point(460, 90);
            radioMatchPublic.Name = "radioMatchPublic";
            radioMatchPublic.Size = new Size(95, 19);
            radioMatchPublic.TabIndex = 5;
            radioMatchPublic.Text = "Match public";
            radioMatchPublic.UseVisualStyleBackColor = true;
            //
            // btnReserver
            //
            btnReserver.Location = new Point(460, 130);
            btnReserver.Name = "btnReserver";
            btnReserver.Size = new Size(150, 30);
            btnReserver.TabIndex = 6;
            btnReserver.Text = "Réserver";
            btnReserver.UseVisualStyleBackColor = true;
            btnReserver.Click += btnReserver_Click;
            //
            // tabMatchsPublics
            //
            tabMatchsPublics.Controls.Add(lstMatchsPublics);
            tabMatchsPublics.Controls.Add(btnRafraichirMatchsPublics);
            tabMatchsPublics.Controls.Add(btnSInscrire);
            tabMatchsPublics.Location = new Point(4, 24);
            tabMatchsPublics.Name = "tabMatchsPublics";
            tabMatchsPublics.Padding = new Padding(3);
            tabMatchsPublics.Size = new Size(792, 422);
            tabMatchsPublics.TabIndex = 1;
            tabMatchsPublics.Text = "Matches publics";
            tabMatchsPublics.UseVisualStyleBackColor = true;
            //
            // lstMatchsPublics
            //
            lstMatchsPublics.FormattingEnabled = true;
            lstMatchsPublics.Location = new Point(20, 20);
            lstMatchsPublics.Name = "lstMatchsPublics";
            lstMatchsPublics.Size = new Size(500, 300);
            lstMatchsPublics.TabIndex = 0;
            //
            // btnRafraichirMatchsPublics
            //
            btnRafraichirMatchsPublics.Location = new Point(540, 20);
            btnRafraichirMatchsPublics.Name = "btnRafraichirMatchsPublics";
            btnRafraichirMatchsPublics.Size = new Size(150, 30);
            btnRafraichirMatchsPublics.TabIndex = 1;
            btnRafraichirMatchsPublics.Text = "Rafraîchir";
            btnRafraichirMatchsPublics.UseVisualStyleBackColor = true;
            btnRafraichirMatchsPublics.Click += btnRafraichirMatchsPublics_Click;
            //
            // btnSInscrire
            //
            btnSInscrire.Location = new Point(540, 60);
            btnSInscrire.Name = "btnSInscrire";
            btnSInscrire.Size = new Size(150, 30);
            btnSInscrire.TabIndex = 2;
            btnSInscrire.Text = "S'inscrire";
            btnSInscrire.UseVisualStyleBackColor = true;
            btnSInscrire.Click += btnSInscrire_Click;
            //
            // tabMesReservations
            //
            tabMesReservations.Location = new Point(4, 24);
            tabMesReservations.Name = "tabMesReservations";
            tabMesReservations.Padding = new Padding(3);
            tabMesReservations.Size = new Size(792, 422);
            tabMesReservations.TabIndex = 2;
            tabMesReservations.Text = "Mes réservations";
            tabMesReservations.UseVisualStyleBackColor = true;
            //
            // tabMonProfil
            //
            tabMonProfil.Location = new Point(4, 24);
            tabMonProfil.Name = "tabMonProfil";
            tabMonProfil.Padding = new Padding(3);
            tabMonProfil.Size = new Size(792, 422);
            tabMonProfil.TabIndex = 3;
            tabMonProfil.Text = "Mon profil";
            tabMonProfil.UseVisualStyleBackColor = true;
            //
            // frmAccueil
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(tabAccueil);
            Name = "frmAccueil";
            Text = "Padel Manager";
            Load += frmAccueil_Load;
            tabAccueil.ResumeLayout(false);
            tabReserver.ResumeLayout(false);
            tabReserver.PerformLayout();
            tabMatchsPublics.ResumeLayout(false);
            ResumeLayout(false);
        }

        private TabControl tabAccueil;
        private TabPage tabReserver;
        private ComboBox cboSiteReservation;
        private DateTimePicker dtpDateReservation;
        private Button btnRechercherCreneaux;
        private ListBox lstCreneauxDisponibles;
        private RadioButton radioMatchPrive;
        private RadioButton radioMatchPublic;
        private Button btnReserver;
        private TabPage tabMatchsPublics;
        private ListBox lstMatchsPublics;
        private Button btnRafraichirMatchsPublics;
        private Button btnSInscrire;
        private TabPage tabMesReservations;
        private TabPage tabMonProfil;
    }
}