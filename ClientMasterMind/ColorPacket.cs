using System;
using System.Drawing;

namespace ClientMasterMind {
    public class ColorPacket {
        // Marc-André Patry
        // 1726476

        private Color[] couleurs { get; set; } // Les 4 couleurs regroupées dans un Array pour les parcourirs
        private int currentRow { get; set; } // Pour que CLient.cs connaisse la rangée où envoyer le feedback
        private bool winner; // Pour que le client sache si le joueur à gagné
        private double argent; // Pour spécifier l'argent accumulé

        public ColorPacket() {
            // C'est le seul constructeur utilisé
            couleurs = new Color[4];
        }

        public ColorPacket(Color couleur) {
            // Ce constructeur n'est pas utilisé. Il est là seulement par convention
            couleurs = new Color[4];

            for (int i = 0; i < 4; i++)
                couleurs[i] = couleur;
        }

        public ColorPacket(Color couleur1, Color couleur2, Color couleur3, Color couleur4) {
            // Ce constructeur n'est pas utilisé. Il est là seulement par convention
            couleurs = new Color[4];
            couleurs[0] = couleur1;
            couleurs[1] = couleur2;
            couleurs[2] = couleur3;
            couleurs[3] = couleur4;
        }

        public Color[] Couleurs {
            get { return couleurs; }
            set { couleurs = value; }
        }

        public int CurrentRow {
            get { return currentRow; }
            set { currentRow = value; }
        }

        public bool Winner {
            get { return winner; }
            set { winner = value; }
        }

        public double Argent {
            get { return argent; }
            set { argent = value; }
        }
    }
}
