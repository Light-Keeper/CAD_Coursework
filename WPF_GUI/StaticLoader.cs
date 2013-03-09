using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WPF_GUI
{
    public class StaticLoader
    {
        public static int Exec(string arg)
        {
            MessageBox.Show("hello, " + arg + "!");
            return 0;
        }
    }
}
