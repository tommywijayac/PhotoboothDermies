/*
 * PHOTOBOOTH DERMIES.EXE - Camera
 * ----------------------------------------------------------------------------------
 * Do face recognition to determine when to take photos
 * 
 * Input:
 * 1. Center camera live view stream
 * 
 * Output:
 * 1. Six temporary files photos (locally saved); standard and thumbnail three each
 * ----------------------------------------------------------------------------------
 */

//Basic reference
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
//Include reference for files-related function
using System.IO;
//Include reference for COM serial ports to connect with lamp
using System.IO.Ports;
//Include reference for database connection
using System.Net;
//Include reference for face detection using Emgu CV 3.1
using Emgu.CV;
using Emgu.CV.Structure;
//Include reference for playing .wav files
using System.Media;
//Include reference for database
using MySql.Data.MySqlClient;
//Include reference for Canon DSLR remote control
using EOSDigital.API;
using EOSDigital.SDK;

namespace Erha
{
    public partial class Camera : Form
    {
        #region Variables
        //Canon DSLR Camera variable
        CanonAPI APIHandler;
        EOSDigital.API.Camera MainCamera;
        EOSDigital.API.Camera RightCamera;
        EOSDigital.API.Camera LeftCamera;
        List<EOSDigital.API.Camera> CamList;

        bool IsInit = false;
        Bitmap Evf_Bmp; //input stream
        int w, h;   //screen w&h
        int ErrCount;
        object ErrLock = new object();  //error lock
        object LvLock = new object();   //liveview lock
        public double ska = 0, thum = 0.04;  //resize factor
        public static string imgsrc,imgout, sap ,dire,fold,uppath;  //upload variables
        
        //Settings variable
        IniFile Config = new IniFile("Settings.ini");       //config filename

        //Face detection variable
        private CascadeClassifier face = new CascadeClassifier("haarcascade_frontalface_default.xml");  //face library filename
        public System.Drawing.Rectangle centerBox = new System.Drawing.Rectangle();             //desired face area location and size
        //public List<System.Drawing.Rectangle> faces = new List<System.Drawing.Rectangle>();   //array contains faces location as result of face detection
        int timeLeft = 3;           //countdown variable
        bool timerRun = false;      //is countdown running?
        bool inside = false;        //is face inside desired area?
        int sisi;                   //centerbox size

        //Resize variable
        public static Mat fResized = new Mat();
        public static Mat rResized = new Mat();
        public static Mat lResized = new Mat();

        //Lamp variable
        private SerialPort ardPort;
        #endregion

        public Camera()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        //Description   :  when Camera form load, do: connect to cameras,
        //                 connect to lamp
        //                 get several parameters from settings file
        //                 show view frame while playing welcome audio
        {
            //Try to establish connection with camera
            try
            {
                CvInvoke.UseOpenCL = false;

                //Connect to Canon DSLR
                APIHandler = new CanonAPI();
                APIHandler.CameraAdded += APIHandler_CameraAdded;
                ErrorHandler.SevereErrorHappened += ErrorHandler_SevereErrorHappened;
                ErrorHandler.NonSevereErrorHappened += ErrorHandler_NonSevereErrorHappened;

                //Application.Idle += ProcessFrame;

                //Get camera list and open their session
                RefreshCamera();
                if (MainCamera?.SessionOpen == true) { CloseMainCamSession(); }
                else
                {
                    OpenMainCamSession();
                    IsInit = true;
                }

                if (RightCamera?.SessionOpen == true) { CloseRightCamSession(); }
                else
                {
                    OpenRightCamSession();
                }
                if (LeftCamera?.SessionOpen == true) { CloseLeftCamSession(); }
                else
                {
                    OpenLeftCamSession();
                }

                //Get centerbox size
                string temp = Config.Read("censize", "General");
                if (temp != "") { Int32.TryParse(temp, out sisi); }
                else { sisi = ibFront.Height; } //default size

                //Get time tolerate interval
                int temptimer;
                Int32.TryParse(Config.Read("tolerance", "General"), out temptimer);
                if (temptimer != 0) { timerResetTolerate.Interval = temptimer; }
                else { temptimer = 1000; }
            }
            catch (NullReferenceException excpt)
            {
                ReportError(excpt.Message, true);
                Environment.Exit(0);
                return;
            }
            catch (DllNotFoundException) { ReportError("Canon DLLs not found!", true); }
            catch (Exception ex) { ReportError(ex.Message, true); }
            
            //Try to establish connection with lamp, and then turn it on
            try
            {
                //Get serial port chosen
                String port = Config.Read("COM", "General");
                ardPort = new SerialPort();
                ardPort.BaudRate = 9600;
                ardPort.PortName = port;
                ardPort.Open();

                ardPort.WriteLine("1");
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                MessageBox.Show("Lampu tidak terhubung");
            }

            //Get scale factor for resizing
            string skal = Config.Read("scalf", "General");
            ska = Convert.ToDouble(skal) * 0.1;

            fold = Barcode.regid; //get value from Barcode form
            uppath = Config.Read("directory", "General");
            dire = @"D:\erhatemp";
            sap = dire + "\\" +fold+"\\"+ Barcode.upsynch + "\\";
            
            //Show view frame and play audio, after ticks, add process frame for face detection
            Application.Idle += ViewFrame;
            //Play welcome sound
            String wel = Config.Read("wel", "Audio");
            if (wel != "")
            {
                if (File.Exists(wel))
                {
                    //Play welcome sound
                    //playSimpleSound(wel);
                    SoundPlayer simpleSound = new SoundPlayer(wel);
                    simpleSound.Play();
                }
            }
            String weldur = Config.Read("weldur", "Audio");
            int weldur_i = Convert.ToInt32(weldur);
            timerView.Interval = (weldur_i * 1000) + 1; //add 1 second more
            timerView.Start();
        }
        
