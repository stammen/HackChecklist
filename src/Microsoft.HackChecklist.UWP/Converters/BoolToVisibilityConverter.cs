//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Microsoft.HackChecklist.UWP.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public bool Inverse { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var visibility = value != null && value is bool && (bool)value ? Visibility.Visible : Visibility.Collapsed;
            return Inverse ? Invert(visibility) : visibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        private Visibility Invert(Visibility visibility)
        {
            return visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
