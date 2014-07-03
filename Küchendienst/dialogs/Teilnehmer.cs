using System;
using System.Windows.Forms;
using System.Data;
using System.Data.OleDb;

namespace Küchendienst
{
    public partial class Teilnehmer : Form
    {
        public Teilnehmer()
        {
            InitializeComponent();
            dataGridView1.BackgroundColor = System.Drawing.SystemColors.Control;
            showTable(dataGridView1);

        }

        public static void showTable(DataGridView bindTo, String query=null) {

            OleDbCommand cmd;

            if (query == null)
            {
                cmd = new OleDbCommand("Select id, vorname as Vorname, nachname as Nachname from teilnehmer ORDER BY Nachname asc", DBHelper.connection);
            }
            else
            {
                cmd = new OleDbCommand(query, DBHelper.connection);
            }
            OleDbDataAdapter myDataAdapter = new OleDbDataAdapter(cmd);
            DataTable scores = new DataTable();
            myDataAdapter.Fill(scores);
            //scores.
            bindTo.DataSource = scores;
            bindTo.Columns[0].Visible = false;
        }

        private void bt_delete_Click(object sender, EventArgs e)
        {
            
            int selectedIndex = dataGridView1.SelectedRows[0].Index;
            var id = int.Parse(dataGridView1[0, selectedIndex].Value.ToString());

            string name = dataGridView1[1, selectedIndex].Value.ToString() + " " + dataGridView1[2, selectedIndex].Value.ToString();

            DialogResult dialogResult = MessageBox.Show("Soll \"" + name + "\" wirklich gelöscht werden?", "Wirklich löschen?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dialogResult == DialogResult.No)
            {
                return;
            }

            OleDbCommand cmd = new OleDbCommand("DELETE FROM teilnehmer where ID=@ID", DBHelper.connection);
            cmd.Parameters.AddWithValue("@ID", id);
            cmd.ExecuteNonQuery();
            showTable(dataGridView1);
        }

        private void btn_add_Click(object sender, EventArgs e)
        {
            addTeilnehmer add = new addTeilnehmer();
            DialogResult dr = add.ShowDialog();
            if (dr == DialogResult.OK)
            {
                OleDbCommand cmd = new OleDbCommand("INSERT INTO teilnehmer (vorname, nachname) VALUES (@vorname, @nachname)", DBHelper.connection);
                cmd.Parameters.AddWithValue("@vorname", add.vorname);
                cmd.Parameters.AddWithValue("@nachname", add.nachname);
                cmd.ExecuteNonQuery();
                showTable(dataGridView1);
            }
        }

        private void btn_edit_Click(object sender, EventArgs e)
        {
            int selectedIndex = dataGridView1.SelectedRows[0].Index;
            var id = int.Parse(dataGridView1[0, selectedIndex].Value.ToString());
            var vorname = dataGridView1[1, selectedIndex].Value.ToString();
            var nachname = dataGridView1[2, selectedIndex].Value.ToString();

            addTeilnehmer edit = new addTeilnehmer();
            DialogResult dr = edit.customDialog(id, vorname, nachname);

            if (dr == DialogResult.OK)
            {
                OleDbCommand cmd = new OleDbCommand("UPDATE teilnehmer SET vorname=@vorname,nachname=@nachname WHERE ID=@id", DBHelper.connection);
                cmd.Parameters.AddWithValue("@vorname", edit.vorname);
                cmd.Parameters.AddWithValue("@nachname", edit.nachname);
                cmd.Parameters.AddWithValue("@id", edit.id);
                cmd.ExecuteNonQuery();
                showTable(dataGridView1);

                int rowIndex = -1;
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (int.Parse(row.Cells[0].Value.ToString()) == edit.id)
                    {
                        rowIndex = row.Index;
                        break;
                    }
                }
                dataGridView1.FirstDisplayedScrollingRowIndex = rowIndex;
                dataGridView1.Rows[rowIndex].Selected = true;
            }
        }
    }
}
