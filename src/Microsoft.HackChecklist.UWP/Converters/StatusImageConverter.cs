﻿using System;
using Windows.UI.Xaml.Data;
using Microsoft.HackChecklist.Models.Enums;

namespace Microsoft.HackChecklist.UWP.Converters
{
    public class StatusImageConverter : IValueConverter
    {
        private const string RootPath = "ms-appx:///Assets/";

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ResponseStatus)
            {
                return $"{RootPath}{(ResponseStatus)value}.png";
            }
            return $"{RootPath}{ResponseStatus.None}.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
