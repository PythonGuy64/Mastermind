using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.Drawing;
using System.Security.Cryptography;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace ClientMasterMind {
    public class Client {
        // Marc-André Patry
        // 1726476

        private string adresseIP_Serveur; // Adresse IP du serveur
        private int portServeur; // Port sur lequel le serveur écoute
        private TcpClient socketCommunication;
        private BufferedStream reader;
        private BufferedStream writer;
        private byte[] key; // Pour l'encryption
        private byte[] iv; // Pour l'encryption
        private DESCryptoServiceProvider desProvider; // Pour l'encryption

        public Client(string adresseIP, int port, byte[] aKey, byte[] aIv) {
            adresseIP_Serveur = adresseIP;
            portServeur = port;
            socketCommunication = new TcpClient();
            key = aKey;
            iv = aIv;
            desProvider = new DESCryptoServiceProvider();
        }

        public void Run() {
            socketCommunication.Client.Bind(new IPEndPoint(IPAddress.Any, 0)); // Assigner une adresse IP et un port
            socketCommunication.Connect(new IPEndPoint(IPAddress.Parse(adresseIP_Serveur), portServeur)); // Connecter
            reader = new BufferedStream(socketCommunication.GetStream());
            writer = new BufferedStream(socketCommunication.GetStream());
            ColorPacket solution = new ColorPacket();

            try {
                for (;;) {
                    string strJSON = Recevoir();
                    ColorPacket reponseColorPacket = JsonConvert.DeserializeObject<ColorPacket>(strJSON);

                    if (reponseColorPacket.CurrentRow == 0) { // Si CurrentRow est égal à 0, il s'agit de la solution
                        solution = reponseColorPacket;
                        continue;
                    }

                    UpdateArgent(reponseColorPacket.Argent); // Mettre à jour l'argent

                    if (!reponseColorPacket.Winner) { // Si le joueur n'est pas gagnant
                        for (int i = 0; i < 4; i++)
                            Application.OpenForms["Form1"].Controls[$"btnR{reponseColorPacket.CurrentRow}F{i + 1}"]
                                .BackColor = reponseColorPacket.Couleurs[i];

                        if (reponseColorPacket.CurrentRow == 10) { // Si le joueur a perdu
                            for (int i = 0; i < 4; i++) { // Afficher la solution
                                Application.OpenForms["Form1"].Controls[$"btnSolution{i + 1}"]
                                    .BackColor = solution.Couleurs[i];
                            }

                            MessageBox.Show("Vous avez perdu!");
                        }
                    }
                    else { // Si le joueur est gagnant
                        for (int i = 0; i < 4; i++) {
                            Application.OpenForms["Form1"].Controls[$"btnR{reponseColorPacket.CurrentRow}F{i + 1}"]
                                .BackColor = Color.Black; // Afficher le feedback
                            Application.OpenForms["Form1"].Controls[$"btnSolution{i + 1}"]
                                .BackColor = solution.Couleurs[i]; // Afficher la solution
                        }

                        MessageBox.Show($"Vous avez gagné avec {reponseColorPacket.Argent:C}!");

                        if (reponseColorPacket.CurrentRow < 10)
                            DisableColorButtons(); // Désactiver les boutons de couleurs quand le joueur gagne
                                                   // avant le dernier essaie
                    }
                }
            }
            catch (Exception e) {
                MessageBox.Show(Convert.ToString(e));
            }

            // Fermer le socket
            if (socketCommunication != null)
                socketCommunication.Close();
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
            ICryptoTransform cryptoTransform = desProvider.CreateDecryptor(key, iv);
            byte[] decryptedData = cryptoTransform.TransformFinalBlock(inputData, 0, inputData.Length);
            string strData = Encoding.ASCII.GetString(decryptedData);

            return strData;
        }

        public void UpdateArgent(double argent) {
            // Mettre à jour l'argent
            Label lbl = Application.OpenForms["Form1"].Controls["lblArgent"] as Label;
            lbl.Invoke(new Action(() => lbl.Text = $"Argent gagné: {argent:C}"));
        }

        public void DisableColor(string buttonName) {
            // Désactiver un bouton de couleur
            Button btn = Application.OpenForms["Form1"].Controls[buttonName] as Button;
            btn.Invoke(new Action(() => btn.Enabled = false));
        }

        public void DisableColorButtons() {
            // Désactiver tous les boutons de couleurs
            DisableColor("btnBleu");
            DisableColor("btnVert");
            DisableColor("btnJaune");
            DisableColor("btnOrange");
            DisableColor("btnRouge");
            DisableColor("btnMauve");
            DisableColor("btnCyan");
            DisableColor("btnBlanc");
            DisableColor("btnGris");
        }
    }
}
