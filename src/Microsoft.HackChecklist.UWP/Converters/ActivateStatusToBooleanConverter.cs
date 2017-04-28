using System;
using Windows.UI.Xaml.Data;
using Microsoft.HackChecklist.Models.Enums;

namespace Microsoft.HackChecklist.UWP.Converters
{
    public class ActivateStatusToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is ResponseStatus && (ResponseStatus)value == ResponseStatus.Processing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
