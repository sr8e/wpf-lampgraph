using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace LampGraph
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public Dictionary<string, List<object>[]> bms_o;
        public static RoutedCommand RcOpenlist = new RoutedCommand();

        public MainWindow()
        {
            InitializeComponent();
            bool reg=Register.CheckRegistered();
            if (!reg)
            {
                bool valid = false;
                while (!valid)
                {
                    Console.WriteLine("!!!");
                    valid = Register.SelectFolder();

                    if (!valid)
                    {
                        System.Windows.Forms.MessageBox.Show("Please Select a valid folder!", "Invalid Folder",MessageBoxButtons.OK,MessageBoxIcon.Error);
                    }
                }
                    
                
            }
            ReadDiffTable.ReadTableList();
            ReadDB.ReadSongDB();
            ReadDB.GetPlayer();
            combo_t.ItemsSource = ReadDiffTable.ReturnTableNameList();
            combo_t.SelectedIndex = 0;
            combo_u.ItemsSource = ReadDB.plist;
            combo_u.SelectedIndex = 0;
            combo_d.ItemsSource = new object[] { new { Key="###Default###",Disp="---"} };
            combo_d.SelectedIndex = 0; 
        }
        private void DispGraph(object sender, RoutedEventArgs e)
        {
            string key = ((dynamic)combo_t.SelectedItem).Key;
            if (key == "###Default###") return;
            string u = (string)combo_u.SelectedItem;
            bool fetch = cb_up.IsChecked.Value;

            dynamic g = ReadDB.GenerateGraphData(key, u, fetch);
            Dictionary<string, int[]> graph = g.Graph;
            bms_o = g.Bms;
            
            graphStack.Children.Clear();
            bmsView.Children.Clear();
            string sym = ReadDiffTable.symbols[key];
            List<object> dif = new List<object>();
            int index = 0;
            foreach (KeyValuePair<string, int[]> kvp in graph)
            {
                GraphCC gcc = new GraphCC(index++)
                {
                    Label = sym + kvp.Key,
                    Value = kvp.Value
                };
                graphStack.Children.Add(gcc);
                object obj = new
                {
                    Key = kvp.Key,
                    Disp = sym + kvp.Key
                };
                dif.Add(obj);
            }
            combo_d.ItemsSource = dif;
            combo_d.SelectedIndex = 0;
            ShowBmsList(null,null);
            status.Text = "Done.";
        }

        public static readonly DependencyProperty SidewidthProperty = DependencyProperty.Register
            ("Sidewidth", typeof(int), typeof(MainWindow), new FrameworkPropertyMetadata(300));

        public int Sidewidth
        {
            get { return (int)GetValue(SidewidthProperty); }
            set { SetValue(SidewidthProperty, value); }
        }

        
        private void ShowBmsList(object sender,RoutedEventArgs e)
        {
            dynamic sel = combo_d.SelectedItem;
            if (sel == null) return;
            int c = combo_s.SelectedIndex;
            string d = sel.Key;
            string ds = sel.Disp;
            if (d == "###Default###") return;
            bmsView.Children.Clear(); 
            foreach(dynamic bms in bms_o[d][c])
            {
                string hash = bms.md5;
                string title = bms.title;
                string url = String.Format("http://www.dream-pro.info/~lavalse/LR2IR/search.cgi?bmsmd5={0}", hash);
                string artist = bms.artist;
                Hyperlink h = new Hyperlink
                {
                    NavigateUri = new Uri(url)
                };
                h.Inlines.Add(title);
                h.Click += Link;
                TextBlock tt = new TextBlock
                {
                    Text = string.Format("Artist: {0}\nMD5Hash: {1}", artist, hash),
                };
                TextBlock tb = new TextBlock()
                {
                    ToolTip = tt,
                    FontSize=15,
                    Height = 25,
                    Margin=new Thickness(5,0,0,0)
                };
                tb.Inlines.Add(h);
                bmsView.Children.Add(tb);
            }
        }
        private void Link(object sender,RoutedEventArgs e)
        {
            Uri u = ((Hyperlink)sender).NavigateUri;
            Process.Start(u.OriginalString);
        }

        private void S_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            Key k = e.Key;
            if (k == Key.S)
            {
                Sidewidth = Sidewidth == 0 ? 300 : 0;
            }
        }

        private void OpenList(object sender, ExecutedRoutedEventArgs e)
        {
            string[] param = ((string)e.Parameter).Split('/');
            combo_d.SelectedIndex = int.Parse(param[0]);
            combo_s.SelectedIndex = int.Parse(param[1]);
            
        }

    }

}
