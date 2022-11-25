using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LampGraph
{
    class ReadDiffTable
    {
        public static dynamic tables;
        public static Dictionary<string, string> symbols;

        public static void ReadTableList()
        {
            string dir = Register.dir;
            string path = dir + "\\tables.json";
            symbols = new Dictionary<string, string>();
            FileInfo fi = new FileInfo(path);
            try
            {
                using (StreamReader sr = new StreamReader(fi.OpenRead(), Encoding.UTF8))
                {
                    string s = sr.ReadToEnd();
                    tables = JsonConvert.DeserializeObject(s);
                }
            }
            catch
            {
                MessageBox.Show("JSON Parse Error! Please Make Sure 'tables.json' is Correct.", "Failed to Read TableList", MessageBoxButtons.OK, MessageBoxIcon.Error);

                Environment.Exit(1);
            }

        }

        public static List<object> ReturnTableNameList()
        {
            List<object> obj = new List<object>();
            object def = new
            {
                Key = "###Default###",
                Name = "---"
            };
            obj.Add(def);
            foreach(string k in tables["keys"])
            {
                string tn = tables["body"][k]["Title"];
                object o = new
                {
                    Key = k,
                    Name = tn
                };
                obj.Add(o);
            }
            return obj;
        }

        public static dynamic GetTable(string key, bool fetch = false)
        {
            string dir = Register.dir + "\\" + key + "_body.json";
            FileInfo fi = new FileInfo(dir);
            if (fi.Exists && !fetch)
            {
                Console.WriteLine("Table Json Exists!");
                using (StreamReader sr = new StreamReader(fi.OpenRead()))
                {
                    try
                    {
                        return JsonConvert.DeserializeObject(sr.ReadToEnd());
                    }
                    catch
                    {
                        Console.WriteLine("Incorrect JSON!");
                    }
                }
            }
            string url;
            try
            {
                url = tables["body"][key]["Url"];
            }
            catch
            {
                Console.WriteLine("The Specified key does not exist!");
                return null;
            }
            Match m = Regex.Match(url, @"(https?://.*/).*");
            if (!m.Success)
            {
                Console.WriteLine("ReadDiffTable_GetTable :Incorrect url!");
                return null; //incorrect url
            }
            string root = m.Groups[1].Value;

            WebRequest req_1 = WebRequest.Create(url);
            WebResponse res_1;
            try
            {
                res_1 = req_1.GetResponse();
            }
            catch
            {
                Console.WriteLine("ReadDiffTable_GetTable :Cannot Get Response: req_1, " + url);
                return null;
            }
            string ress;
            using (StreamReader sr = new StreamReader(res_1.GetResponseStream()))
            {
                ress = sr.ReadToEnd();
            }
            Match m2 = Regex.Match(ress, @"<meta *name=""bmstable"" *content=""(.*?)"" */?>");
            if (!m2.Success) return null; //it does not contain the tag
            string h_url_raw = m2.Groups[1].Value;
            string h_url = MergeRelToAbs(root, h_url_raw);

            WebRequest req_h = WebRequest.Create(h_url);
            WebResponse res_h;
            try
            {
                res_h = req_h.GetResponse();
            }
            catch
            {
                Console.WriteLine("ReadDiffTable_GetTable :Cannot Get Response: req_h, " + h_url);
                return null;
            }
            dynamic header;
            using (StreamReader sr = new StreamReader(res_h.GetResponseStream()))
            {
                header = JsonConvert.DeserializeObject(sr.ReadToEnd());
            }
            string d_url_raw = header["data_url"];
            string symbol = header["symbol"];
            symbols[key]= symbol;
            object[] order = new object[0];
            if (header["level_order"] != null)
                order = header["level_order"].ToObject<object[]>();
            string d_url = MergeRelToAbs(root, d_url_raw);

            WebRequest req_d = WebRequest.Create(d_url);
            WebResponse res_d;
            try
            {
                res_d = req_d.GetResponse();
            }
            catch
            {
                Console.WriteLine("ReadDiffTable_GetTable :Cannot Get Response: req_d, " + d_url);
                return null;
            }
            using (StreamReader sr = new StreamReader(res_d.GetResponseStream()))
            {

                string json = sr.ReadToEnd();
                dynamic data = JsonConvert.DeserializeObject(json);
                dynamic obj = new
                {
                    symbol = symbol,
                    order = order,
                    body = data
                };
                string se = JsonConvert.SerializeObject(obj);
                if(fi.Exists)fi.Delete();
                using (StreamWriter sw = new StreamWriter(fi.OpenWrite()))
                {
                    sw.Write(se);
                    sw.Flush();
                }
                Console.WriteLine();
                return obj;
            }
        }

        public static string MergeRelToAbs(string root, string p)
        {
            Match m = Regex.Match(p, @"https?://.*");//absolute
            if (m.Success) return p;
            p.TrimStart('/');
            return root + p;//relative
        }

    }
}
