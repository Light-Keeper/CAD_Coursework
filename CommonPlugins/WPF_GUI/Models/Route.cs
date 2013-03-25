using System;
using System.Windows.Input;

namespace WPF_GUI.Models
{
    public class Route
    {
        public String Name { get; set; }
        public ICommand Command { get; set; }
        public Boolean IsChecked { get; set; }
    }
}
