using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.Data;
using System.Data.OleDb;

namespace Küchendienst
{
    public partial class addDienst : Form
    {

        public int KW;
        public int Year;
        private List<Person> _persons = new List<Person>();

        public addDienst()
        {
            InitializeComponent();
        }

        public static DateTime GetFirstDayOfWeek(DateTime dayInWeek)
        {
            var cultureInfo = CultureInfo.CurrentCulture;
            DayOfWeek firstDay = cultureInfo.DateTimeFormat.FirstDayOfWeek;
            DateTime firstDayInWeek = dayInWeek.Date;
            while (firstDayInWeek.DayOfWeek != firstDay)
                firstDayInWeek = firstDayInWeek.AddDays(-1);

            return firstDayInWeek;
        }


        private void monthCalendar1_DateSelected(object sender, DateRangeEventArgs e)
        {
            var d = GetFirstDayOfWeek(e.Start);

            monthCalendar1.SelectionStart = d;
            monthCalendar1.SelectionEnd = d.AddDays(6);

            var cultureInfo = CultureInfo.CurrentCulture;
            var weekNo = cultureInfo.Calendar.GetWeekOfYear(
                            e.Start,
                            cultureInfo.DateTimeFormat.CalendarWeekRule,
                            cultureInfo.DateTimeFormat.FirstDayOfWeek);
            lblKW.Text = weekNo.ToString();

            Year = e.Start.Year;
            KW = weekNo;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            selectTeilnehmer select = new selectTeilnehmer();
            DialogResult dr = select.ShowDialog();
            if (dr == DialogResult.OK)
            {
                for (int i = _persons.Count - 1; i >= 0; i--)
                {
                    if (_persons[i].Vorname == select.selected.Vorname && _persons[i].Nachname == select.selected.Nachname)
                    {
                        MessageBox.Show("Teilnehmer bereits in der Liste vorhanden.", "Bereits vorhanden", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
                _persons.Add(select.selected);
                listBox1.DataSource = null;
                listBox1.DataSource = _persons;
                listBox1.DisplayMember = "Display";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var selectedPerson = (Person)listBox1.SelectedItem;
            if (selectedPerson != null)
            {
                for (int i = _persons.Count - 1; i >= 0; i--)
                {
                    if (_persons[i].ID == selectedPerson.ID)
                    {
                        _persons.RemoveAt(i);
                        listBox1.DataSource = null;
                        listBox1.DataSource = _persons;
                        listBox1.DisplayMember = "Display";
                    }
                }
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {

            if (this.listBox1.Items.Count == 0)
            {
                MessageBox.Show("Fügen Sie einen Teilnehmer zu Liste hinzu!", "Teilnehmer hinzufügen!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (this.KW == 0)
            {
                MessageBox.Show("Wählen Sie eine Kalenderwoche aus!", "Kalenderwoche auswählen!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            foreach (var person in _persons)
            {
                OleDbCommand cmd = new OleDbCommand("INSERT INTO dienst (teilnehmer_ID, jahr, kalenderwoche) VALUES (@id, @jahr, @kw)", DBHelper.connection);
                cmd.Parameters.AddWithValue("@id", person.ID);
                cmd.Parameters.AddWithValue("@jahr", this.Year);
                cmd.Parameters.AddWithValue("@kw", this.KW);
                cmd.ExecuteNonQuery();
                this.Close();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void monthCalendar1_DateChanged(object sender, DateRangeEventArgs e)
        {

        }
    }
}
