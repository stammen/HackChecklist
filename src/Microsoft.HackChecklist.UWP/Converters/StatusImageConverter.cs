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
