using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Küchendienst
{
    class DBHelper
    {
        public static OleDbConnection connection;
        public static string password = "";

        public static void init(string pw)
        {

            if (!File.Exists("db.accdb"))
            {
                MessageBox.Show("Datenbank Datei 'db.accdb' konnte nicht gefunden werde. Es wird eine leere Datenbank mit folgendem Passwort erstellt:\n\n\t\t12345", "Datenbank nicht gefunden", MessageBoxButtons.OK, MessageBoxIcon.Information);
                using (Stream input = Assembly.GetExecutingAssembly().GetManifestResourceStream("Küchendienst.assets.empty.accdb"))
                using (Stream output = File.Create("db.accdb"))
                {
                    CopyStream(input, output);
                }
            }

            try
            {
                DataSet myDataSet = new DataSet();
                string conn = string.Format(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=db.accdb;Jet OLEDB:Database Password={0};Mode=Share Exclusive;", pw);
                connection = new OleDbConnection(conn);
                connection.Open();
                password = pw;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fehler aufgetreten", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
        }

        public static DataTable query(String query)
        {
            OleDbCommand cmd = new OleDbCommand(query, DBHelper.connection);
            OleDbDataAdapter myDataAdapter = new OleDbDataAdapter(cmd);
            DataTable scores = new DataTable();
            myDataAdapter.Fill(scores);


            return scores;
        }

        public static DataTable query(String query, Hashtable parameters)
        {
            OleDbCommand cmd = new OleDbCommand(query, DBHelper.connection);
            foreach (DictionaryEntry param in parameters)
            {
                cmd.Parameters.AddWithValue((String)param.Key, param.Value);
            }
            OleDbDataAdapter myDataAdapter = new OleDbDataAdapter(cmd);
            DataTable scores = new DataTable();
            myDataAdapter.Fill(scores);

            return scores;
        }

        public static void CopyStream(Stream input, Stream output)
        {
            // Insert null checking here for production
            byte[] buffer = new byte[8192];

            int bytesRead;
            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
        }
    }
}
