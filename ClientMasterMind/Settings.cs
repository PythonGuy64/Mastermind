using System;

namespace ClientMasterMind {
    public class Settings {
        // Marc-André Patry
        // 1726476

        private bool duplicates; // Détermine si les couleurs dupliquées dans le code secret sont autorisées
                                 // (On peut toujours essayer de deviner en mettant des couleurs dupliquées)
        private bool blanks; // Détermine si les espaces vides sont autorisés
                             // (Les espaces vides ne peuvent pas recevoir de pion blanc
        private int colorNumber; // Le nombre de couleur (De 6 à 8)
        private bool normalFeedback; // Le type de feedback

        public Settings() {
        }
        
        public Settings(bool aDuplicates, bool aBlanks, int aColorNumber, bool aNormalFeedback) {
            duplicates = aDuplicates;
            blanks = aBlanks;
            colorNumber = aColorNumber;
            normalFeedback = aNormalFeedback;
        }

        public bool Duplicates {
            get { return duplicates; }
            set { duplicates = value; }
        }

        public bool Blanks {
            get { return blanks; }
            set { blanks = value; }
        }

        public int ColorNumber {
            get { return colorNumber; }
            set { colorNumber = value; }
        }

        public bool NormalFeedback {
            get { return normalFeedback; }
            set { normalFeedback = value; }
        }
    }
}
