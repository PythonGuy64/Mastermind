using System;
using System.Threading.Tasks;

namespace ServeurParentMasterMind {
    class Program {
        // Marc-André Patry
        // 1726476

        static void Main(string[] args) {
            ServeurParent serveurParent = new ServeurParent("127.0.0.1", 5000);
            Task taskServeur = new Task(serveurParent.Run);
            taskServeur.Start();
            taskServeur.Wait();
        }
    }
}
