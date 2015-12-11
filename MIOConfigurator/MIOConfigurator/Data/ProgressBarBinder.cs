using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIOConfigurator.Data
{
    public class ProgressBarBinder : INotifyPropertyChanged
    {
        public ProgressBarBinder()
        {
            Current = 0;
            Start = 0;
            End = 100;
        }
        private int _current;
        public int Current
        {
            get { return _current; }
            set
            {
                if (value != _current)
                {
                    _current = value;
                    OnPropertyChanged("Current");
                }
            }
        }
        private int _start;
        public int Start
        {
            get { return _start; }
            set
            {
                if (value != _start)
                {
                    _start = value;
                    OnPropertyChanged("Start");
                }
            }
        }

        private int _end;
        public int End
        {
            get { return _end; }
            set
            {
                if (value != _end)
                {
                    _end = value;
                    OnPropertyChanged("End");
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }  
}
