/*
 * PHOTOBOOTH DERMIES.EXE - CCamera
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

//Program flag
#if DEBUG
  //#define DEBUG_MAINCAM
  #define DEBUG_MULTIPLECAM
#endif

//Only one mode can active at a time
//#define CONSECUTIVE
#define SIMULTANEOUS

#if SIMULTANEOUS
  #define _SIMULTANEOUS
#endif //SIMULTANEOUS

//Basic reference
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
//Include reference for switching btw forms
using System.Linq;
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
using System.ComponentModel;
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
    Bitmap Evf_Bmp; //input stream
        
    bool IsInit    = false;
    int w          = 0; 
    int h          = 0;   //screen w&h
    int ErrCount   = 0;
    double ska     = 0;
    double thum    = 0.04;  //resize factor
    object ErrLock = new object();  //error lock
    object LvLock  = new object();   //liveview lock

	  public static string imgsrc,sap,dire,fold,uppath;  //upload variables, STATIC so it can be accessed from other class without losing value
	  
  #if _SIMULTANEOUS
    DownloadInfo diMain, diLeft, diRight;
  #endif //_SIMULTANEOUS

  #if CONSECUTIVE
    char whichCam;
  #endif //CONSECUTIVE
	  static readonly object _locker     = new object();
	  static EventWaitHandle _takeSignal = new AutoResetEvent(false);
    //WAIT variable
  #if RELEASE
    static CountdownEvent cdCameraDL      = new CountdownEvent(3);
    static CountdownEvent cdCameraDLReady = new CountdownEvent(3);
  #elif DEBUG
    static CountdownEvent cdCameraDL      = new CountdownEvent(2);
    static CountdownEvent cdCameraDLReady = new CountdownEvent(2);
  #endif
		
	  //Settings variable
	  IniFile Config = new IniFile("Settings.ini");       //config filename

    //Face detection variable
    CascadeClassifier face = new CascadeClassifier("haarcascade_frontalface_default.xml");  //face library filename
    int  timeLeft = 3;          //countdown variable
    bool timerRun = false;      //is countdown running?
    bool inside   = false;      //is face inside desired area?
    int  sisi;                  //centerbox size

    //Processing variable
    int processTimeoutCount = 5;

	  //Lamp variable
	  bool isLampOn = false;
    SerialPort ardPort;

    //CSV variable
    long iRightCamTake, iRightCamDL, iRightCamStartProcessing, iRightCamEndProcessing;
    long iLeftCamTake, iLeftCamDL, iLeftCamStartProcessing, iLeftCamEndProcessing;
    long iMainCamTake, iMainCamDL, iMainCamStartProcessing, iMainCamEndProcessing;
	  string logTime = "";
  #endregion
		
	  public Camera()
    {
	    ////////////////////////////////////////////////////////////////////
	    //Try to establish connection with camera
      try
      {
        InitializeComponent();
        //Emgu CV
        CvInvoke.UseOpenCL = false;
        //Create Canon API Object
        APIHandler = new CanonAPI();
        //Subscribe to events
        APIHandler.CameraAdded              += APIHandler_CameraAdded;
        ErrorHandler.SevereErrorHappened    += ErrorHandler_SevereErrorHappened;
        ErrorHandler.NonSevereErrorHappened += ErrorHandler_NonSevereErrorHappened;
        
        //Open camera session here so when the form load, the image stream is ready
        //First, get camera list & evaluate camera count
        RefreshCamera();

        //Then open their session
      #if RELEASE
        if (MainCamera?.SessionOpen == true)
          CloseMainCamSession();
        OpenMainCamSession();

        if (RightCamera?.SessionOpen == true)
          CloseRightCamSession();
        OpenRightCamSession();

        if (LeftCamera?.SessionOpen == true)
          CloseLeftCamSession();
        OpenLeftCamSession();
      #elif DEBUG_MAINCAM
        if (MainCamera?.SessionOpen == true)
          CloseMainCamSession();
        OpenMainCamSession();
      #elif DEBUG_MULTIPLECAM
		    if (MainCamera?.SessionOpen == true)
          CloseMainCamSession();
			  OpenMainCamSession();

		    if (RightCamera?.SessionOpen == true)
          CloseRightCamSession();
			  OpenRightCamSession();
      #endif

        //Set flag initiation complete
        IsInit = true;

        //Initialize logging vars
        iRightCamTake = iRightCamEndProcessing = iRightCamStartProcessing = iRightCamTake = 0;
        iLeftCamDL = iLeftCamEndProcessing = iLeftCamStartProcessing = iLeftCamTake = 0;
        iMainCamDL = iMainCamEndProcessing = iMainCamStartProcessing = iMainCamTake = 0;
      }
      catch (Exception ex) 
      {
        Load += (s,e) => Close(); //anonymous event that fires at Load, preventing it to continue
        string str = "Tidak semua kamera siap digunakan, mohon hubungi petugas\n\nCek kembali power atau koneksi kamera lewat program Settings.\n\n" + ex.Message;
        ReportError(str,true);
        //Terminate background process
        if (System.Windows.Forms.Application.MessageLoop) 
          System.Windows.Forms.Application.Exit();
        else 
          System.Environment.Exit(1);
      }
    }

    private void Form1_Load(object sender, EventArgs e)
    //Description   :  when Camera form load, do:
    //                 connect to lamp
    //                 get several parameters from settings file
    //                 show view frame while playing welcome audio
    {   
      /////////////////////////////////////////////////////////////////////
      // Get shot&save parameters from .ini file
      // Get centerbox size
      string temp = Config.Read("censize", "General");
      if (temp != "")
			  Int32.TryParse(temp, out sisi);
      else
			  sisi = ibFront.Height; //default size

      // Get time tolerate interval
      int temptimer;
      Int32.TryParse(Config.Read("tolerance", "General"), out temptimer);
      if (temptimer != 0)
			  timerResetTolerate.Interval = temptimer;
      else
			  temptimer = 1000; //default tolerance time

      //Get scale factor for resizing
      string skal = Config.Read("scalf", "General");
      ska = Convert.ToDouble(skal) * 0.1; //because it is saved in one digit format (not float with one decimal value behind)

      //Get value from Barcode form, as folder name include patient MRNL
      fold = Barcode.regid;
      uppath = Config.Read("directory", "General");
      dire = @"D:\dermiestemp";
      if (!Directory.Exists(dire))
        dire = @"C:\dermiestemp";
      sap = dire + "\\" + fold + "\\" + Barcode.upsynch + "\\";

    #if RELEASE
	    /////////////////////////////////////////////////////////////////////
	    // Establish connection with lamp, and turn it on
	    if (!isLampOn)
	    {
        // Get serial port chosen
			  String port = Config.Read("COM", "General");
			  ardPort = new SerialPort();
			  ardPort.BaudRate = 9600;
			  ardPort.PortName = port;

		    try
		    {
			    ardPort.Open();
			    ardPort.WriteLine("1");
			    isLampOn = true;
		    }
		    catch (Exception ex)
		    {
			    MessageBox.Show("Lampu tidak bisa dinyalakan, mohon hubungi petugas. Tes lampu melalui program Settings\n" + ex.ToString(), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
			    //Close program because taking photo without lamp on is useless
			    if (System.Windows.Forms.Application.MessageLoop)
				    System.Windows.Forms.Application.Exit();
			    else
				    System.Environment.Exit(1);
		    }
	    }
    #endif

      //Log current session details
      logTime = GetTimeStamp(DateTime.Now);	//Time
      
		  //Show ViewFrame and play audio
		  //After audio finished, add ProccessFrame for face detection
		  Application.Idle += ViewFrame;

      // Play welcome sound
      String wel = Config.Read("wel", "Audio");
      if (wel != "")
      {
        if (File.Exists(wel))
          playSimpleSound(wel); //Play welcome sound
      }

      // Will run ViewFrame for welcome-audio-duration + 1 sec
      String weldur = Config.Read("weldur", "Audio");
      int weldur_i = Convert.ToInt32(weldur);
      timerView.Interval = (weldur_i * 1000) + 1; //add 1 second more
      timerView.Start();

      // Set timersave interval (wait for download to finish)
      timersave.Interval = 100; //because we already wait with countdown.
    }
        
    private void ViewFrame(object sender, EventArgs arg)
    //Description : Show center camera stream and center box
		//					    ProcessFrame WITHOUT face detection
    {
      if (MainCamera == null || !MainCamera.SessionOpen)
      {
        Application.Idle -= ViewFrame; //unsubscribe itself
        string str = "View frame - Kamera tengah tidak siap untuk digunakan, mohon hubungi petugas\n\nCek kembali power atau koneksi kamera tengah lewat program Settings.\n\n";
        ReportError(str,true);
        //Terminate background process
        if (System.Windows.Forms.Application.MessageLoop) 
          System.Windows.Forms.Application.Exit();
        else 
          System.Environment.Exit(1);
      }

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
            System.Drawing.Rectangle centerBox = new System.Drawing.Rectangle(dc, cd, sisi, sisi);
          #endregion
						
						//Convert image stream
					  Image<Bgr, Byte> imageCV = new Image<Bgr, byte>(Evf_Bmp); //Image Class from Emgu.CV
						Mat imgOriginal	         = imageCV.Mat;
            CvInvoke.Rectangle(imgOriginal, centerBox, new Bgr(Color.Red).MCvScalar, 3);

            //Show Result
            ibFront.Image = imgOriginal;
          }//if Evf_bmp!=NULL
        }//lock
      }//MainCamera?.IsLiveViewOn
    }
        
  private void timerView_Tick(object sender, EventArgs e)
  //Description   :   Start face detection after welcome audio ends
  {
      timerView.Stop();
		  Application.Idle -= ViewFrame;
      Application.Idle += ProcessFrame;
  }

  private void ProcessFrame(object sender, EventArgs arg)
  //Description   :   Show center camera stream and center box and do face detection
  {
    if (MainCamera == null || !MainCamera.SessionOpen)
    {
      Application.Idle -= ProcessFrame; //unsubscribe itself
      string str = "Proccess Frame - Kamera tengah tidak siap untuk digunakan, mohon hubungi petugas\n\nCek kembali power atau koneksi kamera tengah lewat program Settings.\n\n";
      ReportError(str,true);
      //Terminate background process
      if (System.Windows.Forms.Application.MessageLoop) 
        System.Windows.Forms.Application.Exit();
      else 
        System.Environment.Exit(1);
    }

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
          System.Drawing.Rectangle centerBox = new System.Drawing.Rectangle(dc, cd, sisi, sisi);
        #endregion

        #region Image Processing
          Image<Bgr, Byte> imageCV = new Image<Bgr, byte>(Evf_Bmp); //Image Class from Emgu.CV
          Mat imgOriginal          = imageCV.Mat;  //Convert image to mat for image processing
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
              CvInvoke.Rectangle(imgOriginal, centerBox, new Bgr(Color.Blue).MCvScalar, 3); //coloring target box

              //Not counting? then countdown to capture
              if (timerRun == false)
              {
                timerRun = true;
                timer.Start();
                String hold = Config.Read("hold", "Audio");
                if (hold != "")
                  if (File.Exists(hold))
                    playSimpleSound(hold);
              }
            }
            else
            {
              inside = false;
              CvInvoke.Rectangle(imgOriginal, centerBox, new Bgr(Color.Red).MCvScalar, 3); //coloring target box

              //Interrupt case: face is out of area while countdown to capture
              if (timerRun == true)
                timerResetTolerate.Start(); //Tolerate for predefined time
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
      //Reset countdown parameter
      timer.Stop();
      timerRun = false;
      timeLeft = 3;

    #region Taking photos
    //Stop video stream & image processing
		if (MainCamera != null)
    {
      Application.Idle -= ProcessFrame;
      timer.Enabled = false;
    }

		  //Take photo L -> M -> R (Right must always taken last! 23-12-2017)
    #if SIMULTANEOUS
      #if RELEASE
        try 
        { 
          LeftCamera.TakePhoto(); 
          iLeftCamTake = DateTime.Now.Ticks;
        }
        catch (Exception ex) { ReportError(ex.Message, false); }
      #endif

      try 
      { 
        MainCamera.TakePhoto(); 
        iMainCamTake = DateTime.Now.Ticks;
      }
      catch (Exception ex) { ReportError(ex.Message, false); }

      try 
      { 
        RightCamera.TakePhoto();
        iRightCamTake = DateTime.Now.Ticks;
      }
      catch (Exception ex) { ReportError(ex.Message, false); }

      #if _SIMULTANEOUS
        //Wait until receive signal download ready from each camera
        cdCameraDLReady.Wait();
        //Download in serial
        LeftCamera.DownloadFile(diLeft, sap);
        MainCamera.DownloadFile(diMain, sap);
        RightCamera.DownloadFile(diRight, sap);
      #endif //_SIMULTANEOUS
      //Wait until receieve signal from each camera's downloadprogress
      cdCameraDL.Wait();

      //Play photo finish audio
			String load = Config.Read("load", "Audio");
			if (load != "")
			{
				if (File.Exists(load))
				{
					playSyncSound(load);
				}
			}
      //Start countdown for save image. This give camera some times to complete downloading the image
      timersave.Start();
    #elif CONSECUTIVE
		  Thread t = new Thread(() => TakePhoto());
		  t.Start();

      //Send info to take photo thread
      whichCam = 'm';
		  _takeSignal.Set();
    #endif
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

#region Crop and Resize
	private void timersave_Tick(object sender, EventArgs e)
  //Description   :   Give 3 seconds break after taking photo for saving photos, preventing corrupted data, before resizing  (OLD)
  {
    timersave.Stop();

		//Load saved photos
		DirectoryInfo hdDirectoryInWhichToSearch = new DirectoryInfo(sap);
#if RELEASE
		//Open Left camera result
		FileInfo[] filesInDirL = hdDirectoryInWhichToSearch.GetFiles("Left.jpg");
		Thread threadLeft = new Thread(() => ProccessPhotoL(filesInDirL))
		{
			Name = "ProccessPhotoLeft"
		};			
		//Open Right camera result
		FileInfo[] filesInDirR = hdDirectoryInWhichToSearch.GetFiles("Right.jpg");
		Thread threadRight = new Thread(() => ProccessPhotoR(filesInDirR))
		{
			Name = "ProccessPhotoRight"
		};

		//Open Front camera result
		FileInfo[] filesInDir = hdDirectoryInWhichToSearch.GetFiles("Front.jpg");
		Thread threadMain = new Thread(() => ProccessPhotoM(filesInDir))
		{
			Name = "ProccessPhotoMain"
		};

		//Start processing three camera results simultaneously
		threadLeft.Start();
		threadRight.Start();
		threadMain.Start();

		//Wait with timeout
    int timeout = processTimeoutCount * 1000 * 2; //double the delete file timeout
		bool isLeftFinished		= threadLeft.Join(timeout);
		bool isRightFinished	= threadRight.Join(timeout);
	  bool isMainFinished		= threadMain.Join(timeout);

		if (!isLeftFinished)
		{
			threadLeft.Abort();
		}
		if (!isRightFinished)
		{
			threadRight.Abort();
		}
		if (!isMainFinished)
		{
			threadMain.Abort();
		}
#elif DEBUG_MAINCAM
    //Open Front camera result
		FileInfo[] filesInDir = hdDirectoryInWhichToSearch.GetFiles("Front.jpg");
		Thread threadMain = new Thread(() => ProccessPhotoM(filesInDir))
		{
			Name = "ProccessPhotoMain"
		};

    //Start processing three camera results simultaneously
		threadMain.Start();
    int timeout = processTimeoutCount * 1000 * 2; //double the delete file timeout
    bool isMainFinished		= threadMain.Join(timeout);

    if (!isMainFinished)
		{
			threadMain.Abort();
		}
#elif DEBUG_MULTIPLECAM
    //Open Right camera result
    FileInfo[] filesInDirR = hdDirectoryInWhichToSearch.GetFiles("Right.jpg");
		Thread threadRight = new Thread(() => ProccessPhotoR(filesInDirR))
		{
		  Name = "ProccessPhotoRight"
		};
			
		//Open Front camera result
		FileInfo[] filesInDir = hdDirectoryInWhichToSearch.GetFiles("Front.jpg");
		Thread threadMain = new Thread(() => ProccessPhotoM(filesInDir))
		{
		  Name = "ProccessPhotoMain"
		};

		//Start processing three camera results simultaneously
		threadRight.Start();
		threadMain.Start();

		//Wait until all process finished
    int timeout = processTimeoutCount * 1000 * 2; //double the delete file timeout
		bool isRightFinished = threadRight.Join(timeout);
		bool isMainFinished = threadMain.Join(timeout);
    if(!isRightFinished)
      threadRight.Abort();
    if (!isMainFinished)
			threadMain.Abort();
  #endif

    //Calculate log
    String sLeftLog  = CalculateLog(iLeftCamTake, iLeftCamDL, iLeftCamStartProcessing, iLeftCamEndProcessing);
    String sMainLog  = CalculateLog(iMainCamTake, iMainCamDL, iMainCamStartProcessing, iMainCamEndProcessing); 
    String sRightLog = CalculateLog(iRightCamTake, iRightCamDL, iRightCamStartProcessing, iRightCamEndProcessing); 

    //Log to CSV
    string CSVpath = dire + "\\DermiesLog" + Barcode.tanggal + ".csv";
		if (!File.Exists(CSVpath))
		{
			string CSVheader = "Time" + " " +
				"Cam Left : Download - Take" + "." + "Cam Left: Proc Start - Take" + "." + "Cam Left: Proc End - Proc Start" + "." +
				"Cam Main : Download - Take" + "." + "Cam Main: Proc Start - Take" + "." + "Cam Main: Proc End - Proc Start" + "." +
				"Cam Right: Download - Take" + "." + "Cam Right: Proc Start- Take" + "." + "Cam Right: Proc End- Proc Start";
      File.AppendAllText(CSVpath, CSVheader);
		}
		string CSVinput = Environment.NewLine + logTime + "." + sLeftLog + "." + sMainLog + "." + sRightLog;
		File.AppendAllText(CSVpath, CSVinput);
    
    //Turn off the lamp
  #if RELEASE
		if(isLampOn)
		{
			try
			{
				ardPort.WriteLine("1");
				ardPort.Close();
				isLampOn = false;
			}
			catch (Exception ex)
			{
			  MessageBox.Show("Program error, mohon hubungi petugas\n\nLampu tidak terhubung\n" + ex.ToString(), "ERROR CODE: 2", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //Program still continue, but expect employee to turn off the lamp with Settings program (hence reset the state)
			}
		}
  #endif

    //Cleaning up
		//Close camera session
  #if RELEASE
    if(MainCamera?.SessionOpen == true)
      MainCamera.CloseSession();
    if(LeftCamera?.SessionOpen == true)
      LeftCamera.CloseSession();
    if(RightCamera?.SessionOpen == true)
      RightCamera.CloseSession();
  #elif DEBUG_MAINCAM
    if(MainCamera?.SessionOpen == true)
        CloseMainCamSession();
  #elif DEBUG_MULTIPLECAM
    if(MainCamera?.SessionOpen == true)
        CloseMainCamSession();
    if(RightCamera?.SessionOpen == true)
        CloseRightCamSession();
  #endif

		//Open Review form
		this.Hide();
    Review fo3 = new Review();
    fo3.Show();
  }

	private void ProccessPhotoL(FileInfo[] filesInDirL)
	{
		foreach (FileInfo foundFile in filesInDirL)
		{
			if (foundFile.Length == 0)
			{
				iLeftCamStartProcessing = iLeftCamEndProcessing = 0;
				return;
			}
			else //foundFile is not null
			{
				iLeftCamStartProcessing = DateTime.Now.Ticks;
				String imgsrcL = foundFile.FullName; //Save path to original image (aka Left.jpg)
  
				try
				{
					using (Bitmap cropimgsrcL = new Bitmap(imgsrcL))
					{
            Bitmap croppedImage;
            //1st step - crop
						System.Drawing.Rectangle cropSize = new System.Drawing.Rectangle((cropimgsrcL.Width - cropimgsrcL.Height) / 2, 0, cropimgsrcL.Height, cropimgsrcL.Height);
						croppedImage = new Bitmap(cropimgsrcL.Width, cropimgsrcL.Height);
						croppedImage = cropimgsrcL.Clone(cropSize, System.Drawing.Imaging.PixelFormat.DontCare);

            //2nd step - resize
            ResizeImage(croppedImage, ska, "2", 0); //Large
            ResizeImage(croppedImage, thum, "2", 1); //Thumb
					}
          //Check file status. Put outside using as using lock the resource (image file)
          int count = 0;
				  while (count < processTimeoutCount)
          {
            if(IsFileLocked(foundFile))
						  Thread.Sleep(1000);
            else
            {
              //Delete original image
					    System.GC.Collect();
					    System.GC.WaitForPendingFinalizers();
					    File.Delete(imgsrcL);
              break;
            }
            count++;
          }
				  iLeftCamEndProcessing = DateTime.Now.Ticks;
				}
				catch (Exception ex)
				{
					MessageBox.Show("Pemrosesan foto "+ foundFile +" gagal dilakukan, mohon hubungi petugas\n\n" + ex.ToString(), "ERROR CODE: 3L", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}
	}

	private void ProccessPhotoR(FileInfo[] filesInDirR)
	{
		foreach (FileInfo foundFile in filesInDirR)
		{
			if (foundFile.Length == 0)
			{
				iRightCamStartProcessing = iRightCamEndProcessing = 0;
				return;
			}
			else //foundFile is not null
			{
				iRightCamStartProcessing = DateTime.Now.Ticks;
				String imgsrcR = foundFile.FullName; //Save path to original image (aka Right.jpg)
        try
        {
          using (Bitmap cropimgsrcR = new Bitmap(imgsrcR))
          {
            //1st step - crop
            System.Drawing.Rectangle cropSize = new System.Drawing.Rectangle((cropimgsrcR.Width - cropimgsrcR.Height) / 2, 0, cropimgsrcR.Height, cropimgsrcR.Height);
						Bitmap croppedImage = new Bitmap(cropimgsrcR.Width, cropimgsrcR.Height);
						croppedImage = cropimgsrcR.Clone(cropSize, System.Drawing.Imaging.PixelFormat.DontCare);

            //2nd step - resize
            ResizeImage(croppedImage, ska, "3", 0); //Large
            ResizeImage(croppedImage, thum, "3", 1); //Thumb                
          }

          //Delete original image
          int count = 0;
          while (count < processTimeoutCount)
          {
            if (IsFileLocked(foundFile))
              Thread.Sleep(1000);
            else
            {
              //Delete original image
					    System.GC.Collect();
					    System.GC.WaitForPendingFinalizers();
					    File.Delete(imgsrcR);
              break;
            }
            count++;
          }
        
          iRightCamEndProcessing = DateTime.Now.Ticks;
        }
        catch (Exception ex)
        {
					MessageBox.Show("Pemrosesan foto "+ foundFile +" gagal dilakukan, mohon hubungi petugas\n\n" + ex.ToString(), "ERROR CODE: 3R", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
			}
		}
	}
		
	private void ProccessPhotoM(FileInfo[] filesInDir)
	{
		foreach (FileInfo foundFile in filesInDir)
		{
			if (foundFile.Length==0)
			{
        iMainCamStartProcessing = iMainCamEndProcessing = 0;
				return;
			}
			else
			{
        iMainCamStartProcessing = DateTime.Now.Ticks;
				String imgsrcF = foundFile.FullName;        
				try
				{
					using (Bitmap cropimgsrcF = new Bitmap(imgsrcF))
					{
            //1st step - crop
						System.Drawing.Rectangle cropSize = new System.Drawing.Rectangle((cropimgsrcF.Width - cropimgsrcF.Height) / 2, 0, cropimgsrcF.Height, cropimgsrcF.Height);
						Bitmap croppedImage = new Bitmap(cropimgsrcF.Width, cropimgsrcF.Height);
						croppedImage = cropimgsrcF.Clone(cropSize, System.Drawing.Imaging.PixelFormat.DontCare);

            //2nd step - resize
            ResizeImage(croppedImage, ska, "1", 0); //Large
            ResizeImage(croppedImage, thum, "1", 1); //Thumb
					}

          //Delete original image
          int count = 0;
          while(count < processTimeoutCount)
          {
            if (IsFileLocked(foundFile))
              Thread.Sleep(1000);
            else
            {
              System.GC.Collect();
					    System.GC.WaitForPendingFinalizers();
              File.Delete(imgsrcF);
              break;
            }
            count++;
          }
          iMainCamEndProcessing = DateTime.Now.Ticks; //Will be still 0 if jump to CATCH
				}
				catch (Exception ex)
				{
					MessageBox.Show("Pemrosesan foto "+ foundFile +" gagal dilakukan, mohon hubungi petugas\n\n" + ex.ToString(), "ERROR CODE: 3R", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}
	}
#endregion

#region API Events
  //EDSDK's API events
  private void APIHandler_CameraAdded(CanonAPI sender)
  {
    try
    {
      Invoke((Action)delegate { RefreshCamera(); });
    }
    catch (Exception ex)
    {
      ReportError(ex.Message, false);
    }
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
    catch (Exception ex)
    {
      ReportError(ex.Message, false);
    }
  }

	private void MainCamera_DownloadReady(EOSDigital.API.Camera sender, DownloadInfo Info)
	{
		try
		{
    #if _SIMULTANEOUS
      Info.FileName = "Front.jpg";
      diMain = Info;
      cdCameraDLReady.Signal();
    #else
			string dir = sap;
			//Invoke((Action)delegate { dir = sap; });
			Info.FileName = "Front.jpg";
			sender.DownloadFile(Info, dir);
    #endif //_SIMULTANEOUS
		}
		catch (Exception ex)
    {
      ReportError(ex.Message, false);
    }
	}

	private void MainCamera_StateChanged(EOSDigital.API.Camera sender, StateEventID eventID, int parameter)
	{
		try
		{
			if (eventID == StateEventID.Shutdown && IsInit)
				Invoke((Action)delegate { CloseMainCamSession(); });
		}
		catch (Exception ex)
		{
			ReportError(ex.Message, false);
		}
	}

	private void MainCamera_ProgressChanged(object sender, int progress)
	{
  #if DEBUG
    #if SIMULTANEOUS
      try
		  {
			  if (progress == 100)
        {
          cdCameraDL.Signal();
          iMainCamDL = DateTime.Now.Ticks;
        }
		  }
		  catch (Exception ex) 
      { 
        ReportError(ex.Message, false); 
      }
    #elif CONSECUTIVE
		  try
		  {
			  Invoke((Action)delegate {
				  if (progress == 100)
          {
            iMainCamDL = DateTime.Now.Ticks;

          #if DEBUG_MAINCAM
            //exit thread gracefully
					  whichCam = ' ';
					  _takeSignal.Set();

					  //Play photo finish audio
					  String load = Config.Read("load", "Audio");
					  if (load != "")
					  {
						  if (File.Exists(load))
						  {
							  playSyncSound(load);
						  }
					  }
            timersave.Start();
          #elif DEBUG_MULTIPLECAM
            whichCam = 'r';
					  _takeSignal.Set();
          #endif
          }
			  });
		  }
		  catch (Exception ex)
      {
        ReportError(ex.Message, false);
      }
    #endif //SIMULTANEOUS/CONSECUTIVE

  #elif RELEASE 
    #if SIMULTANEOUS
      try
		  {
			  if (progress == 100)
        {
          cdCameraDL.Signal();
          iMainCamDL = DateTime.Now.Ticks;
        }
		  }
		  catch (Exception ex) 
      { 
        ReportError(ex.Message, false); 
      }
    #elif CONSECUTIVE
		  try
		  {
			  Invoke((Action)delegate {
				  if (progress == 100) 
          {
					  iMainCamDL = DateTime.Now.Ticks;
					  whichCam = 'l';
					  _takeSignal.Set();
				  }
			  });
		  }
		  catch (Exception ex) { ReportError(ex.Message, false); }
    #endif //SIMULATENOUS/CONSECUTIVE
  #endif //DEBUG/RELEASE
  }

	private void LeftCamera_DownloadReady(EOSDigital.API.Camera sender, DownloadInfo Info)
  {
    try
    {
    #if _SIMULTANEOUS
      Info.FileName = "Left.jpg";
      diLeft = Info;
      cdCameraDLReady.Signal();
    #else
      string dir = sap;
      Info.FileName = "Left.jpg";
      sender.DownloadFile(Info, dir);
    #endif //_SIMULTANEOUS
    }
    catch (Exception ex)
    {
      ReportError(ex.Message, false);
    }
  }

  private void LeftCamera_StateChanged(EOSDigital.API.Camera sender, StateEventID eventID, int parameter)
  {
    try
    {
      if (eventID == StateEventID.Shutdown && IsInit)
        Invoke((Action)delegate { CloseLeftCamSession(); });
    }
    catch (Exception ex)
    {
      ReportError(ex.Message, false);
    }
  }

	private void LeftCamera_ProgressChanged(object sender, int progress)
	{
#if SIMULTANEOUS
  try
	{
		if (progress == 100)
    {
      cdCameraDL.Signal();
      iLeftCamDL = DateTime.Now.Ticks;
    }
	}
	catch (Exception ex) 
  { 
    ReportError(ex.Message, false); 
  }
#elif CONSECUTIVE
	try
	{
		Invoke((Action)delegate {
			if (progress == 100) 
      {
				iLeftCamDL = DateTime.Now.Ticks;

				whichCam = 'r';
				_takeSignal.Set();
			}
		});
	}
	catch (Exception ex) { ReportError(ex.Message, false); }
#endif
	}

	private void RightCamera_DownloadReady(EOSDigital.API.Camera sender, DownloadInfo Info)
  {
    try
    {
    #if _SIMULTANEOUS
      Info.FileName = "Right.jpg";
      diRight = Info;
      cdCameraDLReady.Signal();
    #else
      string dir = sap;
      Info.FileName = "Right.jpg";
      sender.DownloadFile(Info, dir);
    #endif //_SIMULTANEOUS
    }
    catch (Exception ex)
    {
      ReportError(ex.Message, false);
    }
  }

  private void RightCamera_StateChanged(EOSDigital.API.Camera sender, StateEventID eventID, int parameter)
  {
    try
    {
      if (eventID == StateEventID.Shutdown && IsInit)
        Invoke((Action)delegate { CloseRightCamSession(); });
    }
    catch (Exception ex)
    {
      ReportError(ex.Message, false);
    }
  }

	private void RightCamera_ProgressChanged(object sender, int progress)
	{
#if SIMULTANEOUS
    try
		{
			if (progress == 100)
      {
        cdCameraDL.Signal();
        iRightCamDL = DateTime.Now.Ticks;
      }
		}
		catch (Exception ex) 
    { 
      ReportError(ex.Message, false); 
    }
#elif CONSECUTIVE
		try
		{
			Invoke((Action)delegate 
			{
				if (progress == 100)
				{
					iRightCamDL = DateTime.Now.Ticks;

					//exit thread gracefully
					whichCam = ' ';
					_takeSignal.Set();

					//Play photo finish audio
					String load = Config.Read("load", "Audio");
					if (load != "")
					{
						if (File.Exists(load))
							playSyncSound(load);
					}
					
					//Continue
					timersave.Start();
				}
			});
		}
		catch (Exception ex) 
    { 
      ReportError(ex.Message, false); 
    }
#endif
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
      public void imgesize(string imageFile, double scaleFactor, string posi)
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

				          String outputFile = sap + Barcode.filename + "_0" + posi + "r.jpg";
				          newImage.Save(outputFile, System.Drawing.Imaging.ImageFormat.Jpeg);
			        }
          }
      }
          
      public void imgthumb(string imageFile, double scaleFactor, string posi)
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

                  String outputFile = sap + Barcode.filename+ "_0" + posi + "s.jpg";
                  newImage.Save(outputFile, System.Drawing.Imaging.ImageFormat.Jpeg);
              }
          }
      }

      public void ResizeImage(Bitmap srcImage, double scaleFactor, string posi, int mode)
      //Description   :   Crop image and then resize image resolution with scale factor
      //Input         :   Original image and scale factor
      //Output        :   Resized image
      {
          int newWidth  = (int)(srcImage.Width * scaleFactor);
          int newHeight = (int)(srcImage.Height * scaleFactor);

          using (var newImage = new Bitmap(newWidth, newHeight))
          using (var graphics = Graphics.FromImage(newImage))
          {
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            graphics.DrawImage(srcImage, new System.Drawing.Rectangle(0, 0, newWidth, newHeight));
                
            String outputFile = "";
            if(mode == 0) //large
              outputFile = sap + Barcode.filename + "_0" + posi + "r.jpg";
            else if (mode == 1) //thumbnail
              outputFile = sap + Barcode.filename + "_0" + posi + "s.jpg";
            newImage.Save(outputFile, System.Drawing.Imaging.ImageFormat.Jpeg);
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
          Int32.TryParse(Config.Read("center", "Camera"), out int cam);
          if (CamList.Count > 0)
          {
              MainCamera = CamList[cam];
              MainCamera.OpenSession();
              MainCamera.LiveViewUpdated += MainCamera_LiveViewUpdated;
              MainCamera.StateChanged    += MainCamera_StateChanged;
              MainCamera.DownloadReady   += MainCamera_DownloadReady;
			        MainCamera.ProgressChanged += MainCamera_ProgressChanged;

              if(!MainCamera.IsLiveViewOn)
                MainCamera.StartLiveView();

              MainCamera.SetSetting(PropertyID.SaveTo, (int)SaveTo.Host);
              MainCamera.SetCapacity(4096, int.MaxValue);
          }
          else
          {
              MessageBox.Show("Camera not connected\nPlease connect and try again", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
              CloseMainCamSession();
              MainCamera.Dispose();
              //Terminate background process
              if (System.Windows.Forms.Application.MessageLoop) 
                System.Windows.Forms.Application.Exit();
              else 
                System.Environment.Exit(1);
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
          Int32.TryParse(Config.Read("right", "Camera"), out int cam);
          if (CamList.Count > 0)
          {
              RightCamera = CamList[cam];
              RightCamera.OpenSession();
              RightCamera.StateChanged    += RightCamera_StateChanged;
              RightCamera.DownloadReady   += RightCamera_DownloadReady;
			        RightCamera.ProgressChanged += RightCamera_ProgressChanged;

              RightCamera.SetSetting(PropertyID.SaveTo, (int)SaveTo.Host);
              RightCamera.SetCapacity(4096, int.MaxValue);
          }
          else
          {
              MessageBox.Show("Camera not connected\nPlease connect and try again", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
              CloseRightCamSession();
              RightCamera.Dispose();
              //Terminate background process
              if (System.Windows.Forms.Application.MessageLoop) 
                System.Windows.Forms.Application.Exit();
              else 
                System.Environment.Exit(1);
          }
      }

      private void CloseRightCamSession()
      //Description   :   Close RightCamera session
      {
          RightCamera.CloseSession();
      }

      private void OpenLeftCamSession()
      //Description   :   Open LeftCamera session
      {
          Int32.TryParse(Config.Read("left", "Camera"), out int cam);
          if (CamList.Count > 0)
          {
              LeftCamera = CamList[cam];
              LeftCamera.OpenSession();
              LeftCamera.StateChanged    += LeftCamera_StateChanged;
              LeftCamera.DownloadReady   += LeftCamera_DownloadReady;
			        LeftCamera.ProgressChanged += LeftCamera_ProgressChanged;

              LeftCamera.SetSetting(PropertyID.SaveTo, (int)SaveTo.Host);
              LeftCamera.SetCapacity(4096, int.MaxValue);
          }
          else
          {
              MessageBox.Show("Camera not connected\nPlease connect and try again", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
              CloseLeftCamSession();
              LeftCamera.Dispose();
              //Terminate background process
              if (System.Windows.Forms.Application.MessageLoop) 
                System.Windows.Forms.Application.Exit();
              else 
                System.Environment.Exit(1);
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

          if (errc < 4) 
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
          else if (errc == 4) 
            MessageBox.Show("Many errors happened!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

          lock (ErrLock) { ErrCount--; }
      }
#endregion
        
  private void Camera_FormClosing(object sender, FormClosingEventArgs e)
  //Description   :   Close all cameras session and terminate background when form is closed
  {
    //Close DSLR - enter standby mode
    MainCamera?.Dispose();
    LeftCamera?.Dispose();
    RightCamera?.Dispose();
    APIHandler?.Dispose();
    IsInit = false;

  #if CONSECUTIVE
	  _takeSignal.Close();
  #endif

    //Terminate background process
    if (System.Windows.Forms.Application.MessageLoop)
      System.Windows.Forms.Application.Exit();
    else
      System.Environment.Exit(1);

    return;
  }

	//############################################################//
	//                    MULTI THREADING                         //

#if CONSECUTIVE
	private void TakePhoto()
	{
		while(true)
		{
			_takeSignal.WaitOne();
			lock (_locker)
			{
				switch (whichCam)
				{
					case 'm':
					{
						try
						{
							MainCamera.TakePhoto();
              iMainCamTake = DateTime.Now.Ticks;
						}
						catch (Exception ex)
            {
              ReportError(ex.Message, false);
            }
						break;
					}
					case 'r':
					{
						try
						{
							RightCamera.TakePhoto();
              iRightCamTake = DateTime.Now.Ticks;
						}
						catch (Exception ex)
            {
              ReportError(ex.Message, false);
            }
						break;
					}
					case 'l':
					{
						try
						{
							LeftCamera.TakePhoto();
              iLeftCamTake = DateTime.Now.Ticks;
						}
						catch (Exception ex)
            {
              ReportError(ex.Message, false);
            }
						break;
					}
					case ' ':
						return;
				}
			}
		}
	}

	private void takePhotoR()
	{
		try
		{
			RightCamera.TakePhoto();
			iRightCamDL = DateTime.Now.Ticks;
		}
		catch (Exception ex)
		{
			ReportError(ex.Message, false);
		}
	}

	private void takePhotoL()
	{
		try
		{
			LeftCamera.TakePhoto();
			iLeftCamDL = DateTime.Now.Ticks;
		}
		catch (Exception ex)
		{
			ReportError(ex.Message, false);
		}
	}

	private void takePhotoM()
	{
		try
		{
			MainCamera.TakePhoto();
			iMainCamDL = DateTime.Now.Ticks;
		}
		catch (Exception ex)
		{
			ReportError(ex.Message, false);
		}
	}
#endif //CONSECUTIVE

	//############################################################//
	//                       DEBUGGING                            //

	private void lbCountdown_Click(object sender, EventArgs e)
  //Description   :   Manually take photo
  {
		//Finish image processing (freezing the image stream display as result)
		if (MainCamera != null)
    {
      Application.Idle -= ViewFrame;
      Application.Idle -= ProcessFrame;
      timer.Enabled = false;
    }

#if SIMULTANEOUS
    //Multiple threads method
    //		Thread t1 = new Thread(() => TakePhotoL());
    //		Thread t2 = new Thread(() => TakePhotoM());
    //		Thread t3 = new Thread(() => TakePhotoR());

    //		t1.Start();
    //		t2.Start();
    //		t3.Start();

    //		//what if I freeze the main thread here
    //		//does the download proccess still halted? no
    //		//and result in resize and crop error because photo is still not finished DL?
    //		//isn't Join is blocking too?
    //		t1.Join();
    //		t2.Join();
    //		t3.Join();
       
    try 
    { 
      MainCamera.TakePhoto(); 
      iMainCamTake = DateTime.Now.Ticks;
    }
    catch (Exception ex) { ReportError(ex.Message, false); }

    try 
    { 
      RightCamera.TakePhoto();
      iRightCamTake = DateTime.Now.Ticks;
    }
    catch (Exception ex) { ReportError(ex.Message, false); }

    //Wait until receieve signal from each camera's downloadprogress
    cdCameraDL.Wait();

    //Play photo finish audio
		String load = Config.Read("load", "Audio");
		if (load != "")
		{
			if (File.Exists(load))
			{
				playSyncSound(load);
			}
		}

    //Continue
    timersave.Start();
  #elif CONSECUTIVE
    Thread t = new Thread(() => TakePhoto());
		t.Start();

    whichCam = 'm';
    _takeSignal.Set();
  #endif
  }

	public static String GetTimeStamp(DateTime value)
  //Description   :   Count timestamp
  {
    return value.ToString("HH mm ss ffff");
  }

  public static String GetMonthStamp(DateTime value)
  {
	  return value.ToString("MM");
  }

	  private bool IsFileLocked(FileInfo file)
	  //Description	:	Check file status
	  {
		  FileStream stream = null;
		  try
		  {
			  stream = file.Open(FileMode.Open, System.IO.FileAccess.ReadWrite, FileShare.None);
		  }
		  catch (IOException)
		  {
			  //File is unavailable:
			  //still being written
			  //or being processed by another thread
			  //or does not exist (has already been processed)
			  return true;
		  }
		  finally
		  {
			  if (stream != null)
				  stream.Close();
		  }

		  //File is not locked
		  return false;
	  }

    private String CalculateLog(long iTake, long iDL, long iStartProc, long iEndProc)
    {
      String LogDL = " "; 
      String LogStartProc= " ";
      String LogEndProc = " ";

      if(iTake == 0)
        LogDL = "No Take";
      else if (iDL == 0)
        LogDL = "No DL";
      else
      {
        TimeSpan elapsedSpan = TimeSpan.FromTicks(iDL - iTake);
        LogDL = (elapsedSpan.TotalSeconds * 1000).ToString();
      }

      if(iTake == 0)
        LogStartProc = "No Take"; //Start proc will be 0 too
      else if(iStartProc == 0)
        LogStartProc = "No Proc";
      else
      {
         TimeSpan elapsedSpan = TimeSpan.FromTicks(iStartProc - iTake);
         LogStartProc = (elapsedSpan.TotalSeconds *1000).ToString();
      }

      if(iStartProc == 0)
        LogEndProc = "No Proc";
      else if (iEndProc == 0)
        LogEndProc = "No Finish Proc";
      else
      {
        TimeSpan elapsedSpan = TimeSpan.FromTicks(iEndProc - iStartProc);
        LogEndProc = (elapsedSpan.TotalSeconds*1000).ToString();
      }

      return LogDL+"."+LogStartProc+"."+LogEndProc;
    }
  }
}