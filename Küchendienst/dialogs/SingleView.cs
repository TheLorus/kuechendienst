using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;

namespace Küchendienst
{
    public partial class SingleView : Form
    {

        public string query = @"
                SELECT dienst.ID, teilnehmer.vorname, teilnehmer.nachname, dienst.jahr, dienst.kalenderwoche
                FROM teilnehmer INNER JOIN dienst ON teilnehmer.ID = dienst.teilnehmer_ID
                ORDER BY dienst.jahr, dienst.kalenderwoche;
        ";

        public SingleView()
        {
            InitializeComponent();

            dataGridView1.BackgroundColor = System.Drawing.SystemColors.Control;
            Teilnehmer.showTable(dataGridView1, query);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int rowindex = dataGridView1.CurrentCell.RowIndex;
            int id = (int)dataGridView1.Rows[rowindex].Cells[0].Value;

            OleDbCommand cmd = new OleDbCommand("DELETE FROM dienst WHERE ID=@ID", DBHelper.connection);
            cmd.Parameters.AddWithValue("@ID", id);
            int result = cmd.ExecuteNonQuery();

            if (result == 1)
            {
                dataGridView1.Rows.Remove(dataGridView1.Rows[rowindex]);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            addDienst dienst = new addDienst();
            dienst.ShowDialog();
            Teilnehmer.showTable(dataGridView1, query);
            dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.RowCount - 1;
            dataGridView1.ClearSelection();
            dataGridView1.Rows[dataGridView1.RowCount - 1].Selected = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
