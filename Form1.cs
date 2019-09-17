using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using System.Net;
using EOSDigital.API;
using EOSDigital.SDK;

namespace SettingErha
{

    public partial class Form1 : Form
    {
        IniFile config;//= new IniFile("Settings.ini");
        public string wel, lol, hol, fin;
        public string weldur;
        public int weldur_i=0;
        public string cen, lef, righ;
        public string dire;
        public string rev,dup,tol,cnsz,scal;
        public string dbser, dbus, dbpw, dbnm;
        public static string cp,par,pal,pac,paths;
        public const string crev="0",cdup="0",ctol="1",ccnsz="400",cdbser="localhost",cdbus="root",cdbpw="",cdbnm="zuluerha",cscalf="5";
        public static string sser, suser, spass;

        private SerialPort ardPort;
        bool lampstate = false;

        CanonAPI APIHandler;
        public static List<Camera> CamList;
        bool IsInit = false;
        Bitmap Evf_Bmp;
        int LVBw, LVBh, w, h;
        float LVBratio, LVration;

        public Form1()
        {
            InitializeComponent();
            
            APIHandler = new CanonAPI();
            APIHandler.CameraAdded += APIHandler_CameraAdded;
            IsInit = true;
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            RefreshCamera();
            GetAvailableCOMPorts();
            
            //Creating file
            if (File.Exists("Settings.ini"))
            {
                config = new IniFile("Settings.ini");
            }
            else
            {
                File.Create("Settings.ini");
                config = new IniFile("Settings.ini");
            }

            //Opening file
            if (new FileInfo("Settings.ini").Length == 0)
                //if 1st time, fills with empty parameters
            {
                config.Write("wel", "", "Audio");
                config.Write("weldur", "0", "Audio");
                config.Write("load", "", "Audio");
                config.Write("hold", "", "Audio");
                config.Write("finish", "", "Audio");

                //camera
                config.Write("center", "", "Camera");
                config.Write("left", "", "Camera");
                config.Write("right", "", "Camera");

                config.Write("directory", "", "General");
                config.Write("review", "", "General");
                config.Write("duplicate", "", "General");
                config.Write("tolerance", "", "General");
                config.Write("censize", "", "General");
                config.Write("scalf", "", "General");
                config.Write("COM", "", "General");

                config.Write("server", "", "Database");
                config.Write("user", "", "Database");
                config.Write("pass", "", "Database");
                config.Write("name", "", "Database");
            }
            else
            {
                //not 1st time, read existing parameters
                //read settings for audio
                wel = config.Read("wel", "Audio");
                weldur = config.Read("weldur", "Audio");
                weldur_i = Convert.ToInt32(weldur);
                lol = config.Read("load", "Audio");
                hol = config.Read("hold", "Audio");
                fin = config.Read("finish", "Audio");
                //read settings for camera
                cen = config.Read("center", "Camera");
                lef = config.Read("left", "Camera");
                righ = config.Read("right", "Camera");
                //read settings for general
                dire = config.Read("directory", "General");
                rev = config.Read("review", "General");
                dup = config.Read("duplicate", "General");
                tol = config.Read("tolerance", "General");
                double tolf = Convert.ToDouble(tol) / 1000;
                tol = tolf.ToString();
                cnsz = config.Read("censize", "General");
                scal = config.Read("scalf", "General");
                cbCOMLamp.Text = config.Read("COM", "General");
                //read settings for db
                dbser = config.Read("server", "Database");
                dbus = config.Read("user", "Database");
                dbpw = config.Read("pass", "Database");
                dbnm = config.Read("name", "Database");
                //fill form with predefined settings
                WelP.Text = wel;
                LolP.Text = lol;
                HolP.Text = hol;
                FinP.Text = fin;
                CIP.Text = cen;
                LIP.Text = lef;
                RIP.Text = righ;
                dirt.Text = dire;

                if (rev != crev)
                {
                    RevEn.Checked = true;
                }
                if (dup != cdup)
                {
                    DupEn.Checked = true;
                }
                if (tol != ctol)
                {
                    toltime.Value = Convert.ToDecimal(tol);
                    TolTimEn.Checked = true;

                }
                if (cnsz != ccnsz)
                {
                    censize.Text = cnsz;
                    CenszEn.Checked = true;

                }
                if (scal != cscalf)
                {
                    scalf.Text = scal;
                    scalfen.Checked = true;

                }
                if ((dbser != cdbser) || (dbus != cdbus) || (dbpw != cdbpw) || (dbnm != cdbnm))
                {
                    DBServer.Text = dbser;
                    DBUser.Text = dbus;
                    DBPwd.Text = dbpw;
                    DBName.Text = dbnm;
                    DBEn.Checked = true;

                }
                
                if ((camerabox.Items.Count == 0) || (cameraabox.Items.Count == 0) || (cameraaabox.Items.Count == 0))
                {
                    MessageBox.Show("Silahkan periksa koneksi Kamera anda terlebih dahulu", "Gagal", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                }
                if (cen != "")
                {
                    int ci = Convert.ToInt32(cen);
                    if (ci < camerabox.Items.Count)
                    {
                        camerabox.SelectedIndex = ci;
                    }
                    else { camerabox.SelectedIndex = -1; }

                }
                if (lef != "")
                {
                    int ci = Convert.ToInt32(lef);
                    if (ci < cameraabox.Items.Count)
                    {
                        cameraabox.SelectedIndex = ci;
                    }
                    else { cameraabox.SelectedIndex = -1; }
                }
                if (righ != "")
                {
                    int ci = Convert.ToInt32(righ);
                    if (ci < cameraaabox.Items.Count)
                    {
                        cameraaabox.SelectedIndex = ci;
                    }
                    else { cameraaabox.SelectedIndex = -1; }
                }
            }
            
        }

        private void WelB_Click(object sender, EventArgs e)
        {
            OpenFileDialog choofdlog = new OpenFileDialog();
            choofdlog.Filter = "Wav Files (*.wav)|*.wav";
            choofdlog.FilterIndex = 1;

            if (choofdlog.ShowDialog() == DialogResult.OK)
            {
                string sFileName = choofdlog.FileName;
                WelP.Text = sFileName;
                wel = sFileName;
                byte[] TotalBytes = File.ReadAllBytes(sFileName);
                int bitrate = (BitConverter.ToInt32(new[] { TotalBytes[28], TotalBytes[29], TotalBytes[30], TotalBytes[31] }, 0) * 8);
                weldur_i = (TotalBytes.Length - 8) * 8 / bitrate;
            }
        }

        private void scalfen_MouseHover(object sender, EventArgs e)
        {
            tips.SetToolTip(scalfen, "Tick to customize image scaling factor, higher value resulting bigger filesize");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            dirt.Text = paths;
            MessageBox.Show(dirt.Text);
        }

        private void btLampCheck_Click(object sender, EventArgs e)
        {
            if(lampstate==false)
            //OFF state
            {
                try
                {
                    String port = cbCOMLamp.SelectedItem.ToString();
                    ardPort = new SerialPort();
                    ardPort.BaudRate = 9600;
                    ardPort.PortName = port;
                    ardPort.Open();

                    ardPort.WriteLine("1");
                    ardPort.Close();
                    btLampCheck.Text = "Turn OFF";
                    lampstate = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lampu tidak terhubung");
                }
            }
            else if(lampstate==true)
            //ON state
            {
                try
                {
                    String port = cbCOMLamp.SelectedItem.ToString();
                    ardPort = new SerialPort();
                    ardPort.BaudRate = 9600;
                    ardPort.PortName = port;
                    ardPort.Open();

                    ardPort.WriteLine("1");
                    ardPort.Close();
                    btLampCheck.Text = "Turn ON";
                    lampstate = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lampu tidak terhubung");
                }
            }
            
        }

        private void LolB_Click(object sender, EventArgs e)
        {
            OpenFileDialog choofdlog = new OpenFileDialog();
            choofdlog.Filter = "Wav Files (*.wav)|*.wav";
            choofdlog.FilterIndex = 1;

            if (choofdlog.ShowDialog() == DialogResult.OK)
            {
                string sFileName = choofdlog.FileName;
                LolP.Text = sFileName;
                lol = sFileName;
            }
        }

        private void HoldB_Click(object sender, EventArgs e)
        {
            OpenFileDialog choofdlog = new OpenFileDialog();
            choofdlog.Filter = "Wav Files (*.wav)|*.wav";
            choofdlog.FilterIndex = 1;

            if (choofdlog.ShowDialog() == DialogResult.OK)
            {
                string sFileName = choofdlog.FileName;
                HolP.Text = sFileName;
                hol = sFileName;
            }
        }

        private void scalfen_CheckedChanged(object sender, EventArgs e)
        {
            if (scalfen.Checked)
            {
                scalf.Enabled = true;
                scal = scalf.Value.ToString();
            }
            else
            {
                scalf.Enabled = false;
                scal = cscalf;
            }
        }

        private void FinB_Click(object sender, EventArgs e)
        {
            OpenFileDialog choofdlog = new OpenFileDialog();
            choofdlog.Filter = "Wav Files (*.wav)|*.wav";
            choofdlog.FilterIndex = 1;

            if (choofdlog.ShowDialog() == DialogResult.OK)
            {
                string sFileName = choofdlog.FileName;
                FinP.Text = sFileName;
                fin = sFileName;
            }
        }

        private void HolP_Click(object sender, EventArgs e)
        {
            
        }

        private void SaveB_Click(object sender, EventArgs e)
        {
            cen = CIP.Text;
            lef = LIP.Text;
            righ = RIP.Text;
            dire = dirt.Text;

            if ((CIP.Text != "") & (LIP.Text !="") & (RIP.Text !="") & (dirt.Text!="")) {
                //audio
                config.Write("wel", wel, "Audio");
                weldur = weldur_i.ToString();
                config.Write("weldur", weldur, "Audio");
                config.Write("load", lol, "Audio");
                config.Write("hold", hol, "Audio");
                config.Write("finish", fin, "Audio");

                //camera
                config.Write("center", cen, "Camera");
                config.Write("left", lef, "Camera");
                config.Write("right", righ, "Camera");


                //general
                if (CenszEn.Checked)
                {
                    //censize.Enabled = true;
                    cnsz = censize.Text;
                }
                else
                {
                    //censize.Enabled = false;
                    cnsz = ccnsz;
                }
                if (TolTimEn.Checked)
                {
                    //toltime.Enabled = true;
                    tol = toltime.Value.ToString();
                }
                else
                {
                   // toltime.Enabled = false;
                    tol = ctol;
                }
                if (scalfen.Checked)
                {
                    //scald.Enabled = true;
                    scal = scalf.Value.ToString();
                }
                else
                {
                    // toltime.Enabled = false;
                    scal = cscalf;
                }
                double tolf = Convert.ToDouble(tol) * 1000;
                tol = tolf.ToString();
                config.Write("directory", dire, "General");
                config.Write("review",rev,"General");
                config.Write("duplicate", dup, "General");
                config.Write("tolerance", tol, "General");
                config.Write("scalf", scal, "General");
                config.Write("censize", cnsz, "General");
                config.Write("COM", cbCOMLamp.Text, "General");

                //database
                if (DBEn.Checked)
                {
                    dbser = DBServer.Text;
                    dbus = DBUser.Text;
                    dbpw = DBPwd.Text;
                    dbnm = DBName.Text;
                    //groupBox4.Enabled = true;
                }
                else
                {
                    dbser = cdbser;
                    dbus = cdbus;
                    dbpw = cdbpw;
                    dbnm = cdbnm;
                   // groupBox4.Enabled = false;
                }
                config.Write("server", dbser, "Database");
                config.Write("user", dbus, "Database");
                config.Write("pass", dbpw, "Database");
                config.Write("name", dbnm, "Database");
                MessageBox.Show("Konfigurasi Sukses", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            else {
                MessageBox.Show("Silahkan periksa konfigurasi anda", "Gagal", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CancelB_Click(object sender, EventArgs e)
        {
            
            Application.Exit();
            /*if (System.Windows.Forms.Application.MessageLoop)
            {
                System.Windows.Forms.Application.Exit();
            }
            else
            {
                System.Environment.Exit(1);
            }*/
        }

        private void DirB_Click(object sender, EventArgs e)
        {
            /*using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {

                    dire = fbd.SelectedPath;
                    dirt.Text = dire;
                }
            }
            */
            if (DBEn.Checked)
            {
                sser = DBServer.Text;
                suser = DBUser.Text;
                spass = DBPwd.Text;
                
                //groupBox4.Enabled = true;
            }
            else
            {
                sser = cdbser;
                suser = cdbus;
                spass = cdbpw;
                
                // groupBox4.Enabled = false;
            }
            
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + sser);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(suser, spass);
                request.GetResponse();
                Form3 ab = new Form3();
                ab.Owner = (Form)this;
                ab.Show();

            }
            catch  (WebException ex)
            {
               if (ex.Status !=0)
                {
                    MessageBox.Show("Silahkan periksa konfigurasi server, username, dan password anda", "Gagal", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                
                

            }
            

            
        }

        private void DBEn_CheckedChanged(object sender, EventArgs e)
        {
            if (DBEn.Checked)
            {
                /**dbser = DBServer.Text;
                dbus = DBUser.Text;
                dbpw = DBPwd.Text;
                dbnm = DBName.Text;*/
                groupBox4.Enabled = true;
            }
            else
            {
                 /**dbser = cdbser;
                 dbus = cdbus;
                 dbpw = cdbpw;
                 dbnm = cdbnm;*/
                 groupBox4.Enabled = false;
            }

        }

        private void CenszEn_CheckedChanged(object sender, EventArgs e)
        {
            if (CenszEn.Checked)
            {
                censize.Enabled = true;
                //cnsz = censize.Text;
            }
            else
            {
                censize.Enabled = false;
               // cnsz = ccnsz;
            }
        }

        private void TolTimEn_CheckedChanged(object sender, EventArgs e)
        {
            if (TolTimEn.Checked)
            {
                toltime.Enabled = true;
                tol = toltime.Value.ToString();
            }
            else
            {
                toltime.Enabled = false;
                tol = ctol;
            }
        }

        private void RevEn_CheckedChanged(object sender, EventArgs e)
        {
            if (RevEn.Checked)
            {
                rev = "1";
            }
            else
            {
                rev = "0";
            }
        }

        private void DupEn_CheckedChanged(object sender, EventArgs e)
        {
            if (DupEn.Checked)
            {
                dup = "1";
            }
            else
            {
                dup = "0";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            cp = CIP.Text;
            Form2 ab = new Form2();
            ab.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            cp = LIP.Text;
            Form2 ab = new Form2();
            ab.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            cp = RIP.Text;
            Form2 ab = new Form2();
            ab.Show();
        }

        private void DupEn_MouseHover(object sender, EventArgs e)
        {
            tips.SetToolTip(DupEn, "Default: Replace if File Exist for the same Customer");
        }

        private void RevEn_MouseHover(object sender, EventArgs e)
        {
            tips.SetToolTip(RevEn, "Tick to enable result preview after capturing");
        }

        private void DBEn_MouseHover(object sender, EventArgs e)
        {
            tips.SetToolTip(DBEn, "Tick if server settings need to be configured");
        }

        private void CenszEn_MouseHover(object sender, EventArgs e)
        {
            tips.SetToolTip(CenszEn, "Tick to customize face detection area");
        }

        private void TolTimEn_MouseHover(object sender, EventArgs e)
        {
            tips.SetToolTip(TolTimEn, "Tick to customize tolerance for face detection");
        }
        
        private void RefreshCamera()
        {
            
            CamList = APIHandler.GetCameraList();
            foreach (Camera cam in CamList) camerabox.Items.Add(cam.DeviceName);
            foreach (Camera cam in CamList) cameraabox.Items.Add(cam.DeviceName);
            foreach (Camera cam in CamList) cameraaabox.Items.Add(cam.DeviceName);
            
        }

        private void GetAvailableCOMPorts()
        {
            string[] ports = SerialPort.GetPortNames();

            foreach (String p in ports) cbCOMLamp.Items.Add(p);
        }

        private void APIHandler_CameraAdded(CanonAPI sender)
        {
            try { Invoke((Action)delegate { RefreshCamera(); }); }
            catch (Exception ex) { }
        }

        private void camerabox_SelectedIndexChanged(object sender, EventArgs e)
        {
            CIP.Text = camerabox.SelectedIndex.ToString();
        }

        private void cameraabox_SelectedIndexChanged(object sender, EventArgs e)
        {
            LIP.Text = cameraabox.SelectedIndex.ToString();
        }

        private void cameraaabox_SelectedIndexChanged(object sender, EventArgs e)
        {
            RIP.Text = cameraaabox.SelectedIndex.ToString();
        }

        private void RIP_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            // this.Close();
            APIHandler?.Dispose();
            if (System.Windows.Forms.Application.MessageLoop)
            {
                System.Windows.Forms.Application.Exit();
            }
            else
            {
                System.Environment.Exit(0);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;
            cp = CIP.Text;
            Form2 abc = new Form2();
            abc.Show();
            cp = LIP.Text;
            Form2 ab = new Form2();
            ab.Show();
            ab.StartPosition = FormStartPosition.Manual;
            ab.Location = new System.Drawing.Point(0, abc.Location.Y);
            cp = RIP.Text;
            Form2 abd = new Form2();
            abd.Show();
            abd.StartPosition = FormStartPosition.Manual;
            abd.Location = new System.Drawing.Point((screenWidth-abd.Width), abc.Location.Y);
        }
    }
}