        private void ViewFrame(object sender, EventArgs arg)
        //Description   :   Show center camera stream and center box
        {
            if (MainCamera == null || !MainCamera.SessionOpen) return;

            if (MainCamera.IsLiveViewOn)
            {
                lock (LvLock)
                {
                    if (Evf_Bmp != null)
                    {
                        #region Setting LV area and Center Box
                        //Setting centerbox
                        w = Evf_Bmp.Width;
                        h = Evf_Bmp.Height;

                        //atur posisi centerbox
                        double pinggir, atas;
                        pinggir = ((w - sisi) / 2);
                        atas = ((h - sisi) / 2);
                        int dc = Convert.ToInt32(pinggir);
                        int cd = Convert.ToInt32(atas);
                        centerBox = new System.Drawing.Rectangle(dc, cd, sisi, sisi);
                        #endregion

                        #region Image Processing
                        Image<Bgr, Byte> imageCV = new Image<Bgr, byte>(Evf_Bmp); //Image Class from Emgu.CV
                        Mat imgOriginal = imageCV.Mat;  //Convert image to mat for image processing
                        CvInvoke.Rectangle(imgOriginal, centerBox, new Bgr(Color.Red).MCvScalar, 3);
                        #endregion

                        //Show Result
                        ibFront.Image = imgOriginal;
                    }//if Evf_bmp!=NULL
                }//lock
            }//MainCamera?.IsLiveViewOn
        }

