using System.Collections.Generic;
using Microsoft.HackChecklist.Models.Contracts;

namespace Microsoft.HackChecklist.Models
{
    public class Requirement : Software, IRequirement
    {
        public IEnumerable<Requirement> Modules { get; set; }
    }
}
