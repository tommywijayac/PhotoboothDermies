/*
 * PHOTOBOOTH ERHA.EXE - CReview
 * ----------------------------------------------------------------------------------
 * Preview photos result, and upload in database if finished 
 * 
 * Input:
 * 1. User barcode reading
 * 
 * Output:
 * 1. Six entries to database
 * 2. Six photos to server via ftp
 * ----------------------------------------------------------------------------------
 */

//Basic reference
using System;
using System.Windows.Forms;
using System.Net;
//Include reference for database connection
using MySql.Data.MySqlClient;
using System.Threading;
//Include reference for playing .wav files
using System.Media;
//Include reference for files
using System.IO;
//Include reference for switching btw forms
using System.Linq;

namespace Erha
{
  public partial class Review : Form
  {
  #region Variables
    IniFile Config = new IniFile("Settings.ini");
    int timeleft  = 5;
    int timecount = 0;
    int review    = 0;

		//private:
		string sImageDir;
		string sFileName;
    bool   bImageNotFound = true;
  #endregion
        
    public Review()
    {
      InitializeComponent();
    }

    private void Saveimg(string cli, string bra, string filename, string upsynch, string regid)
    //Description   :   Save image query to databse with specified path
    //Input         :   clientID,branchID,filename,updateSynch, and regID from database
    {
      string querynew = "INSERT INTO file(`clientID`, `branchID`, `filename`, `updateSynch`,`regID`) VALUES (" + cli + ", '" + bra + "', '" + filename + "', '" + upsynch + "', '" + regid + "')";

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
        // Show any error message.
        MessageBox.Show("Koneksi ke database gagal" + ex.Message);
      }
    }

    private void Review_Load(object sender, EventArgs e)
    //Description   :   Load photos taken to be viewed
    {
			sImageDir = Camera.sap;
			sFileName = Barcode.filename;
            
      //Check file
      string ImgLoc = sImageDir + sFileName + "_01r.jpg";
      if (File.Exists(ImgLoc))
        ibFront.ImageLocation = ImgLoc;
            
      ImgLoc = sImageDir + sFileName + "_03r.jpg";
      if (File.Exists(ImgLoc))
        ibRight.ImageLocation = ImgLoc;

      ImgLoc = sImageDir + sFileName + "_02r.jpg";
      if(File.Exists(ImgLoc))
        ibLeft.ImageLocation = ImgLoc;

      Int32.TryParse(Config.Read("review", "General"), out review);

      //Start countdown
      timclose.Start();
    }
		
