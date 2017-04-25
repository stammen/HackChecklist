using System.Collections.Generic;

namespace Microsoft.HackChecklist.Models
{
    public class Requirement : Software
    {
        public IEnumerable<Software> Modules { get; set; }
    }
}
