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
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.HackChecklist.UWP.Contracts;

namespace Microsoft.HackChecklist.UWP.Services
{
    public class AppDataService : IAppDataService
    {
        private const string RootPath = "ms-appx:///";

        public async Task<string> GetDataFile(string fileName)
        {
            var configText = string.Empty;
            try
            {
                var uri = new Uri($"{RootPath}{fileName}.json");

                var configFile = await StorageFile.GetFileFromApplicationUriAsync(uri);
                if (configFile != null)
                {
                    configText = await FileIO.ReadTextAsync(configFile);
                }
            }
            catch (Exception)
            { 
                //Ignore
            }

            return configText;
        }
    }
}
