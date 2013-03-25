using System;
using System.Windows.Input;

namespace WPF_GUI.Models
{
    public class Place
    {
        public String Name { get; set; }
        public ICommand Command { get; set; }
        public Boolean IsChecked { get; set; }
    }
}
