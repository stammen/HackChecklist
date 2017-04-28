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
        public static IEnumerable<string> GetRegistryValues(RegistryHive hive, string subKey, string valueKey)
        {
            return GetRegistryValues(subKey, valueKey, GetRegistryKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                .Union(GetRegistryValues(subKey, valueKey, GetRegistryKey(RegistryHive.LocalMachine, RegistryView.Registry32)));
        }

        public static string GetRegistryValue(RegistryHive hive, string subKey, string valueKey)
        {
            return GetRegistryValue(subKey, valueKey, GetRegistryKey(hive));
        }        

        private static RegistryKey GetRegistryKey(RegistryHive hive)
        {
            return (Environment.Is64BitOperatingSystem)
                ? GetRegistryKey(hive, RegistryView.Registry64)
                : GetRegistryKey(hive, RegistryView.Registry32);
        }

        private static RegistryKey GetRegistryKey(RegistryHive hive, RegistryView view)
        {            
            return RegistryKey.OpenBaseKey(hive, view);
        }

        private static string GetRegistryValue(string subKey, string valueKey, RegistryKey registryKey)
        {
            if (string.IsNullOrEmpty(subKey) || string.IsNullOrEmpty(valueKey)) return null;

            try
            {
                return registryKey?.OpenSubKey(subKey)?.GetValue(valueKey).ToString();
            }
            catch
            {
                return null;
            }
        }

        private static IEnumerable<string> GetRegistryValues(string subKey, string valueKey, RegistryKey registryKey)
        {
            var searchResult = new List<string>();
            if (registryKey == null) return searchResult;

            try
            {
                var key = registryKey.OpenSubKey(subKey);
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
