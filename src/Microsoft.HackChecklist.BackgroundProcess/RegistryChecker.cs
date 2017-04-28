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
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;

namespace Microsoft.HackChecklist.BackgroundProcess
{
    public static class RegistryChecker
    {
        public static string GetLocalRegistryValue(string subKey, string valueKey)
        {
            return GetLocalRegistryValue(subKey, valueKey, GetLocalRegistryKey());
        }

        public static IEnumerable<string> GetLocalRegistryValues(string subKey, string valueKey)
        {            
            return GetLocalRegistryValues(subKey, valueKey, GetLocalRegistryKey(RegistryView.Registry64))
                .Union(GetLocalRegistryValues(subKey, valueKey, GetLocalRegistryKey(RegistryView.Registry32)));
        }

        private static string GetLocalRegistryValue(string subKey, string valueKey, RegistryKey localKey)
        {
            if (string.IsNullOrEmpty(subKey) || string.IsNullOrEmpty(valueKey)) return null;

            try
            {                
                return localKey?.OpenSubKey(subKey)?.GetValue(valueKey).ToString();
            }
            catch
            {
                return null;
            }
        }

        private static RegistryKey GetLocalRegistryKey()
        {
            return (Environment.Is64BitOperatingSystem)
                ? GetLocalRegistryKey(RegistryView.Registry64)
                : GetLocalRegistryKey(RegistryView.Registry32);
        }

        private static RegistryKey GetLocalRegistryKey(RegistryView view)
        {
            return RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
        }

        private static IEnumerable<string> GetLocalRegistryValues(string subKey, string valueKey, RegistryKey localKey)
        {
            var searchResult = new List<string>();
            if (localKey == null) return searchResult;

            try
            {
                var key = localKey.OpenSubKey(subKey);
                if (key != null)
                {
                    foreach (var subkey in key.GetSubKeyNames().Select(keyName => key.OpenSubKey(keyName)))
                    {
                        var value = subkey.GetValue(valueKey) as string;
                        if (value != null)
                        {
                            searchResult.Add(value);
                        }
                    }
                    key.Close();
                }
                return searchResult;
            }
            catch
            {
                return searchResult;
            }
        }
    }
}
