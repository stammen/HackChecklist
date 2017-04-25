using Windows.UI.Xaml.Controls;
using Microsoft.HackChecklist.UWP.Contracts;

namespace Microsoft.HackChecklist.UWP.View
{
    public sealed partial class MainView : Page
    {
        public MainView()
        {
            InitializeComponent();
            IoCConfiguration.Init();
            DataContext = IoCConfiguration.GetType<IMainViewModel>();
        }
    }
}
