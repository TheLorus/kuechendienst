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
    public partial class addTeilnehmer : Form
    {

        public int id;
        public string vorname;
        public string nachname;

        public addTeilnehmer()
        {
            InitializeComponent();
        }

        public DialogResult customDialog(int id, string vorname, string nachname)
        {
            this.id = id;
            this.button1.Text = "Ändern";
            txtVorname.Text = vorname;
            txtNachname.Text = nachname;

            return this.ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            vorname = txtVorname.Text;
            nachname = txtNachname.Text;
        }
    }
}
