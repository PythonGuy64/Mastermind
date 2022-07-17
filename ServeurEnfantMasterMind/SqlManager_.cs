using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace ServeurEnfantMasterMind {
    public class SqlManager_ {
        // Marc-André Patry
        // 1726476

        private string connectionString;
        private SqlConnection connection;

        public SqlManager_() {
            // Entrer votre connection string ici
            connectionString = @"Data Source=DESKTOP-4OJ38L4\SQL;Initial Catalog=MasterMind;User ID=sa;Password=SQL";

            // La connection
            connection = new SqlConnection(connectionString);
        }

        public int Insert(int combinaison) {
            connection.Open(); // Ouvrir la connection

            // Créer la requête
            string query = "insert into HistoriqueDeCombinaison values(@colorPacketID)";
            SqlCommand command = new SqlCommand(query, connection);

            // Mettre une valeur à la variable colorPacket
            command.Parameters.AddWithValue("colorPacketID", combinaison);

            //
            int rowsAffected = command.ExecuteNonQuery(); // Exécuter la requête
            connection.Close(); // Fermer la connection

            return rowsAffected;
        }

        public string ConnectionString {
            get => connectionString; // On peut seulement lire le connectionString
        }

        public SqlConnection Connection {
            get => connection;
            set => connection = value;
        }
    }
}
