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

namespace LampGraph
{

    public class GraphCC : Control
    {
        private int diffIndex;
        public GraphCC(int diffIndex){
            Value = new int[] { 17, 6, 1, 32, 2, 2, 0 };
            this.diffIndex = diffIndex;
        }
        static GraphCC()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GraphCC), new FrameworkPropertyMetadata(typeof(GraphCC)));
        }
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register
            ("Label", typeof(string), typeof(GraphCC), new FrameworkPropertyMetadata("★7"));
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register
            ("Value", typeof(int[]), typeof(GraphCC), new FrameworkPropertyMetadata());

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }
        public int[] Value
        {
            get { return (int[])GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public string[] GraphWidth
        {
            get
            {
                string[] w = new string[7];
                for (int i = 0; i < 7; i++)
                    w[i] = String.Format("{0}*", Value[i]);
                return w;
            }
        }
        public string[] ComParam {
            get
            {
                string[] p = new string[7];
                for (int i = 0; i < 7; i++)
                    p[i] = string.Format("{0}/{1}", diffIndex, i);
                return p;
            }
        }

    }
}
