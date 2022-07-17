using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;
using System.Text;

namespace ServeurEnfantMasterMind {
    public class ServeurEnfant {
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
        private TcpClient socketCommunication;
        private BufferedStream reader;
        private BufferedStream writer;
        private Random random; // Pour choisir des entiers aléatoires
        private byte[] key; // Pour l'encryption
        private byte[] iv; // Pour l'encryption
        private DESCryptoServiceProvider desProvider; // Pour l'encryption
        private SqlManager_ sqlManager_; // Pour gérer les insertions dans la bases de données

        public ServeurEnfant(TcpClient socketComm, byte[] aKey, byte[] aIv) {
            socketCommunication = socketComm;
            reader = new BufferedStream(socketComm.GetStream());
            writer = new BufferedStream(socketComm.GetStream());
            key = aKey;
            iv = aIv;
            desProvider = new DESCryptoServiceProvider();
            random = new Random();
            sqlManager_ = new SqlManager_();
        }

        public void Run() {
            try {
                Settings settings = new Settings();
                ColorPacket solution = new ColorPacket();
                ColorPacket guess = new ColorPacket();
                ColorPacket feedBack = new ColorPacket();
                double dollarParPoint = 10;
                bool gotInitialSettings = false;

                while (true) {
                    List<Color> couleurs = new List<Color> {
                        Color.Blue, Color.Lime, Color.Yellow, Color.DarkOrange, Color.Red, Color.MediumOrchid
                    };
                    int offset = 1; // Décalage pour les points
                    double argent = 0.0;

                    if (!gotInitialSettings) {
                        // Il faut initialement prendre les paramètres dans la boucle extérieur
                        string strJSON = Recevoir();
                        settings = JsonConvert.DeserializeObject<Settings>(strJSON);
                        gotInitialSettings = true;
                    }

                    // Ajout de couleur(s) supplémentaire(s) si elles sont autorisées
                    switch (settings.ColorNumber) {
                    case 7:
                        couleurs.Add(Color.Cyan); break;
                    case 8:
                        couleurs.Add(Color.Cyan);
                        couleurs.Add(Color.White); break;
                    }

                    if (settings.Blanks)
                        couleurs.Add(Color.Gray);

                    // Générer une combinaison aléatoire
                    for (int i = 0; i < 4; i++) {
                        int index = random.Next(0, couleurs.Count);
                        solution.Couleurs[i] = couleurs[index];

                        // Si les couleurs dupliqués ne sont pas autorisées, on enlève cette couleur de la liste
                        if (!settings.Duplicates)
                            couleurs.RemoveAt(index);
                    }

                    // Pour afficher la solution
                    //for (int i = 0; i < 4; i++) Console.Write($"{solution.Couleurs[i]} ");
                    //Console.WriteLine();

                    // Ajouter la combinaison à la base de données
                    int colorPacketID =
                        colorsID[solution.Couleurs[0]] * 1000 +
                        colorsID[solution.Couleurs[1]] * 100 +
                        colorsID[solution.Couleurs[2]] * 10 +
                        colorsID[solution.Couleurs[3]];
                    //int ra = sqlManager_.Insert(colorPacketID); // Pour ajouter la combinaison à la base de données
                    //Console.WriteLine(ra); // Pour vérifier le nombre de rangée affectées

                    while (true) {
                        string strJSON = Recevoir();
                        guess = JsonConvert.DeserializeObject<ColorPacket>(strJSON);

                        if (guess.CurrentRow == 0) {
                            // Si le CurrentRow est égal à 0, il s'agit d'un objet de type Settings
                            settings = JsonConvert.DeserializeObject<Settings>(strJSON); // On met à jour les paramètres
                            break; // On recommence une nouvelle partie
                        }

                        for (int i = 0; i < 4; i++) // Préparer le feedback
                            feedBack.Couleurs[i] = Color.LightGray;

                        // Se préparer à faire le logique des pions noirs et des pions blancs
                        bool[] guessMatches = { false, false, false, false };
                        bool[] solutionMatches = { false, false, false, false };
                        int feedbackIndex = 0;
                        int blackPinCount = 0;

                        // Pions noirs (Une seule boucle)
                        for (int i = 0; i < 4; i++) {
                            if (guess.Couleurs[i] == solution.Couleurs[i]) {
                                if (settings.NormalFeedback) {
                                    feedBack.Couleurs[feedbackIndex] = Color.Black;
                                    feedbackIndex++;
                                }
                                else
                                    feedBack.Couleurs[i] = Color.Black;

                                guessMatches[i] = true;
                                solutionMatches[i] = true;
                                blackPinCount++;
                            }
                        }

                        // Pions blancs (Une boucle dans une boucle)
                        for (int i = 0; i < 4; i++) {
                            // Ici, déjà une correspondance pour cette index du guess ou il s'agit d'un espace vide
                            //Console.WriteLine($"{guessMatches[i]} || {settings.Blanks} && {guess.Couleurs[i]} == {Color.Gray}");
                            if (guessMatches[i] || guess.Couleurs[i] == Color.Gray)
                                continue;

                            for (int i2 = 0; i2 < 4; i2++) {
                                if (!solutionMatches[i2] && guess.Couleurs[i] == solution.Couleurs[i2]) {
                                    if (settings.NormalFeedback) {
                                        feedBack.Couleurs[feedbackIndex] = Color.White;
                                        feedbackIndex++;
                                    }
                                    else
                                        feedBack.Couleurs[i] = Color.White;

                                    guessMatches[i] = true;
                                    solutionMatches[i2] = true;
                                    break; // On peut matcher une seule fois par index
                                }
                            }
                        }

                        // Calcul des points
                        if (blackPinCount > 0) {
                            int points;

                            if ((points = blackPinCount + offset) > 0)
                                argent += points * dollarParPoint;
                        }

                        offset--;

                        // Déterminer si le joueur est gagnant
                        bool winner = true;

                        for (int i = 0; i < 4; i++) {
                            if (feedBack.Couleurs[i] != Color.Black) {
                                winner = false;
                                break;
                            }
                        }

                        // Mettre les valeurs supplémentaires du ColorPacket pour le feedback
                        feedBack.CurrentRow = guess.CurrentRow;
                        feedBack.Winner = winner;
                        feedBack.Argent = argent;

                        // Envoyer au client
                        if (winner) { // Le joueur est gagnant
                            Envoyer(solution);
                            Envoyer(feedBack);
                        }
                        else {
                            if (guess.CurrentRow == 10) {
                                feedBack.Argent = 0.0;
                                Envoyer(solution);
                            }

                            Envoyer(feedBack);
                        }
                    }
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }

            // Fermeture de socket
            if (socketCommunication != null) {
                socketCommunication.Client.Shutdown(SocketShutdown.Both);
                socketCommunication.Close();
            }
        }

        public void Envoyer(object obj) {
            string strJSON = JsonConvert.SerializeObject(obj); // Sérializer
            byte[] cryptedData = EncryptData(strJSON); // Encrypter

            // Envoyer
            writer.Write(cryptedData, 0, cryptedData.Length);
            writer.Flush();
            socketCommunication.GetStream().Flush();
        }

        public string Recevoir() {
            // Recevoir
            byte[] cryptedData = new byte[2048];
            int nbOctets = 0;

            do {
                nbOctets += reader.Read(cryptedData, 0, cryptedData.Length);
            } while (socketCommunication.Available > 0);

            // Décrypter
            return DecryptData(cryptedData.Take(nbOctets).ToArray());
        }

        public byte[] EncryptData(string data) {
            ICryptoTransform cryptoTransform = desProvider.CreateEncryptor(key, iv);
            byte[] inputData = Encoding.ASCII.GetBytes(data);
            byte[] cryptedData = cryptoTransform.TransformFinalBlock(inputData, 0, inputData.Length);

            return cryptedData;
        }

        public string DecryptData(byte[] inputData) {
            ICryptoTransform cryptoTransform = this.desProvider.CreateDecryptor(this.key, this.iv);
            byte[] decryptedData = cryptoTransform.TransformFinalBlock(inputData, 0, inputData.Length);
            string strData = Encoding.ASCII.GetString(decryptedData);

            return strData;
        }
    }
}