    private void timclose_Tick(object sender, EventArgs e)
    //Description   :   Countdown before closing 
    {
			//Review ON
			if (review == 1)
      {
        if (timeleft > 0)
        {
          //Focus on textbox
          tbBarcodeR.Select();
          tbBarcodeR.Focus();

          timeleft = timeleft - 1;
          
          if (timeleft == 4)
            rb5.Visible = false;
          else if (timeleft == 3)
            rb4.Visible = false;
          else if (timeleft == 2)
            rb3.Visible = false;
          else if (timeleft == 1)
            rb2.Visible = false;
					else if (timeleft == 0)
					{
						//Display "Thank You"
						tableLayoutPanel1.Hide();
						lbThanks.Left = (this.Width - lbThanks.Width) / 2;
						lbThanks.Top = (this.Height - lbThanks.Height) / 2;
					}
        }
        else
        {
          //NOT RETAKE case
          timclose.Stop();
					
          //Start hold to play Thank You audio
          //Change to finish rev step 5
          String finish = Config.Read("finish", "Audio");
          if (finish != "")
          {
            if (File.Exists(finish))
              playSyncSound(finish);
          }

        #if RELEASE
					//Upload image path to database
					try
          {
            string _constring = Barcode.constring;
            MySqlConnection con = new MySqlConnection(_constring);

            string[] asd = Directory.GetFiles(Camera.sap, "*.*");
            //Searching for existing database query...
            foreach (string file in asd)
            {
              string tuju = Camera.uppath + "/" + Path.GetFileName(file);
              string namafile, fileup;
              namafile = Path.GetFileNameWithoutExtension(file);
              fileup = namafile.Substring(namafile.Length - 1, 1);

              if (fileup == "r")
              {
                MySqlCommand check_query = new MySqlCommand(" SELECT * FROM file WHERE filename ='" + namafile.TrimEnd('r') + ".jpg' ", con);
                con.Open();
                MySqlDataReader myreader = check_query.ExecuteReader();
                con.Close();
                if (myreader.HasRows)
                {
                  //do nothing
                }
                else
                {
                  Saveimg(Barcode.cid, Barcode.brid, namafile.TrimEnd('r') + ".jpg", Barcode.upsynch, Barcode.regid);
                }
              }
            }
          }
          catch (Exception ex) 
          { 
            MessageBox.Show(ex.ToString()); 
          }

          //Upload photo path to ftp
          using (WebClient client = new WebClient())
          {
            bool IsUploadComplete = false;

            client.Credentials = new NetworkCredential(Barcode.dbu, Barcode.dbp);
            //string tuju = "ftp://"+hostid.Text + "//" + sFN;

            string[] asd = Directory.GetFiles(Camera.sap, "*.*");
            foreach (string file in asd)
            {
							string tuju = Camera.uppath + "/" + Path.GetFileName(file);
							timerHold.Start();

							//Check upload result:
							//Validation done as many as possible within time constrain
							//Defined timeout constrain 10 sec.
							do
							{
								IsUploadComplete = UploadFile(tuju, file);

								if (IsUploadComplete)
								{
									timerHold.Stop();
									timecount = 0;
									//break;
								}

								if (timecount == 10)
								{
									client.Dispose();
									MessageBox.Show("Timeout. FTP can't be reached. Closing program...");
									//System.Diagnostics.Process.Start("admopn.exe");
									Environment.Exit(1);
									break;
								}
							} while (!IsUploadComplete);
						}
          }
        #endif //RELEASE

					//Finish, return to Barcode form
					System.Diagnostics.Process.Start("admopn.exe");
          Application.Exit();
        }
      }

      //Review OFF
      else if (review == 0)
      {
        timclose.Stop();

        //Display "Thank You" text

        tableLayoutPanel1.Hide();
        lbThanks.Left = (this.Width - lbThanks.Width) / 2;
        lbThanks.Top = (this.Height - lbThanks.Height) / 2;

        //NOT RETAKE case
        timclose.Stop();

				//Start hold to play Thank You audio
				//Change to finish rev step 5
				String finish = Config.Read("finish", "Audio");
        if (finish != "")
        {
            if (File.Exists(finish))
                playSyncSound(finish);
        }

#if RELEASE
				//Upload image path to database
				try
        {
            string _constring = Barcode.constring;
            MySqlConnection con = new MySqlConnection(_constring);

            string[] asd = Directory.GetFiles(Camera.sap, "*.*");
            //Searching for existing database query...
            foreach (string file in asd)
            {
                string tuju = Camera.uppath + "/" + Path.GetFileName(file);
                string namafile, fileup;
                namafile = Path.GetFileNameWithoutExtension(file);
                fileup = namafile.Substring(namafile.Length - 1, 1);

                if (fileup == "r")
                {
                    MySqlCommand check_query = new MySqlCommand(" SELECT * FROM file WHERE filename ='" + namafile.TrimEnd('r') + ".jpg' ", con);
                    con.Open();
                    MySqlDataReader myreader = check_query.ExecuteReader();
                    con.Close();
                    if (myreader.HasRows)
                    {
                        //do nothing
                    }
                    else
                    {
                        Saveimg(Barcode.cid, Barcode.brid, namafile.TrimEnd('r') + ".jpg", Barcode.upsynch, Barcode.regid);
                    }
                }
            }
        }
        catch (Exception ex) { MessageBox.Show(ex.ToString()); }

          //Upload photo path to ftp
          using (WebClient client = new WebClient())
          {
              bool IsUploadComplete = false;

              client.Credentials = new NetworkCredential(Barcode.dbu, Barcode.dbp);
              //string tuju = "ftp://"+hostid.Text + "//" + sFN;

              string[] asd = Directory.GetFiles(Camera.sap, "*.*");
              foreach (string file in asd)
              {
                  string tuju = Camera.uppath + "/" + Path.GetFileName(file);
                  timerHold.Start();

                  //Check upload result:
                  //Validation done as many as possible within time constrain
                  //Defined timeout constrain 10 sec.
                  do
                  {
                      IsUploadComplete = UploadFile(tuju, file);

                      if (IsUploadComplete)
                      {
                          timerHold.Stop();
                          timecount = 0;
                          //break;
                      }

                      if (timecount == 10)
                      {
								          timerHold.Stop();
                          client.Dispose();
                          MessageBox.Show("Timeout. FTP can't be reached. Closing program...");
                          //System.Diagnostics.Process.Start("admopn.exe");
                          Environment.Exit(1);
                          break;
                      }
                  }
                  while (!IsUploadComplete);
              }
          }
#endif

        //Finish, return to Barcode form
        System.Diagnostics.Process.Start("admopn.exe");
        Application.Exit();
      }
    }

