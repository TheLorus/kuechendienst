using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using Pechkin.Synchronized;
using Pechkin;
using System.Drawing.Printing;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Data;
using System.Data.OleDb;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections;

namespace Küchendienst
{
    public partial class Main : Form
    {

        public Teilnehmer teilnehmer;
        public String the_html;
        private int minHighlight = 10;

        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        public Main()
        {

            LoadDllFromRessource("libgcc_s_dw2-1.dll");
            LoadDllFromRessource("mingwm10.dll");
            LoadDllFromRessource("wkhtmltox0.dll");

            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {

                String resourceName = "Küchendienst.dlls." +

                   new AssemblyName(args.Name).Name + ".dll";

                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {

                    Byte[] assemblyData = new Byte[stream.Length];

                    stream.Read(assemblyData, 0, assemblyData.Length);

                    return Assembly.Load(assemblyData);

                }

            };
            InitializeComponent();
        }

        public void LoadDllFromRessource(String filename)
        {
            string dirName = Path.Combine(Path.GetTempPath(), "Küchendienst." +
             Assembly.GetExecutingAssembly().GetName().Version.ToString());
            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);
            string dllPath = Path.Combine(dirName, filename);

            // Get the embedded resource stream that holds the Internal DLL in this assembly.
            // The name looks funny because it must be the default namespace of this project
            // (MyAssembly.) plus the name of the Properties subdirectory where the
            // embedded resource resides (Properties.) plus the name of the file.
            using (Stream stm = Assembly.GetExecutingAssembly().GetManifestResourceStream(
              "Küchendienst.dlls."+filename))
            {
                // Copy the assembly to the temporary file
                try
                {
                    using (Stream outFile = File.Create(dllPath))
                    {
                        const int sz = 4096;
                        byte[] buf = new byte[sz];
                        while (true)
                        {
                            int nRead = stm.Read(buf, 0, sz);
                            if (nRead < 1)
                                break;
                            outFile.Write(buf, 0, nRead);
                        }
                    }
                }
                catch
                {
                    // This may happen if another process has already created and loaded the file.
                    // Since the directory includes the version number of this assembly we can
                    // assume that it's the same bits, so we just ignore the excecption here and
                    // load the DLL.
                }
            }

            // We must explicitly load the DLL here because the temporary directory 
            // is not in the PATH.
            // Once it is loaded, the DllImport directives that use the DLL will use
            // the one that is already loaded into the process.
            IntPtr h = LoadLibrary(dllPath);
            Debug.Assert(h != IntPtr.Zero, "Unable to load library " + dllPath);
        }

        private static String HexConverter(System.Drawing.Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }

