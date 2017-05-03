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

using Microsoft.HackChecklist.Models.Enums;

namespace Microsoft.HackChecklist.Models.Contracts
{
    public interface ISoftware
    {
        string Name { get; set; }
        string AdditionalInformation { get; set; }
        CheckType CheckType { get; set; }        
        bool IsOptional { get; set; }
        string InstallationNotes { get; set; }
    }
}