    public bool UploadFile(string addr, string filename)
    {
      try
      {
        WebClient client = new WebClient();
        client.Credentials = new NetworkCredential(Barcode.dbu, Barcode.dbp);
        client.UploadFile(addr, "STOR", filename);
        return true;
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.ToString());
        return false;
      }
    }
        
    private void tbBarcodeR_KeyDown(object sender, KeyEventArgs e)
    //Description   :   RETAKE case;
    //                  Disable enter sound because barcode reading ended with enter keypress
    //                  Return to Barcode form with Barcode reading as args
    {
      if (e.KeyCode == Keys.Enter)
      {
        //Disabled "enter" sound
        e.Handled = true;
        e.SuppressKeyPress = true;

        System.Diagnostics.Process.Start("admopn.exe", tbBarcodeR.Text);
        Application.Exit();
      }
    }

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

    private void timerHold_Tick(object sender, EventArgs e)
    {
      timerHold.Stop();
			timecount += 1;
    }

      ///////////////////////// PREVIOUS VERSION //////////////////////////////////////
      private void button1_Click(object sender, EventArgs e)
      //Close button
      {
          using (WebClient client = new WebClient())
          {
              client.Credentials = new NetworkCredential(Barcode.dbu, Barcode.dbp);
              string[] asd = Directory.GetFiles(Camera.sap, "*.*");
              foreach (string file in asd)
              {

                  string tuju = Camera.uppath + "/" + Path.GetFileName(file);
                  client.UploadFile(tuju, "STOR", file);
                  Saveimg(Barcode.cid, Barcode.brid, Path.GetFileName(file), Barcode.upsynch, Barcode.regid);
              }

          }

          System.Diagnostics.Process.Start("admopn.exe");
          Application.Exit();
      }

      private void RTake_Click(object sender, EventArgs e)
      //Retake button
      {
          //Camera fo2 = new Camera();
          //this.Hide();
          //fo2.Show();
          DirectoryInfo hdDirectoryInWhichToSearch = new DirectoryInfo(Camera.sap);
          FileInfo[] filesInDir = hdDirectoryInWhichToSearch.GetFiles("*.*");
          try
          {
              foreach (FileInfo foundFile in filesInDir)
              {
                  string imgsrc = foundFile.FullName;
                  //imgResize(convert to mat imgsrc);

                  File.Delete(imgsrc);
              }
          }
          catch { MessageBox.Show("File not found- C2"); }
          System.Diagnostics.Process.Start("admopn.exe");
          Application.Exit();
      }

      ///////////////////////// THREADING EXPERIMENT //////////////////////////////////////
      public void UploadFileinBackground(string addr, string filename, int n)
      {
          System.Threading.AutoResetEvent waiter = new System.Threading.AutoResetEvent(false);
          WebClient client = new WebClient();
          Uri uri = new Uri(addr);
          string method = "POST";

          client.UploadFileCompleted += new UploadFileCompletedEventHandler(UploadFileCallBack);
          client.UploadFileAsync(uri, method, filename, waiter);

          waiter.WaitOne();
          MessageBox.Show("Upload Complete");

          if (n == 6) { timerHold.Start(); }
      }

      private static void UploadFileCallBack(Object sender, UploadFileCompletedEventArgs e)
      {
          System.Threading.AutoResetEvent waiter = (System.Threading.AutoResetEvent)e.UserState;
          try
          {
              string reply = System.Text.Encoding.UTF8.GetString(e.Result);
              MessageBox.Show(reply);
          }
          finally
          {
              waiter.Set();
          }
      }
  }
}
