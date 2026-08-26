namespace Padel.UI.Membre
{
    partial class frmConnexion
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
        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            label1 = new Label();
            txtMatricule = new TextBox();
            btnConnexion = new Button();
            SuspendLayout();
            //
            // label1
            //
            label1.AutoSize = true;
            label1.Location = new Point(69, 51);
            label1.Name = "label1";
            label1.Size = new Size(57, 15);
            label1.TabIndex = 0;
            label1.Text = "Matricule";
            //
            // txtMatricule
            //
            txtMatricule.Location = new Point(69, 69);
            txtMatricule.Name = "txtMatricule";
            txtMatricule.Size = new Size(150, 23);
            txtMatricule.TabIndex = 1;
            //
            // btnConnexion
            //
            btnConnexion.Location = new Point(69, 98);
            btnConnexion.Name = "btnConnexion";
            btnConnexion.Size = new Size(100, 23);
            btnConnexion.TabIndex = 2;
            btnConnexion.Text = "Se connecter";
            btnConnexion.UseVisualStyleBackColor = true;
            btnConnexion.Click += btnConnexion_Click;
            //
            // frmConnexion
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(400, 200);
            Controls.Add(btnConnexion);
            Controls.Add(txtMatricule);
            Controls.Add(label1);
            Name = "frmConnexion";
            Text = "Padel Manager — Connexion";
            ResumeLayout(false);
            PerformLayout();
        }
        #endregion
        private Label label1;
        private TextBox txtMatricule;
        private Button btnConnexion;
    }
}