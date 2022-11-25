using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace LampGraph
{
    class Register
    {
        public static string LR2filesPath;
        public static string dir;
        public Register()
        {
            
        }

        public static bool CheckRegistered()
        {
            dir=AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
            FileInfo fi = new FileInfo(dir + "\\config.json");
            Console.WriteLine(dir);
            if (fi.Exists)
            {
                using(StreamReader sr=new StreamReader(fi.OpenRead()))
                {
                    dynamic json = JsonConvert.DeserializeObject(sr.ReadToEnd());
                    //dynamic j = DynamicJson.Parse(fi.OpenRead(), Encoding.UTF8);
                    LR2filesPath = json["Folderpath"];

                }
            }
            return fi.Exists;
        }

        public static bool SelectFolder()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog()
            {
                Description = "Please Select 'LR2files' Folder"
            };
            if (fbd.ShowDialog()!=DialogResult.OK) return false;
            string p = fbd.SelectedPath;
            FileInfo fi = new FileInfo(p + "\\Database\\song.db");
            if (fi.Exists)
            {
                LR2filesPath = p;
                var obj = new {Folderpath=LR2filesPath};
                string ds = JsonConvert.SerializeObject(obj);
                //string ds = DynamicJson.Serialize(obj);
                FileInfo fs = new FileInfo(dir + "\\config.json");
                using (StreamWriter sw = new StreamWriter(fs.OpenWrite()))
                {
                    sw.Write(ds);
                    sw.Flush();
                }
            }
            return fi.Exists;
        }

    }
}
