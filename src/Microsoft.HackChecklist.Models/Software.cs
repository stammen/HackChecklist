using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.HackChecklist.Models.Enums;

namespace Microsoft.HackChecklist.Models
{
    public class Software 
    {
        public string Name { get; set; }

        public string AdditionalInformation { get; set; }

        public CheckType CheckType { get; set; }

        public string InstallationRegistryKey { get; set; }

        public string InstallationRegistryValue { get; set; }

        public string InstallationRegistryExpectedValue { get; set; }

        public bool IsOptional { get; set; }

        public string InstallationNotes { get; set; }

        public string Status { get; set; }

        public bool ActivateLoading { get; set; }
    }            
}
