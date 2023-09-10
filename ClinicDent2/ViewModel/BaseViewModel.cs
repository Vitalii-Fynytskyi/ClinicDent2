using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ClinicDent2.ViewModel
{
    public enum ViewModelStatus:byte
    {
        NotChanged = 0,
        Updated = 1,
        Inserted = 2,
        Deleted = 3
    }
    public class BaseViewModel : INotifyPropertyChanged
    {
        public ViewModelStatus ViewModelStatus { get; set; } = ViewModelStatus.NotChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void NotifyPropertyChanged(params string[] propertyNames)
        {
            if (PropertyChanged != null)
            {
                foreach (string propertyName in propertyNames)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }
        public virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
