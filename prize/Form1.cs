using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace prize
{
    public partial class Form1 : Form
    {
        BackgroundWorker bw = null;

        private string path;
        private List<string> names;
        private List<string> winnerNames;
        private string prizeWinner;
        private int nameIndex = 0;

        public Form1()
        {
            InitializeComponent();

            path = Path.Combine(Application.StartupPath, "name.txt");
            names = new List<string>();
            winnerNames = new List<string>();
            
            getNames();

            bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += new DoWorkEventHandler(bw_dowork);
            bw.ProgressChanged += new ProgressChangedEventHandler(bw_progress);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_complete);
        }

        private void getNames()
        {
            StreamReader sr = new StreamReader(path, Encoding.UTF8);
            String line;
            while ((line = sr.ReadLine()) != null)
            {
                line = line.Trim();
                if (String.IsNullOrEmpty(line)) continue;
                names.Add(line);
                if (line.StartsWith("--"))
                {
                    winnerNames.Add(line.Substring(2));
                    setWinner(line.Substring(2));
                }
            }
            sr.Close();
        }

        private void setWinner(string name)
        {
            richTextBox1.Text += name + "\r\n";
        }

        private void updateName(string winner)
        {
            FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
            StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);

            for (int i = 0; i < names.Count; i++) {
                if (names[i].Equals(winner))
                {
                    names[i] = "--" + names[i];
                    sw.WriteLine(names[i]);
                }
                else {
                    sw.WriteLine(names[i]);
                }
            }
            sw.Flush();
            sw.Close();
            fs.Close();

            winnerNames.Add(winner);
            setWinner(winner);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (winnerNames.Count == names.Count)
            {
                label1.Text = "over";
                button1.Text = "over";
                return;
            }

            if (button1.Text.Equals("start"))
            {
                button1.Text = "stop";
                bw.RunWorkerAsync();
            }
            else {
                button1.Text = "start";
                bw.CancelAsync();
                bw.Dispose();
            }
        }

        //==============

        private void bw_dowork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (winnerNames.Count == names.Count) return;

                if (nameIndex >= names.Count) nameIndex = 0;
                while (names[nameIndex].StartsWith("--"))
                {
                    nameIndex++;
                    if (nameIndex >= names.Count) nameIndex = 0;
                }
                prizeWinner = names[nameIndex];
                nameIndex++;

                bw.ReportProgress(1);
                System.Threading.Thread.Sleep(100);

                if (bw.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        private void bw_progress(object sender, ProgressChangedEventArgs e)
        {
            label1.Text = prizeWinner;
        }

        private void bw_complete(object sender, RunWorkerCompletedEventArgs e)
        {
            updateName(prizeWinner);
        }
    }
}
