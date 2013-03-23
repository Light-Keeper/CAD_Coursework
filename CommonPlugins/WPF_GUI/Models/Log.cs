using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WPF_GUI.Models
{
    public class Log
    {
        public string Message { get; set; }
        public DateTime CreateTime { get; set; }
        public string Formated
        {
            get { return "[" + this.CreateTime.ToLongTimeString() + "] - " + this.Message; }
        }
    }
}
