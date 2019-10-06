using System.ComponentModel;

namespace Aeon.Internal.UI
{
    public sealed class PluginModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Private Fields

        private string name;
        private string path;
        private bool isRunning;

        #endregion

        public string Name
        {
            get { return name; }
            set
            {
                if (value != null)
                {
                    name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        public string Path
        {
            get { return path; }
            set
            {
                if (value != null)
                {
                    path = value;
                    NotifyPropertyChanged("Path");
                }
            }
        }

        public bool IsRunning
        {
            get { return isRunning; }
            set
            {
                isRunning = value;
                NotifyPropertyChanged("IsRunning");
            }
        }

        private void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
