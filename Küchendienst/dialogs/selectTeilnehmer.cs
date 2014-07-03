using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Küchendienst
{
    public partial class selectTeilnehmer : Form
    {

        public Person selected;

        public selectTeilnehmer()
        {
            InitializeComponent();
            dataGridView1.BackgroundColor = System.Drawing.SystemColors.Control;
            Teilnehmer.showTable(dataGridView1);
            dataGridView1.ClearSelection();
            this.ActiveControl = textBox1;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                Teilnehmer.showTable(dataGridView1);
            }
            else
            {
                string query = String.Format("Select id, vorname, nachname from teilnehmer WHERE vorname like '{0}%' or nachname like '{0}%' order by nachname asc;", textBox1.Text);
                Teilnehmer.showTable(dataGridView1, query);
            }
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            var row = dataGridView1.SelectedRows[0];
            selected = (Person)dataGridView1.SelectedRows[0];
            this.Close();
        }

        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            var row = dataGridView1.SelectedRows[0];
            selected = (Person)dataGridView1.SelectedRows[0];
            this.Close();
        }
    }
}
