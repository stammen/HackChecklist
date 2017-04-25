using System.Collections.Generic;
using System.ComponentModel;

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
    }            
}
