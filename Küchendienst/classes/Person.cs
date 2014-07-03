using System;
using System.Windows.Forms;

namespace Küchendienst
{
    public class Person
    {
        public int _id;
        private string _vorname;
        private string _nachname;
        private string _display;

        public Person(int ID, string Vorname, string Nachname)
        {
            this._id = ID;
            this._vorname = Vorname;
            this._nachname = Nachname;
            this._display = Nachname + ", " + Vorname;
        }

        public string Display
        {
            get { return _display; }
            set { this._display = value; }
        }


        public int ID
        {
            get { return _id; }
            set { this._id = value; }
        }

        public string Vorname
        {
            get { return _vorname; }
            set { this._vorname = value; }
        }

        public string Nachname
        {
            get { return _nachname; }
            set { this._nachname = value; }
        }

        public static explicit operator Person(DataGridViewRow dr)
        {
            // code to convert from dr to Client

            int ID = (int)dr.Cells["ID"].Value;
            string Vorname = (String)dr.Cells["vorname"].Value;
            string Nachname = (String)dr.Cells["nachname"].Value;

            return new Person(ID, Vorname, Nachname);
        }
    }
}
