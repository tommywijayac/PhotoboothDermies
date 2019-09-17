/*
 * PHOTOBOOTH ERHA.EXE - Barcode class
 * ----------------------------------------------------------------------------------
 * Read barcode reading from user and validate them
 * 
 * Input:
 * 1. User barcode reading
 * 
 * Output:
 * 1. None
 * ----------------------------------------------------------------------------------
 */

//Basic reference
using System;
using System.Windows.Forms;
//Include reference for playing .wav files
using System.Media;
//Include reference for database connection
using MySql.Data.MySqlClient;
//Include reference for switching btw forms
using System.Linq;

namespace Erha
{
  public partial class Barcode : Form
  {
    #region variable
    public static string foldername;
    public static string dbs, dbu, dbp, dbn, dbt;
    public static string constring;
    public static string patid, mnrl, cid, brid, filename, tanggal, upsynch, regid;
    #endregion

    string[] args  = Environment.GetCommandLineArgs(); //args is filled with ID for retake photo (if called)
    IniFile Config = new IniFile("Settings.ini");

    public Barcode()
    {
      InitializeComponent();
    }

    private void Barcode_Load(object sender, EventArgs e)
    //Description   :   When the form load..
    {
      //Get database info from settings
      dbs = Config.Read("server", "Database");
      dbu = Config.Read("user", "Database");
      dbp = Config.Read("pass", "Database");
      dbn = Config.Read("name", "Database");
      constring = "host=" + dbs + ";user=" + dbu + ";password=" + dbp + ";database=" + dbn + ";";

      //If form opened with initial args (retake case from Review form), immediately set textbox input with those value
      if (args.Length == 2)
      {
        tbBarcode.Text = args[1];
      }
            
      //Focus on textbox
      tbBarcode.Select();
      tbBarcode.Focus();
    }

    private void tbBarcode_KeyDown(object sender, KeyEventArgs e)
    //Description   :   Disable enter sound because barcode reading ended with enter keypress
    {
      if (e.KeyCode == Keys.Enter)
      {
        //Disable "enter" sound
        e.Handled = true;
        e.SuppressKeyPress = true;

        mvtocamera();
      }
    }

    private void tbBarcode_TextChanged(object sender, EventArgs e)
    //Description   :   Process input data from barcode if length is correct
    {
      if (args.Length == 2)
      {
        mvtocamera();
      }
    }

    private void mvtocamera()
    //Description   :   Validate reading and continue to Camera form if true
    {
      //Disable tbBarcode
      tbBarcode.Enabled = false;

      //Foldername for this session result
      foldername = tbBarcode.Text;

    #if RELEASE
			//Validate reading
			MySqlConnection con = new MySqlConnection(constring);
      MySqlConnection con1 = new MySqlConnection(constring);
      
      MySqlCommand check_User_Name = new MySqlCommand("SELECT 1 FROM registration WHERE RIGHT(regID,4) = @id", con);
      check_User_Name.Parameters.AddWithValue("@id", tbBarcode.Text);
      con.Open();
      MySqlDataReader reader = check_User_Name.ExecuteReader();
      con.Close();

      //If validation result to regID return true
      if (reader.HasRows)
      {
        //Username exist
        string mide = tbBarcode.Text;

        string sql2;
        string sql = " SELECT * FROM registration where RIGHT(regID,4)='" + mide + "' ";

        MySqlCommand cmd = new MySqlCommand(sql, con);
        con.Open();
        MySqlDataReader readers2 = cmd.ExecuteReader();

        while (readers2.Read())
        {
          patid = readers2.GetString("patientID");
          cid = readers2.GetString("clientId");
          brid = readers2.GetString("branchId");
          regid = readers2.GetString("regID");

          sql2 = " SELECT * FROM patient where patientIDL='" + patid + "' ";
          MySqlCommand cmd1 = new MySqlCommand(sql2, con1);
          con1.Open();
          MySqlDataReader readers21 = cmd1.ExecuteReader();

          while (readers21.Read())
          {
            mnrl = readers21.GetString("MRNL");
          }
          con1.Close();
        }
        con.Close();
      }
      else
      {
        //Username doesn't exist.
        MessageBox.Show("Pasien tidak ditemukan dalam database. Silahkan mendaftar terlebih dahulu.");
        //Empty textbox
        tbBarcode.Text = "";
        tbBarcode.Select();
        tbBarcode.Focus();
        return;
      }
      #elif DEBUG
		    //Use preset parameters for 0006
		    regid = "30006";
		    mnrl = "5000000";
      #endif

			  //Collect necessary infos
			  //regid = rid.Text.Substring(rid.Text.Length-1,1);
			  tanggal = DateTime.Now.ToString("yyMMdd");
        Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        upsynch = unixTimestamp.ToString();
        filename = mnrl + "_" + tanggal + "_" + regid.Substring(regid.Length - 1, 1);
            
        //Wait for a few seconds
        timerHold.Start();
      }

    private void timerHold_Tick(object sender, EventArgs e)
    //Description   :   Wait finish, load Camera form
    {
      timerHold.Stop();
            
      this.Hide();
      Camera fo = new Camera();
      fo.Show();
    }

    private void Barcode_FormClosed(object sender, FormClosedEventArgs e)
    //Description   :   Terminate background process if form is closed
    {
      if (System.Windows.Forms.Application.MessageLoop)
      {
        System.Windows.Forms.Application.Exit();
      }
      else
      {
        System.Environment.Exit(1);
      }
    }

    /*======================================================================*/
    /*======================== HELPER FUNCTION =============================*/
    private void playSyncSound(string a)
    //Description   :   play .wav sound file, other processes are halted
    //Input         :   .wav file name
    {
      SoundPlayer SyncSound = new SoundPlayer(a);
      SyncSound.PlaySync();
    }
  }
}