        public String generateHTML(int kw)
        {

            //int kw = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek);

            string query = String.Format(@"
                SELECT j1.ID, j1.vorname, j1.nachname, j2.count4, j3.count8, j4.count12
                FROM ((teilnehmer AS j1
                LEFT JOIN (
                    SELECT Count(teilnehmer.ID) AS count4, teilnehmer.ID
                    FROM teilnehmer INNER JOIN dienst
                    ON teilnehmer.ID = dienst.teilnehmer_ID
                    WHERE dienst.kalenderwoche <= {0} and dienst.kalenderwoche > {1}
                    GROUP BY teilnehmer.ID) AS j2
                ON j1.ID = j2.ID)
                LEFT JOIN (
                    SELECT Count(teilnehmer.ID) AS count8, teilnehmer.ID
                    FROM teilnehmer INNER JOIN dienst
                    ON teilnehmer.ID = dienst.teilnehmer_ID
                    WHERE dienst.kalenderwoche <= {0} and dienst.kalenderwoche > {2}
                    GROUP BY teilnehmer.ID) AS j3
                ON j1.ID = j3.ID)
                LEFT JOIN (
                    SELECT Count(teilnehmer.ID) AS count12, teilnehmer.ID
                    FROM teilnehmer INNER JOIN dienst
                    ON teilnehmer.ID = dienst.teilnehmer_ID
                    WHERE dienst.kalenderwoche <= {0} and dienst.kalenderwoche > {3} GROUP BY teilnehmer.ID)  AS j4
                ON j1.ID = j4.ID;
               ", kw, kw - 4, kw - 8, kw - 12);

            var scores = DBHelper.query(query);
            scores.Columns.Add("score");
            scores.Columns.Add("low");

            the_html = @"
                <table width=""100%"" style=""text-align:center;border-collapse: collapse;"" border=""1"">
                    <tr>
                        <th>Name</th>
                        <th>4 Wochen<br>(50%)</th>
                        <th>8 Wochen<br>(30%)</th>
                        <th>12 Wochen<br>(20%)</th>
                        <th>Score</th>
                    </tr>";

            double lowestScore = 100;
            int countZeroScore = 0;

            foreach (DataRow row in scores.Rows)
            {
                double score = 0;

                row["low"] = "false";

                row["count4"] = row["count4"] == DBNull.Value ? 0 : (int)row["count4"];
                row["count8"] = row["count8"] == DBNull.Value ? 0 : (int)row["count8"];
                row["count12"] = row["count12"] == DBNull.Value ? 0 : (int)row["count12"];
                var count4 = (int)row["count4"];
                var count8 = (int)row["count8"];
                var count12 = (int)row["count12"];

                if (count4 != 0)
                {
                    score += count4 * 0.5;
                }
                if (count8 != 0)
                {
                    score += count8 * 0.3;
                }
                if (count12 != 0)
                {
                    score += count12 * 0.2;
                }
                row["score"] = score;

                if (score < lowestScore)
                {
                    lowestScore = score;
                }

                if (score == 0)
                {
                    countZeroScore++;
                }

                //the_html += row["Vorname"].ToString() + "<br>";
            }

            if (countZeroScore < minHighlight || lowestScore != 0) {
                scores = determineLowest(scores, minHighlight);
            }

            DataView dv = scores.DefaultView;
            dv.Sort = "nachname asc";
            DataTable sortedScores = dv.ToTable();

            foreach (DataRow row in sortedScores.Rows)
            {
                if (double.Parse(row["score"].ToString()) == 0.0 || bool.Parse(row["low"].ToString()))
                {
                    the_html += string.Format("<tr bgcolor=\"#C6C6C6\"><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td></tr>", row["Nachname"] + ", " + row["Vorname"], row["count4"], row["count8"], row["count12"], row["score"]);
                }
                else
                {
                    the_html += string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td></tr>", row["Nachname"] + ", " + row["Vorname"], row["count4"], row["count8"], row["count12"], row["score"]);
                }

                
            }

            the_html += "</table>";


            return the_html;
        }

        private DataTable determineLowest(DataTable input, int count) {
            DataView dv = input.DefaultView;
            dv.Sort = "score,count4,count8,count12 asc, id desc";
            DataTable sortedByScores = dv.ToTable();
            int i = 0;
            foreach (DataRow row in sortedByScores.Rows)
            {
                if (i < count)
                {
                    row["low"] = "true";
                }
                i++;
            }

            return sortedByScores;
        }

        private void btn_member_Click(object sender, EventArgs e)
        {
            teilnehmer.ShowDialog();
            refreshAll();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            addDienst dienst = new addDienst();
            dienst.ShowDialog();
            refreshAll();
        }

        private void refreshAll(bool init=false)
        {
            var box = comboBox1;
            var result = DBHelper.query("SELECT kalenderwoche from dienst group by kalenderwoche order by kalenderwoche desc");
            box.Items.Clear();
            box.ResetText();
            foreach (DataRow row in result.Rows)
            {
                box.Items.Add(row["kalenderwoche"].ToString());
            }
            if (box.Items.Count > 0)
            {
                box.SelectedIndex = 0;
            }

            int kw = 0;
            if (comboBox1.Items.Count > 0)
            {
                kw = int.Parse(comboBox1.SelectedItem.ToString());
            }
            string html = "<body style=\"background-color:" + HexConverter(SystemColors.Control) + ";\">" + generateHTML(kw) + "</body>";

            if (init)
            {
                webBrowser1.DocumentText = html;
            }
            else
            {
                webBrowser1.Document.OpenNew(false);
                webBrowser1.Document.Write(html);
                webBrowser1.Refresh();
            }
        }

        private void refreshHtml(bool init = false)
        {

            int kw = 0;
            if (comboBox1.Items.Count > 0)
            {
                kw = int.Parse(comboBox1.SelectedItem.ToString());
            }
            string html = "<body style=\"background-color:" + HexConverter(SystemColors.Control) + ";\">" + generateHTML(kw) + "</body>";

            if (String.IsNullOrEmpty(webBrowser1.DocumentText))
            {
                webBrowser1.DocumentText = html;
            }
            else
            {
                webBrowser1.Document.OpenNew(false);
                webBrowser1.Document.Write(html);
                webBrowser1.Refresh();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //int kw = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek);
            int kw = 0;
            if (comboBox1.Items.Count > 0)
            {
                kw = int.Parse(comboBox1.SelectedItem.ToString());
            }

            if (kw == 0)
            {
                MessageBox.Show("Keine Kalenderwoche ausgewählt", "Auswahl", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int kwnext = kw + 1;
            int year = DateTime.Now.Year;
            saveFileDialog1.FileName = String.Format("küchendienst_{0}_kw{1:00}", year, kw+1);

            String query = @"
                SELECT teilnehmer.vorname, teilnehmer.nachname
                FROM teilnehmer INNER JOIN dienst ON teilnehmer.ID = dienst.teilnehmer_ID
                where dienst.kalenderwoche = @kw";

            Hashtable parameters = new Hashtable();
            parameters.Add("@kw", kw.ToString());

            var scores = DBHelper.query(query, parameters);

            the_html = generateHTML(kw);

            string right = "<ul>";
            foreach (DataRow row in scores.Rows)
            {
                right += String.Format("<li>{0}, {1}</li>", row["nachname"], row["vorname"]);
            }

            right += "</ul>";

            the_html = @"
                <html>
                <head>
                <style type=""text/css"">
                    #wrapper {
                        width: 100%;
                        height: auto;
                    }
                    #left {
                         width: 60%;
                         height: auto;
                         float: left;
                     }
                    #right {
                          width: 35%;
                          margin-left: 5%;
                          height: auto;
                          float: left;
                     }
                </style>
                </head>
                <body>
                <div id=""wrapper"">
                  <section id=""left"">" + this.the_html + @"</section>   
                  <section id=""right"">
                    <u>Küchendienst diese Woche (KW" + kw.ToString() + @"):</u><br>" + right + @"<br><br><u>Küchendienst nächste Woche (KW" + kwnext.ToString() + @"):</u><br><ul><li>...............</li><br><li>...............</li><br><li>...............</li><br><li>...............</li></ul>
                  </section>
                </div>
                </body>
                </html>
            ";

            saveFileDialog1.ShowDialog();
        }

        private void onFinished(SimplePechkin converter, bool success)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => { progressBar1.Hide(); }));
            }
            else
            {
                progressBar1.Hide();
            }


        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            progressBar1.Show();
            ThreadStart ts = () =>
            {
                SynchronizedPechkin sc = new SynchronizedPechkin(new GlobalConfig().SetMargins(new Margins(10, 10, 10, 10))
                    .SetDocumentTitle("Ololo").SetCopyCount(1).SetImageQuality(50)
                    .SetLosslessCompression(true).SetMaxImageDpi(20).SetOutlineGeneration(true).SetOutputDpi(1200).SetPaperOrientation(false)
                    .SetPaperSize(PaperKind.Letter));

                sc.Finished += new Pechkin.EventHandlers.FinishEventHandler(onFinished);
                sc.Error += new Pechkin.EventHandlers.ErrorEventHandler(sc_Error);

                byte[] buf = sc.Convert(new ObjectConfig().SetPrintBackground(true), the_html);

                try
                {
                    string fn = saveFileDialog1.FileName;

                    FileStream fs = new FileStream(fn, FileMode.Create);
                    fs.Write(buf, 0, buf.Length);
                    fs.Close();

                    Process myProcess = new Process();
                    myProcess.StartInfo.FileName = fn;
                    myProcess.Start();
                }
                catch { }

            };
            Thread t = new Thread(ts);
            t.Start();
        }

        void sc_Error(SimplePechkin converter, string errorText)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => { MessageBox.Show(errorText); }));
            }
            else
            {
                MessageBox.Show(errorText);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SingleView sv = new SingleView();
            sv.ShowDialog();
            refreshAll();
        }

        private void Main_Shown(object sender, EventArgs e)
        {
            if (!File.Exists("db.accdb"))
            {
                DBHelper.init("12345");
                teilnehmer = new Teilnehmer();
                refreshAll(true);
                return;
            }

            PasswordInput pw = new PasswordInput();
            DialogResult dr = pw.ShowDialog();

            if (dr == DialogResult.OK)
            {
                DBHelper.init(pw.txt_pw.Text);
                teilnehmer = new Teilnehmer();
                refreshAll(true);
            }
            else
            {
                Application.Exit();
            }
        }

        private void passwordÄndernToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            PasswordChange pw = new PasswordChange();
            DialogResult dr = pw.ShowDialog();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            refreshHtml();
        }
    }
}
