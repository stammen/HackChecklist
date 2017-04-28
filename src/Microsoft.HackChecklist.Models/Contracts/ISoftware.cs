using Microsoft.HackChecklist.Models.Enums;

namespace Microsoft.HackChecklist.Models.Contracts
{
    public interface ISoftware
    {
        string Name { get; set; }
        string AdditionalInformation { get; set; }
        CheckType CheckType { get; set; }
        string InstallationRegistryKey { get; set; }
        string InstallationRegistryValue { get; set; }
        string InstallationRegistryExpectedValue { get; set; }
        bool IsOptional { get; set; }
        string InstallationNotes { get; set; }
    }
}