using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using ServeurEnfantMasterMind;

namespace ServeurParentMasterMind {
    public class ServeurParent {
        // Marc-André Patry
        // 1726476

        private string adresseIP; // Adresse IP du serveur
        private int port; // Port sur lequel le serveur écoute
        private TcpListener socketConnection;

        public ServeurParent(string aAdresseIP, int aPort) {
            adresseIP = aAdresseIP;
            port = aPort;
            socketConnection = new TcpListener(new IPEndPoint(IPAddress.Parse(adresseIP), port));
        }

        public void Run() {
            try {
                socketConnection.Start();

                for (;;) {
                    TcpClient socketCommunication = socketConnection.AcceptTcpClient(); // Accepter les clients

                    // Faire un instance de ServeurEnfant et lui démarrer une tâche avec la fonction Run
                    ServeurEnfant serveurEnfant = new ServeurEnfant(
                        socketCommunication,
                        new byte[] { 200, 5, 78, 232, 9, 6, 0, 4 },
                        new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
                    new Task(serveurEnfant.Run).Start();
                }
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }
    }
}
