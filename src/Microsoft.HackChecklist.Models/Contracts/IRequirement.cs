using System.Collections.Generic;

namespace Microsoft.HackChecklist.Models.Contracts
{
    public interface IRequirement : ISoftware
    {
        IEnumerable<Requirement> Modules { get; set; }
    }
}