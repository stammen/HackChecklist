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

using Windows.UI.Xaml.Controls;
using Microsoft.HackChecklist.UWP.ViewModels;

namespace Microsoft.HackChecklist.UWP.View
{
    public sealed partial class MainView : Page
    {
        public MainView()
        {
            InitializeComponent();
            IoCConfiguration.Configure();
            DataContext = IoCConfiguration.GetType<MainViewModel>();
        }
    }
}
