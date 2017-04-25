using System;

namespace Microsoft.HackChecklist.BackgroundProcess
{
    public class Program
    {
        public static void Main()
        {
            new SystemChecker().Run();
            Console.ReadKey();
        }
    }
}
