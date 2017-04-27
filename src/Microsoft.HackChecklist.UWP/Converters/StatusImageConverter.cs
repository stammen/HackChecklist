using System;
using Windows.UI.Xaml.Data;

namespace Microsoft.HackChecklist.UWP.Converters
{
    public class StatusImageConverter : IValueConverter
    {
        private const string RootPath = "ms-appx:///Assets/";
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var image = value as string;
            return $"{RootPath}{image}.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