        private void ProcessFrame(object sender, EventArgs arg)
        //Description   :   Show center camera stream and center box and do face detection
        {
            if (MainCamera == null || !MainCamera.SessionOpen) return;

            if (MainCamera.IsLiveViewOn)
            {
                lock (LvLock)
                {
                    if (Evf_Bmp != null)
                    {
                        #region Setting LV area and Center Box
                        //Setting centerbox
                        w = Evf_Bmp.Width;
                        h = Evf_Bmp.Height;

                        //atur posisi centerbox
                        double pinggir, atas;
                        pinggir = ((w - sisi) / 2);
                        atas = ((h - sisi) / 2);
                        int dc = Convert.ToInt32(pinggir);
                        int cd = Convert.ToInt32(atas);
                        centerBox = new System.Drawing.Rectangle(dc, cd, sisi, sisi);
                        #endregion

                        #region Image Processing
                        Image<Bgr, Byte> imageCV = new Image<Bgr, byte>(Evf_Bmp); //Image Class from Emgu.CV
                        Mat imgOriginal = imageCV.Mat;  //Convert image to mat for image processing
                        CvInvoke.Rectangle(imgOriginal, centerBox, new Bgr(Color.Red).MCvScalar, 3);
                        
                        Mat imgGrayscale = new Mat();
                        CvInvoke.CvtColor(imgOriginal, imgGrayscale, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);
                        
                        //Store face detected in facesDetected
                        System.Drawing.Rectangle[] facesDetected = face.DetectMultiScale(
                            imgGrayscale,
                            1.4,    //scale increase rate: 1.1(def) - 1.4, higher means running fewer scales, thus increase processing speed (but less accurate as it may skip some faces)
                            3,      //minimum neighbors threshold: 0 - 3(def) - n, numbers of rectangle needed to be accepted as face, higher means requirement is stricter, less likely to detect wrongly
                            new System.Drawing.Size(300, 300));   //minimum detection scale: smallest face we search for (RoT: 1/4 image width, 1:1 aspect ratio)
                            
                        if (facesDetected.Length == 0 ) { timerResetTolerate.Start(); inside = false; }

                        //For each face do
                        foreach (System.Drawing.Rectangle f in facesDetected)
                        {
                            //detected face ISN'T inside desired area
                            if (centerBox.Contains(f) == false)
                            {
                                inside = false;
                                
                                CvInvoke.Rectangle(imgOriginal, centerBox, new Bgr(Color.Red).MCvScalar, 3);
                                //string eks = f.X.ToString() + " a " + f.Y.ToString();
                                //Console.WriteLine(eks);

                                //Interrupt case: face is out of area while countdown to capture
                                if (timerRun == true)
                                {
                                    //Tolerate for predefined time
                                    timerResetTolerate.Start();
                                }
                            }
                            //detected face IS inside desired area
                            else if (centerBox.Contains(f) == true)
                            {
                                inside = true;
                                //CvInvoke.Rectangle(imgOriginal, f, new Bgr(Color.Red).MCvScalar, 3); //coloring face box
                                CvInvoke.Rectangle(imgOriginal, centerBox, new Bgr(Color.Blue).MCvScalar, 3); //coloring target box
                                //string eks = f.X.ToString() + " " + f.Y.ToString();
                                //Console.WriteLine(eks);

                                //Not counting? then countdown to capture
                                if (timerRun == false)
                                {
                                    timerRun = true;
                                    timer.Start();
                                    String hold = Config.Read("hold", "Audio");
                                    if (hold != "")
                                    {
                                        if (File.Exists(hold))
                                        {
                                            playSimpleSound(hold);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                inside = false;
                                CvInvoke.Rectangle(imgOriginal, centerBox, new Bgr(Color.Red).MCvScalar, 3); //coloring target box
                                //string eks = f.X.ToString() + " b " + f.Y.ToString();
                                //Console.WriteLine(eks);

                                //Interrupt case: face is out of area while countdown to capture
                                if (timerRun == true)
                                {
                                    //Tolerate for predefined time
                                    timerResetTolerate.Start();
                                }
                            }
                        }//foreach f in facesDetected
                        #endregion

                        //Show Result
                        ibFront.Image = imgOriginal;
                    }//if Evf_bmp!=NULL
                }//lock
            }//MainCamera?.IsLiveViewOn
        }

        private void timer_Tick(object sender, EventArgs e)
        //Description   :   Countdown to capture photo
        {
            //Still counting...
            if (timeLeft > 0)
            {
                timeLeft = timeLeft - 1;
                lbCountdown.Text = timeLeft.ToString();
            }
            //Done counting!
            else
            {
                //Reset countdown to capture
                timer.Stop();
                timerRun = false;
                timeLeft = 3;

                #region Taking photos
                //Freeze image processing
                if (MainCamera != null) { Application.Idle -= ProcessFrame; timer.Enabled = false; }
                
                try { LeftCamera.TakePhoto(); }
                catch (Exception ex) { ReportError(ex.Message, false); }
                
                try { RightCamera.TakePhoto(); }
                catch (Exception ex) { ReportError(ex.Message, false); }

                try { MainCamera.TakePhoto(); }
                catch (Exception ex) { ReportError(ex.Message, false); }

                //Play photo finish audio
                String load = Config.Read("load", "Audio");
                if (load != "")
                {
                    if (File.Exists(load))
                    {
                        playSimpleSound(load);
                    }
                }

                //Turn off the lamp
                try
                {
                    ardPort.WriteLine("1");
                    ardPort.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

                //Give delay then continue to crop and resize process
                timersave.Start();
                #endregion
            }
        }

        private void timerResetTolerate_Tick(object sender, EventArgs e)
        //Description   :   Tolerate for seconds before countdown reset IF face is outside area after entering
        {
            timerResetTolerate.Stop();

            //If face is still out of area after [tolerance] secs after entering
            if (inside == false)
            {
                //Reset countdown to capture
                timer.Stop();
                timerRun = false;
                timeLeft = 3;
                lbCountdown.Text = timeLeft.ToString();
            }
            //If face is back in the area, continue (don't reset)
            else { }
        }

        private void timersave_Tick(object sender, EventArgs e)
        //Description   :   Give 3 seconds break after taking photo for saving photos, preventing corrupted data, before resizing 
        {
            timersave.Stop();

            #region Crop and Resize
            //Load saved photos
            DirectoryInfo hdDirectoryInWhichToSearch = new DirectoryInfo(sap);
            //Local variables
            String imgsrcL, imgsrcR, imgsrcF;
            String filenameL, filenameR, filenameF;

            //Open Left camera result
            FileInfo[] filesInDirL = hdDirectoryInWhichToSearch.GetFiles("Left.jpg");
            try
            {
                foreach (FileInfo foundFile in filesInDirL)
                {
                    imgsrcL = foundFile.FullName;
                    filenameL = "aftrszL.jpg";
                    
                    //Crop image
                    Bitmap cropimgsrcL = new Bitmap(imgsrcL);
                    try
                    {
                        System.Drawing.Rectangle cropB = new System.Drawing.Rectangle((cropimgsrcL.Width - cropimgsrcL.Height) / 2, 0, cropimgsrcL.Height, cropimgsrcL.Height);
                        cropimgsrcL = cropimgsrcL.Clone(cropB, System.Drawing.Imaging.PixelFormat.DontCare);
                        cropimgsrcL.Save(sap + filenameL);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }

                    //Resize image
                    try
                    {
                        imgesize(sap + filenameL, imgout, ska, "2");
                        imgthumb(sap + filenameL, imgout, thum, "2");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                    
                    //Delete original image
                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
                    File.Delete(imgsrcL);
                    File.Delete(sap + filenameL);
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }

            //Open Right camera result
            FileInfo[] filesInDirR = hdDirectoryInWhichToSearch.GetFiles("Right.jpg");
            try
            {
                foreach (FileInfo foundFile in filesInDirR)
                {
                    imgsrcR = foundFile.FullName;
                    filenameR = "aftrcropR.jpg";
                    
                    //Crop image
                    Bitmap cropimgsrcR = new Bitmap(imgsrcR);
                    try
                    {
                        System.Drawing.Rectangle cropB = new System.Drawing.Rectangle((cropimgsrcR.Width - cropimgsrcR.Height) / 2, 0, cropimgsrcR.Height, cropimgsrcR.Height);
                        cropimgsrcR = cropimgsrcR.Clone(cropB, System.Drawing.Imaging.PixelFormat.DontCare);
                        cropimgsrcR.Save(sap + filenameR);
                        cropimgsrcR.Dispose();
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }

                    //Resize image
                    try
                    {
                        imgesize(sap + filenameR, imgout, ska, "3");
                        imgthumb(sap + filenameR, imgout, thum, "3");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                    
                    //Delete original image
                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
                    File.Delete(sap + filenameR);
                    File.Delete(imgsrcR);
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }

            //Open Front camera result
            FileInfo[] filesInDir = hdDirectoryInWhichToSearch.GetFiles( "Front.jpg" );
            try
            {
                foreach (FileInfo foundFile in filesInDir)
                {
                    imgsrcF = foundFile.FullName;
                    filenameF = "aftrsz.jpg";

                    //Crop image
                    Bitmap cropimgsrcF = new Bitmap(imgsrcF);
                    try
                    {
                        System.Drawing.Rectangle cropB = new System.Drawing.Rectangle((cropimgsrcF.Width - cropimgsrcF.Height) / 2, 0, cropimgsrcF.Height, cropimgsrcF.Height);
                        cropimgsrcF = cropimgsrcF.Clone(cropB, System.Drawing.Imaging.PixelFormat.DontCare);
                        cropimgsrcF.Save(sap + filenameF);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }

                    //Resize image
                    try
                    {
                        imgesize(sap + filenameF, imgout, ska, "1");
                        imgthumb(sap + filenameF, imgout, thum, "1");
                    }
                    catch (Exception ex){ MessageBox.Show(ex.ToString()); }
                    
                    //Delete original image
                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
                    File.Delete(imgsrcF);
                    File.Delete(sap + filenameF);
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
            #endregion
            
            //Review Page
            int review;
            Int32.TryParse(Config.Read("review", "General"), out review);

            if (review == 1)
            //Review page turned on
            {
                //Close camera session
                try
                {
                    if (LeftCamera?.SessionOpen == true) { CloseLeftCamSession(); }
                    if (RightCamera?.SessionOpen == true) { CloseRightCamSession(); }
                    if (MainCamera?.SessionOpen == true) { CloseMainCamSession(); }
                }
                catch { MessageBox.Show("Already closed?"); }

                //Open Review form
                this.Hide();
                Review fo3 = new Review();
                fo3.Show();
            }

            else if (review == 0)
            //Review page off
            //NO ONE SHOULD COME HERE (except they change the system flow...)
            {
                //Play finish audio
                String finish = Config.Read("finish", "Audio");
                if (finish != "")
                {
                    if (File.Exists(finish))
                    {
                        playSimpleSound(finish);
                    }
                }

                //Close camera session
                try
                {
                    if (LeftCamera?.SessionOpen == true) { CloseLeftCamSession(); }
                    if (RightCamera?.SessionOpen == true) { CloseRightCamSession(); }
                    if (MainCamera?.SessionOpen == true) { CloseMainCamSession(); }
                }
                catch { MessageBox.Show("Already closed?"); }

                //Upload to database
                using (WebClient client = new WebClient())
                {
                    client.Credentials = new NetworkCredential(Barcode.dbu, Barcode.dbp);
                    //string tuju = "ftp://"+hostid.Text + "//" + sFN;
                    string[] asd = Directory.GetFiles(sap, "*.*");
                    foreach (string file in asd)
                    {
                        //string tuju = "ftp://" + hostid.Text + "/" + Path.GetFileName(file);
                        string tuju = uppath + "/" + Path.GetFileName(file);
                        client.UploadFile(tuju, "STOR", file);
                        Saveimg(Barcode.cid, Barcode.brid, Path.GetFileName(file), Barcode.upsynch, Barcode.regid);
                        //ftpClient.upload(uploadPath + "/" + Path.GetFileName(file), file);
                    }
                }
                
                //Finished, return to Barcode form
                System.Diagnostics.Process.Start("admopn.exe");
                Application.Exit();
            }
        }

        private void Saveimg(string cli, string bra, string filename, string upsynch, string regid)
        //Description   :   Save image to databse with specified path
        //Input         :   clientID,branchID,filename,updateSynch, and regID from database
        {
            string querynew = "INSERT INTO file(`clientID`, `branchID`, `filename`, `updateSynch`,`regID`) VALUES (" + cli + ", '" + bra + "', '" + filename + "', '" + upsynch + "', '" + regid + "')";
            //string queryup = "UPDATE `user` SET `nd`='" + textBox1.Text + "',`nb`='" + textBox2.Text + "',`alm`='" + textBox3.Text + "' WHERE id = " + rid.Text;

            MySqlConnection databaseConnection = new MySqlConnection(Barcode.constring);
            MySqlCommand commandDatabase = new MySqlCommand(querynew, databaseConnection);

            try
            {
                databaseConnection.Open();
                MySqlDataReader myReader = commandDatabase.ExecuteReader();
                databaseConnection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        
        #region API Events
        //EDSDK's API events
        private void APIHandler_CameraAdded(CanonAPI sender)
        {
            try { Invoke((Action)delegate { RefreshCamera(); }); }
            catch (Exception ex) { ReportError(ex.Message, false); }
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
                ibFront.Invalidate();
            }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }
        private void MainCamera_StateChanged(EOSDigital.API.Camera sender, StateEventID eventID, int parameter)
        {
            try { if (eventID == StateEventID.Shutdown && IsInit) { Invoke((Action)delegate { CloseMainCamSession(); }); } }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }
        private void MainCamera_DownloadReady(EOSDigital.API.Camera sender, DownloadInfo Info)
        {
            try
            {
                if (Directory.Exists(sap) == false)
                {
                    DirectoryInfo di = Directory.CreateDirectory(sap);
                }

                string dir = null;
                Invoke((Action)delegate { dir = sap; });
                Info.FileName = "Front.jpg";
                sender.DownloadFile(Info, dir);
            }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }

        private void LeftCamera_DownloadReady(EOSDigital.API.Camera sender, DownloadInfo Info)
        {
            try
            {
                if (Directory.Exists(sap) == false)
                {
                    DirectoryInfo di = Directory.CreateDirectory(sap);
                }

                string dir = null;
                Invoke((Action)delegate { dir = sap; });
                Info.FileName = "Left.jpg";
                sender.DownloadFile(Info, dir);
            }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }
        private void LeftCamera_StateChanged(EOSDigital.API.Camera sender, StateEventID eventID, int parameter)
        {
            try { if (eventID == StateEventID.Shutdown && IsInit) { Invoke((Action)delegate { CloseLeftCamSession(); }); } }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }

        private void RightCamera_DownloadReady(EOSDigital.API.Camera sender, DownloadInfo Info)
        {
            try
            {
                if (Directory.Exists(sap) == false)
                {
                    DirectoryInfo di = Directory.CreateDirectory(sap);
                }

                string dir = null;
                Invoke((Action)delegate { dir = sap; });
                Info.FileName = "Right.jpg";
                sender.DownloadFile(Info, dir);
            }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }
        private void RightCamera_StateChanged(EOSDigital.API.Camera sender, StateEventID eventID, int parameter)
        {
            try { if (eventID == StateEventID.Shutdown && IsInit) { Invoke((Action)delegate { CloseRightCamSession(); }); } }
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
        #endregion

        #region Subroutines : Sound
        private void playSimpleSound(string a)
        //Description   :   play .wav sound file, alongside other processes
        //Input         :   .wav file name
        {
            SoundPlayer simpleSound = new SoundPlayer(a);
            simpleSound.Play();
        }

        private void playSyncSound(string a)
        //Description   :   play .wav sound file, other processes are halted
        //Input         :   .wav file name
        {
            SoundPlayer SyncSound = new SoundPlayer(a);
            SyncSound.PlaySync();
        }
        #endregion

        #region Subroutines : Resize
        public void imgesize(string imageFile, string outputFile, double scaleFactor, string posi)
        //Description   :   Crop image and then resize image resolution with scale factor
        //Input         :   Original image and scale factor
        //Output        :   Resized image
        {
            using (var srcImage = Image.FromFile(imageFile))
            {
                var newWidth = (int)(srcImage.Width * scaleFactor);
                var newHeight = (int)(srcImage.Height * scaleFactor);
                using (var newImage = new Bitmap(newWidth, newHeight))
                using (var graphics = Graphics.FromImage(newImage))
                {
                    //graphics.SmoothingMode;
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    graphics.DrawImage(srcImage, new System.Drawing.Rectangle(0, 0, newWidth, newHeight));

                    outputFile = sap + Barcode.filename+ "_0" +posi + "r.jpg";
                    imgout = outputFile;
                    newImage.Save(outputFile, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
            }
        }
        public void imgthumb(string imageFile, string outputFile, double scaleFactor, string posi)
        //Description   :   Crop image and then resize image resolution with scale factor
        //Input         :   Original image and scale factor
        //Output        :   Resized image
        {
            using (var srcImage = Image.FromFile(imageFile))
            {
                var newWidth = (int)(srcImage.Width * scaleFactor);
                var newHeight = (int)(srcImage.Height * scaleFactor);
                using (var newImage = new Bitmap(newWidth, newHeight))
                using (var graphics = Graphics.FromImage(newImage))
                {
                    //graphics.SmoothingMode;
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    graphics.DrawImage(srcImage, new System.Drawing.Rectangle(0, 0, newWidth, newHeight));

                    outputFile = sap + Barcode.filename+ "_0" + posi + "s.jpg";
                    imgout = outputFile;
                    newImage.Save(outputFile, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
            }

        }
        #endregion

        #region Subroutines : Camera
        private void RefreshCamera()
        //Description   :   Get list of connected cameras
        {
            CamList = APIHandler.GetCameraList();
        }

        private void OpenMainCamSession()
        //Description   :   Open MainCamera session
        {
            int cam;
            Int32.TryParse(Config.Read("center", "Camera"), out cam);
            if (CamList.Count > 0)
            {
                MainCamera = CamList[cam];
                MainCamera.OpenSession();
                MainCamera.StartLiveView();
                MainCamera.LiveViewUpdated += MainCamera_LiveViewUpdated;
                MainCamera.StateChanged += MainCamera_StateChanged;
                MainCamera.DownloadReady += MainCamera_DownloadReady;

                MainCamera.SetSetting(PropertyID.SaveTo, (int)SaveTo.Host);
                MainCamera.SetCapacity(4096, int.MaxValue);
            }
            else
            {
                MessageBox.Show("Camera not connected\nPlease connect and try again", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //Terminate background process
                if (System.Windows.Forms.Application.MessageLoop) { System.Windows.Forms.Application.Exit(); }
                else { System.Environment.Exit(1); }
            }
        }
        private void CloseMainCamSession()
        //Description   :   Close MainCamera session
        {
            MainCamera.StopLiveView();
            MainCamera.CloseSession();
        }

        private void OpenRightCamSession()
        //Description   :   Open RightCamera session
        {
            int cam;
            Int32.TryParse(Config.Read("right", "Camera"), out cam);
            if (CamList.Count > 0)
            {
                RightCamera = CamList[cam];
                RightCamera.OpenSession();
                RightCamera.StateChanged += RightCamera_StateChanged;
                RightCamera.DownloadReady += RightCamera_DownloadReady;

                RightCamera.SetSetting(PropertyID.SaveTo, (int)SaveTo.Host);
                RightCamera.SetCapacity(4096, int.MaxValue);
            }
            else
            {
                MessageBox.Show("Camera not connected\nPlease connect and try again", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //Terminate background process
                if (System.Windows.Forms.Application.MessageLoop) { System.Windows.Forms.Application.Exit(); }
                else { System.Environment.Exit(1); }
            }
        }

        private void timerView_Tick(object sender, EventArgs e)
        //Description   :   Start face detection after welcome audio ends
        {
            timerView.Stop();
            Application.Idle += ProcessFrame;
        }

        private void CloseRightCamSession()
        //Description   :   Close RightCamera session
        {
            RightCamera.CloseSession();
        }

        private void OpenLeftCamSession()
        //Description   :   Open LeftCamera session
        {
            int cam;
            Int32.TryParse(Config.Read("left", "Camera"), out cam);
            if (CamList.Count > 0)
            {
                LeftCamera = CamList[cam];
                LeftCamera.OpenSession();
                LeftCamera.StateChanged += LeftCamera_StateChanged;
                LeftCamera.DownloadReady += LeftCamera_DownloadReady;

                LeftCamera.SetSetting(PropertyID.SaveTo, (int)SaveTo.Host);
                LeftCamera.SetCapacity(4096, int.MaxValue);
            }
            else
            {
                MessageBox.Show("Camera not connected\nPlease connect and try again", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //Terminate background process
                if (System.Windows.Forms.Application.MessageLoop) { System.Windows.Forms.Application.Exit(); }
                else { System.Environment.Exit(1); }
            }
        }
        private void CloseLeftCamSession()
        //Description   :   Close LeftCamera session
        {
            LeftCamera.CloseSession();
        }
        
        private void ReportError(string message, bool lockdown)
        //Description   :   Show error message according to numbers of error(s)
        {
            int errc;
            lock (ErrLock) { errc = ++ErrCount; }

            //if (errc < 4) MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //else if (errc == 4) MessageBox.Show("Many errors happened!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            lock (ErrLock) { ErrCount--; }
        }
        #endregion
        
        private void Camera_FormClosing(object sender, FormClosingEventArgs e)
        //Description   :   Close all cameras session and terminate background when form is closed
        {
            //Close DSLR - enter standby mode
            MainCamera?.Dispose();
            RightCamera?.Dispose();
            APIHandler?.Dispose();
            IsInit = false;

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

        private void lbCountdown_Click(object sender, EventArgs e)
        //Description   :   Manually take photo
        {
            /*
            OpenLeftCamSession();
            try { LeftCamera.TakePhoto(); }
            catch (Exception ex) { ReportError(ex.Message, false); }
            */

            /*
            OpenRightCamSession();
            try { RightCamera.TakePhotoShutterAsync(); }
            catch (Exception ex) { ReportError(ex.Message, false); }
            */
            
            try { MainCamera.TakePhoto(); }
            catch (Exception ex) { ReportError(ex.Message, false); }
            
            //Give 3 seconds halt
            timersave.Start();
        }

        public static String GetTimeStamp(DateTime value)
        //Description   :   Count timestamp
        {
            return value.ToString("HHmmssffff");
        }
    }
}