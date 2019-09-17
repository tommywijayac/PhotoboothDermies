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
using System.Net;
using System.Text.RegularExpressions;

namespace SettingErha
{
    public partial class Form3 : Form
    {
        public class FTPListDetail
        {
            public bool IsDirectory
            {
                get
                {
                    return !string.IsNullOrWhiteSpace(Dir) && Dir.ToLower().Equals("d");
                }
            }
            internal string Dir { get; set; }
            public string Permission { get; set; }
            public string Filecode { get; set; }
            public string Owner { get; set; }
            public string Group { get; set; }
            public string Name { get; set; }
            public string FullPath { get; set; }
        }

        public Form3()
        {
            InitializeComponent();
        }

        private void canc_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void sel_Click(object sender, EventArgs e)
        {
            string beb = pathview.SelectedNode.FullPath;
            string fpath;
            if (beb == "root")
            {
                fpath = "ftp://" + Form1.sser;
            }
            else
            {
                fpath = "ftp://" + Form1.sser + "/" + beb.Substring(5);
            }
            //Form1 ab = new Form1();
            //ab.dirt.Text = "";
            this.Owner.Controls.Find("dirt", true).First().Text = fpath;
            //MessageBox.Show(bc);
            this.Hide();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            

            var root = "ftp://" + Form1.sser; 

            pathview.Nodes.Clear();
            pathview.Nodes.Add(CreateDirectoryNode(root, "root"));
        }

        private TreeNode CreateDirectoryNode(string path, string name)
        {
            var directoryNode = new TreeNode(name);
            var directoryListing = GetDirectoryListing(path);

            var directories = directoryListing.Where(d => d.IsDirectory);
            var files = directoryListing.Where(d => !d.IsDirectory);

            foreach (var dir in directories)
            {
                directoryNode.Nodes.Add(CreateDirectoryNode(dir.FullPath, dir.Name));
            }
            /*foreach (var file in files)
            {
                directoryNode.Nodes.Add(new TreeNode(file.Name));
            }*/
            return directoryNode;
        }

        public IEnumerable<FTPListDetail> GetDirectoryListing(string rootUri)
        {
            var CurrentRemoteDirectory = rootUri;
            var result = new StringBuilder();
            var request = GetWebRequest(WebRequestMethods.Ftp.ListDirectoryDetails, CurrentRemoteDirectory);
            using (var response = request.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    string line = reader.ReadLine();
                    while (line != null)
                    {
                        result.Append(line);
                        result.Append("\n");
                        line = reader.ReadLine();
                    }
                    if (string.IsNullOrEmpty(result.ToString()))
                    {
                        return new List<FTPListDetail>();
                    }
                    result.Remove(result.ToString().LastIndexOf("\n"), 1);
                    var results = result.ToString().Split('\n');
                    string regex =
                        @"^" +               //# Start of line
                        @"(?<dir>[\-ld])" +          //# File size          
                        @"(?<permission>[\-rwx]{9})" +            //# Whitespace          \n
                        @"\s+" +            //# Whitespace          \n
                        @"(?<filecode>\d+)" +
                        @"\s+" +            //# Whitespace          \n
                        @"(?<owner>\w+)" +
                        @"\s+" +            //# Whitespace          \n
                        @"(?<group>\w+)" +
                        @"\s+" +            //# Whitespace          \n
                        @"(?<size>\d+)" +
                        @"\s+" +            //# Whitespace          \n
                        @"(?<month>\w{3})" +          //# Month (3 letters)   \n
                        @"\s+" +            //# Whitespace          \n
                        @"(?<day>\d{1,2})" +        //# Day (1 or 2 digits) \n
                        @"\s+" +            //# Whitespace          \n
                        @"(?<timeyear>[\d:]{4,5})" +     //# Time or year        \n
                        @"\s+" +            //# Whitespace          \n
                        @"(?<filename>(.*))" +            //# Filename            \n
                        @"$";                //# End of line

                    var myresult = new List<FTPListDetail>();
                    foreach (var parsed in results)
                    {
                        var split = new Regex(regex)
                            .Match(parsed);
                        var dir = split.Groups["dir"].ToString();
                        var permission = split.Groups["permission"].ToString();
                        var filecode = split.Groups["filecode"].ToString();
                        var owner = split.Groups["owner"].ToString();
                        var group = split.Groups["group"].ToString();
                        var filename = split.Groups["filename"].ToString();
                        myresult.Add(new FTPListDetail()
                        {
                            Dir = dir,
                            Filecode = filecode,
                            Group = group,
                            FullPath = CurrentRemoteDirectory + "/" + filename,
                            Name = filename,
                            Owner = owner,
                            Permission = permission,
                        });
                    };
                    return myresult;
                }
            }
        }

        private FtpWebRequest GetWebRequest(string method, string uri)
        {
            Uri serverUri = new Uri(uri);
            if (serverUri.Scheme != Uri.UriSchemeFtp)
            {
                return null;
            }
            var reqFTP = (FtpWebRequest)FtpWebRequest.Create(serverUri);
            reqFTP.Method = method;
            reqFTP.UseBinary = true;
            reqFTP.Credentials = new NetworkCredential(Form1.suser, Form1.spass);
            reqFTP.Proxy = null;
            reqFTP.KeepAlive = false;
            reqFTP.UsePassive = false;
            return reqFTP;
        }
    }
}
