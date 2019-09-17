using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using EOSDigital.API;
using EOSDigital.SDK;

namespace SettingErha
{
    public partial class Form2 : Form
    {
        private VideoCapture _captureFront = null;
        CanonAPI APIHandler;
        EOSDigital.API.Camera MainCamera;
        EOSDigital.API.Camera RightCamera;
        EOSDigital.API.Camera LeftCamera;
        List<EOSDigital.API.Camera> CamList;
        bool IsInit = false;
        Bitmap Evf_Bmp;
        int LVBw, LVBh, w, h;
        float LVBratio, LVration;
        int ErrCount;
        object ErrLock = new object();
        object LvLock = new object();
        public int x = 0;
        public Form2()
        {
            InitializeComponent();
            string front = "1";
            front = Form1.cp;
            
            Int32.TryParse(front, out x);

            try
            {
                //Connect to Canon DSLR
                APIHandler = new CanonAPI();
                ErrorHandler.SevereErrorHappened += ErrorHandler_SevereErrorHappened;
                ErrorHandler.NonSevereErrorHappened += ErrorHandler_NonSevereErrorHappened;

                Application.Idle += ProcessFrame;

                //LVBw = LiveViewPicBox.Width;
                //LVBh = LiveViewPicBox.Height;
                LVBw = ib.Width;
                LVBh = ib.Height;

                
                OpenSession();
                IsInit = true;
            }
            catch (NullReferenceException excpt)
            {
                ReportError(excpt.Message, true);
                Environment.Exit(0);
                return;
            }
            catch (DllNotFoundException) { ReportError("Canon DLLs not found!", true); }
            catch (Exception ex) { ReportError(ex.Message, true); }

        }
        private void ProcessFrame(object sender, EventArgs arg)
        
        {


            if (MainCamera == null || !MainCamera.SessionOpen) return;

            if (MainCamera.IsLiveViewOn)
            {
                lock (LvLock)
                {
                    if (Evf_Bmp != null)
                    {
                        
                        //Setting Live View area
                        LVBratio = LVBw / (float)LVBh;
                        LVration = Evf_Bmp.Width / (float)Evf_Bmp.Height;
                        if (LVBratio < LVration)
                        {
                            w = LVBw;
                            h = (int)(LVBw / LVration);
                        }
                        else
                        {
                            w = (int)(LVBh * LVration);
                            h = LVBh;
                        }
                       

                        
                        Image<Bgr, Byte> imageCV = new Image<Bgr, byte>(Evf_Bmp); //Image Class from Emgu.CV
                        Mat imgOriginal = imageCV.Mat;  //Convert image to mat for image processing
                        

                        //Show Result
                        ib.Image = imgOriginal;
                    }//if Evf_bmp!=NULL
                }//lock
            }//MainCamera?.IsLiveViewOn
        }

        

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Idle -= ProcessFrame;
            
            

            if (MainCamera == null || !MainCamera.SessionOpen)  { }
            else { CloseSession();  }
            
            this.Hide();
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
           
            
            
            if (MainCamera == null || !MainCamera.SessionOpen)  { }
            else { CloseSession();  }
            MainCamera?.Dispose();
            APIHandler?.Dispose();
        }
       

        private void OpenSession()
        //Description   :   Open MainCamera session
        {
            CamList = Form1.CamList;
            if (CamList.Count > 0)
            {
                MainCamera = CamList[x];
                MainCamera.OpenSession();
                MainCamera.StartLiveView();
                MainCamera.LiveViewUpdated += MainCamera_LiveViewUpdated;
                MainCamera.StateChanged += MainCamera_StateChanged;
                    
            }
            else
            {
                MessageBox.Show("Camera not connected\nPlease connect and try again", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //Terminate background process
                if (System.Windows.Forms.Application.MessageLoop)
                {
                    System.Windows.Forms.Application.Exit();
                }
                else
                {
                    System.Environment.Exit(1);
                }
            }
        }

        private void CloseSession()
        //Description   :   Close MainCamera session
        {
            MainCamera.CloseSession();
        }

        private void ReportError(string message, bool lockdown)
        //Description   :   Show error message according to numbers of error(s)
        {
            int errc;
            lock (ErrLock) { errc = ++ErrCount; }

            if (errc < 4) MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if (errc == 4) MessageBox.Show("Many errors happened!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            lock (ErrLock) { ErrCount--; }
        }
       

        private void MainCamera_LiveViewUpdated(EOSDigital.API.Camera sender, Stream img)
        {
            try
            {
                lock (LvLock)
                {
                    Evf_Bmp?.Dispose();
                    Evf_Bmp = new Bitmap(img);
                }
                ib.Invalidate();
            }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }

        private void MainCamera_StateChanged(EOSDigital.API.Camera sender, StateEventID eventID, int parameter)
        {
            try { if (eventID == StateEventID.Shutdown && IsInit) { Invoke((Action)delegate { CloseSession(); }); } }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }

        

        private void ErrorHandler_NonSevereErrorHappened(object sender, ErrorCode ex)
        {
            ReportError($"SDK Error code: {ex} ({((int)ex).ToString("X")})", false);
        }

        private void ErrorHandler_SevereErrorHappened(object sender, Exception ex)
        {
            ReportError(ex.Message, true);
        }

    }
}
