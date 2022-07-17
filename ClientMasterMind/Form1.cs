using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientMasterMind {
    public partial class Form1: Form {
        // Marc-André Patry
        // 1726476

        private static Dictionary<Color, int> colorsID = new Dictionary<Color, int> {
            // Représentation int des couleurs
            { Color.Blue, 1 },
            { Color.Lime, 2 },
            { Color.Yellow, 3 },
            { Color.DarkOrange, 4 },
            { Color.Red, 5 },
            { Color.MediumOrchid, 6 },
            { Color.Cyan, 7 },
            { Color.White, 8 },
            { Color.Gray, 0 },
        };
        private int currentRow; // Rangé actuelle (de 1 à 10)
        private int guessPosition; // Position (de 1 à 5) -> 5 veut dire que la rangé est complétées
        private ColorPacket colorPacket; // Pour envoyer au serveur
        private Client client;
        private Settings settings; // Pour envoyer au serveur
        private int[] colorPacketsID; // Pour empêcher d'entrer une combinaison déjà entrée
        bool clientStarted; // Une fois le client connecté au serveur, ce n'est pas autorisé
                            // de changer l'adresse IP et le port

        public Form1() {
            InitializeComponent();
            // Donner les valeurs initiales
            currentRow = 1;
            guessPosition = 1;
            colorPacket = new ColorPacket();
            settings = new Settings();
            colorPacketsID = new int[10];
            clientStarted = false;
        }

        private void Form1_Load(object sender, EventArgs e) {
            // Initialement, il n'y a pas d'index sélectionné pour le ComboBox
            cbColorNumber.SelectedIndex = 0;
        }

        // Paramètres ==================================================================================================
        private void btnResetSettings_Click(object sender, EventArgs e) {
            chkDuplicates.Checked = true; // Les couleurs dupliquées dans le code sont autorisées par défaut
            chkBlanks.Checked = false; // Les espaces vides ne sont pas autorisés par défaut
            cbColorNumber.SelectedIndex = 0; // Le nombre de couleur par défaut est 6
            chkNormalFeedback.Checked = true; // Par défaut, la position des pions du feedback
                                              // ne correspond pas forcément à la position des couleurs
            txtAdresseIP.Text = "127.0.0.1"; // L'adresse IP du serveur
            txtPort.Text = "5000"; // Le port sur lequel le serveur écoute
        }

        private void btnChangeSettings_Click(object sender, EventArgs e) {
            ResetGame(false); // Réinitialiser le jeu sans envoyer les paramètres au serveur et
                              // sans activer les boutons de couleurs

            // Activer les controles des paramètres
            chkDuplicates.Enabled = true;
            chkBlanks.Enabled = true;
            chkNormalFeedback.Enabled = true;
            cbColorNumber.Enabled = true;

            btnResetSettings.Enabled = true;
            btnChangeSettings.Enabled = false;
            btnPlay.Enabled = true;

            // Désactivé les controles du jeu
            btnBleu.Enabled = false;
            btnVert.Enabled = false;
            btnJaune.Enabled = false;
            btnOrange.Enabled = false;
            btnRouge.Enabled = false;
            btnMauve.Enabled = false;
            btnCyan.Enabled = false;
            btnBlanc.Enabled = false;
            btnGris.Enabled = false;

            btnDelete1.Enabled = false;
            btnDeleteRow.Enabled = false;
            btnResetGame.Enabled = false;
        }

        private void btnPlay_Click(object sender, EventArgs e) {
            // Une fois le client connecté au serveur, ce n'est pas autorisé de changer l'adresse IP et le port
            if (!clientStarted) {
                IPAddress adresseIP; // Seulement utilisé pour 2e paramètres de la fonction IPAddress.TryParse

                // Valider l'adresse IP
                if (!IPAddress.TryParse(txtAdresseIP.Text, out adresseIP)) {
                    MessageBox.Show("L'adresse IP est invalide", "Erreur");
                    return;
                }

                clientStarted = true;
                client = new Client(
                    txtAdresseIP.Text,
                    Convert.ToInt32(txtPort.Text),
                    new byte[] { 200, 5, 78, 232, 9, 6, 0, 4 },
                    new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
                new Task(client.Run).Start();
                txtAdresseIP.Enabled = false;
                txtPort.Enabled = false;
            }

            // Mettre à jour les paramètres pour l'objet settings
            settings.Duplicates = chkDuplicates.Checked;
            settings.Blanks = chkBlanks.Checked;
            settings.ColorNumber = Convert.ToInt32(cbColorNumber.SelectedItem);
            settings.NormalFeedback = chkNormalFeedback.Checked;

            client.Envoyer(settings); // Envoyer les paramètres au serveur

            // Désactivé les controles des paramètres
            chkDuplicates.Enabled = false;
            chkBlanks.Enabled = false;
            chkNormalFeedback.Enabled = false;
            cbColorNumber.Enabled = false;

            btnResetSettings.Enabled = false;
            btnChangeSettings.Enabled = true;
            btnPlay.Enabled = false;

            // Activé les bouton de couleurs (Ils sont désactivés quand le joueur gagne avant le dernier essaie)
            EnableColors();

            // Activé les controles du jeu
            btnDelete1.Enabled = true;
            btnDeleteRow.Enabled = true;
            btnResetGame.Enabled = true;
        }

        // Actions =====================================================================================================
        private void btnValidate_Click(object sender, EventArgs e) {
            // Les couleurs du ColorPacket représenté par un entier
            int colorPacketID =
                colorsID[colorPacket.Couleurs[0]] * 1000 +
                colorsID[colorPacket.Couleurs[1]] * 100 +
                colorsID[colorPacket.Couleurs[2]] * 10 +
                colorsID[colorPacket.Couleurs[3]];

            // Logique pour ne pas entrer une combinaison déjà entrée
            bool colorPacketUnique = true;
            for (int i = 0; i < currentRow - 1; i++) {
                if (colorPacketsID[i] == colorPacketID) {
                    colorPacketUnique = false;
                    break;
                }
            }

            if (!colorPacketUnique) {
                MessageBox.Show("Vous avez déjà essayé cette combinaison!", "Erreur");
                btnDeleteRow.PerformClick();
                return;
            }

            colorPacketsID[currentRow - 1] = colorPacketID; // Ajouter le colorPacketID au Array de colorPacketID
                                                            // Pour ne pas entrer une combinaison déjà entrée
            colorPacket.CurrentRow = currentRow; // Spécifier la rangée actuelle à pour l'objet colorPacket

            // Le currentRow s'arrête à 10 et le guessPosition reste à 5
            if (currentRow < 10) {
                guessPosition = 1;
                currentRow++;
            }
            else {
                // Empêcher d'effacer quand toutes les rangées sont complétées
                btnDelete1.Enabled = false;
                btnDeleteRow.Enabled = false;
            }

            btnValidate.Enabled = false; // Désactiver le bouton valider
            client.Envoyer(colorPacket); // Envoyer le colorPacket au serveur
        }

        private void btnDelete1_Click(object sender, EventArgs e) {
            // Effacer 1 carré
            if (guessPosition > 1) {
                guessPosition--;
                ChangerCouleur(Color.LightGray);
                btnValidate.Enabled = false;
            }
        }

        private void btnDeleteRow_Click(object sender, EventArgs e) {
            // Effacer la rangée
            while (guessPosition > 1) {
                guessPosition--;
                ChangerCouleur(Color.LightGray);
            }

            btnValidate.Enabled = false;
        }

        private void ResetGame(bool sendSettings) {
            /**
             * Réinitialiser le jeu
             * @sendSettings: détermine si la fonction doit envoyer les paramètres au serveur et activer les couleurs
             */
            if (sendSettings) {
                client.Envoyer(settings);

                // Activé les bouton de couleurs (Ils sont désactivés quand le joueur gagne avant le dernier essaie)
                EnableColors();
            }

            // Mettre les carrés de la solution de couleur noir
            for (int i = 1; i < 5; i++)
                Application.OpenForms["Form1"].Controls[$"btnSolution{i}"].BackColor = Color.Black;

            while (currentRow > 0) {
                // Réinitialise la rangée actuelle
                while (guessPosition > 1) {
                    guessPosition--;
                    ChangerCouleur(Color.LightGray);
                }

                // Réinitialise le feedback de la rangée actuelle
                for (int i = 1; i < 5; i++)
                    Application.OpenForms["Form1"].Controls[$"btnR{currentRow}F{i}"].BackColor = Color.LightGray;

                // Remettre le guessPosition égal à 5 et décrémenter currentRow pour réinitialiser la prochaine rangée
                guessPosition = 5;
                currentRow--;
            }

            // Remettre guessPosition et currentRow à leur valeur d'origine
            guessPosition = 1;
            currentRow = 1;

            // S'occuper de désactiver le bouton valider et d'activé les boutons pour effacer
            btnValidate.Enabled = false;
            btnDelete1.Enabled = true;
            btnDeleteRow.Enabled = true;

            // Remettre l'argent à 0.00$
            lblArgent.Text = "Argent gagné: 0.00$";
        }

        private void btnResetGame_Click(object sender, EventArgs e) {
            // Réinitialiser le jeu et envoyers les paramètres au serveur et activer les boutons de couleurs
            ResetGame(true);
        }

        private void EnableColors() {
            // Activé les bouton de couleurs (Ils sont désactivés quand le joueur gagne avant le dernier essaie)
            btnBleu.Enabled = true;
            btnVert.Enabled = true;
            btnJaune.Enabled = true;
            btnOrange.Enabled = true;
            btnRouge.Enabled = true;
            btnMauve.Enabled = true;

            switch ((string)cbColorNumber.SelectedItem) {
            case "7":
                btnCyan.Enabled = true; break;
            case "8":
                btnCyan.Enabled = true;
                btnBlanc.Enabled = true; break;
            }

            if (chkBlanks.Checked == true)
                btnGris.Enabled = true;
        }

        private void ChangerCouleur(Color color) {
            // Changer la couleur d'un des boutons dans l'une des rangées
            Application.OpenForms["Form1"].Controls[$"btnR{currentRow}G{guessPosition}"]
                .BackColor = color;
        }

        private void AjouterCouleur(Color color) {
            if (guessPosition < 5) { // On ne peut pas ajouter une couleur si la rangée est déjà complétée
                ChangerCouleur(color);
                colorPacket.Couleurs[guessPosition - 1] = color; // S'occuper de gérer le ColorPacket
                guessPosition++;

                if (guessPosition == 5) // On peut seulement valider la rangée lorsqu'elle est complétée
                    btnValidate.Enabled = true;
            }
        }

        private void btnBleu_Click(object sender, EventArgs e) {
            AjouterCouleur(Color.Blue);
        }

        private void btnVert_Click(object sender, EventArgs e) {
            AjouterCouleur(Color.Lime);
        }

        private void btnJaune_Click(object sender, EventArgs e) {
            AjouterCouleur(Color.Yellow);
        }

        private void btnOrange_Click(object sender, EventArgs e) {
            AjouterCouleur(Color.DarkOrange);
        }

        private void btnRouge_Click(object sender, EventArgs e) {
            AjouterCouleur(Color.Red);
        }

        private void btnMauve_Click(object sender, EventArgs e) {
            AjouterCouleur(Color.MediumOrchid);
        }

        private void btnCyan_Click(object sender, EventArgs e) {
            AjouterCouleur(Color.Cyan);
        }

        private void btnBlanc_Click(object sender, EventArgs e) {
            AjouterCouleur(Color.White);
        }

        private void btnGris_Click(object sender, EventArgs e) {
            AjouterCouleur(Color.Gray);
        }
    }
}
