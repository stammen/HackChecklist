using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.HackChecklist.UWP.Properties;

namespace Microsoft.HackChecklist.UWP.ViewModels.Base
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
