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
    public partial class PasswordChange : Form
    {
        public PasswordChange()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(txt_pwNew.Text) && !String.IsNullOrEmpty(txt_pwNew2.Text) && txt_pwNew.Text == txt_pwNew2.Text)
            {
                String query = String.Format("ALTER DATABASE PASSWORD {0} {1}", txt_pwNew.Text, DBHelper.password);
                OleDbCommand cmd = new OleDbCommand(query, DBHelper.connection);
                cmd.ExecuteNonQuery();
                this.Close();
            }
            else
            {
                MessageBox.Show("Passwörter stimmen nicht überein!", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
        
        }
    }
}
