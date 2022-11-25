using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;

namespace LampGraph
{
    class ReadDB
    {
        public static Dictionary<string,object[]> bmslist;
        public static List<string> plist;
        public static Dictionary<string,Dictionary<string, object[]>> psclist;

        public static void ReadSongDB()
        {
            var sqlsb = new SQLiteConnectionStringBuilder() { DataSource = Register.LR2filesPath + "\\Database\\song.db" };
            using (SQLiteConnection con = new SQLiteConnection(sqlsb.ToString()))
            {
                con.Open();
                SQLiteCommand c = new SQLiteCommand(con)
                {
                    CommandText = "select * from song"
                };
                SQLiteDataReader r = c.ExecuteReader();
                bmslist = new Dictionary<string,object[]>();
                while(r.Read())
                {
                    string hash = r.GetString(r.GetOrdinal("hash"));
                    object[] bms = new object[r.FieldCount];
                    r.GetValues(bms);
                    try
                    {
                        bmslist.Add(hash, bms);
                        //Console.WriteLine(string.Format("hash:{0},title:{1}", hash, bms[1]));
                    }catch(ArgumentException e)
                    {
                        Console.WriteLine(string.Format("Message:{0},hash:{1}", e.Message, hash));
                    }
                }
            }
        }
        public static void GetPlayer()
        {
            string sfpath = Register.LR2filesPath + "\\Database\\Score";
            IEnumerable e = Directory.EnumerateFiles(sfpath);
            plist = new List<string>();
            psclist = new Dictionary<string, Dictionary<string, object[]>>();
            foreach(string path in e)
            {
                Match m = Regex.Match(path, @".*\\([^\\]+)\.db$");
                if (!m.Success) continue;
                string pn = m.Groups[1].Value;
                plist.Add(pn);
                psclist.Add(pn, new Dictionary<string, object[]>());
                var sqlsb = new SQLiteConnectionStringBuilder() { DataSource = path };
                using(SQLiteConnection con=new SQLiteConnection(sqlsb.ToString()))
                {
                    con.Open();
                    SQLiteCommand c = new SQLiteCommand(con) { CommandText = "select * from score" };
                    SQLiteDataReader r = c.ExecuteReader();
                    while (r.Read())
                    {
                        string hash = r.GetString(r.GetOrdinal("hash"));
                        object[] score = new object[r.FieldCount];
                        r.GetValues(score);
                        psclist[pn].Add(hash, score);
                    }
                }
            }
        }
        

        public static dynamic GenerateGraphData(string key,string player,bool fetch=false)
        {
            dynamic difftable = ReadDiffTable.GetTable(key,fetch);
            ReadDiffTable.symbols[key]= (string)difftable.symbol;
            Dictionary<string,int[]> order = new Dictionary<string, int[]>();
            Dictionary<string, List<object>[]> bms_o = new Dictionary<string, List<object>[]>();
            foreach(object obj in difftable.order)
            {
                string k = obj.ToString();
                order.Add(k, new int[7]);
                bms_o.Add(k, new List<object>[7]);
                for (int i = 0; i < 7; i++)
                    bms_o[k][i] = new List<object>();
            }
            foreach(dynamic bms in difftable.body)
            {
                string hash = bms["md5"];
                string lv = bms["level"];
                object[] b = bmslist.ContainsKey(hash)?bmslist[hash]:null;
                object[] s = psclist[player].ContainsKey(hash) ? psclist[player][hash] : null;
                if (!order.ContainsKey(lv))
                {
                    order[lv] = new int[7];
                    bms_o[lv] = new List<object>[7];
                    for (int i = 0; i < 7; i++)
                        bms_o[lv][i] = new List<object>();
                }
                if (b == null)
                {
                    order[lv][0]++;
                    bms_o[lv][0].Add(bms);
                }
                else
                {
                    if (s == null)
                    {
                        order[lv][1]++;
                        bms_o[lv][1].Add(bms);
                    }
                    else
                    {
                        order[lv][int.Parse(s[1].ToString()) + 1]++;
                        bms_o[lv][int.Parse(s[1].ToString()) + 1].Add(bms);
                    }
                }

            }

            object gd = new
            {
                Graph = order,
                Bms = bms_o
            };



            return gd;
        }
    }
}
