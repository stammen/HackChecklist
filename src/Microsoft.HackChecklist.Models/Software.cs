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

using System.Collections.Generic;

namespace Microsoft.HackChecklist.Models
{
    public class Software
    {
        public string Name { get; set; }

        public string AdditionalInformation { get; set; }

        public string InstallationRegistryKey { get; set; }

        public string InstallationRegistryValue { get; set; }

        public bool IsOptional { get; set; }

        public string InstallationNotes { get; set; }

        public IEnumerable<Software> Modules { get; set; }
    }            
}
