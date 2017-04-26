using System;

namespace Microsoft.HackChecklist.SystemChecker
{
    public class Program
    {
        private const int MinimunParametersNumber = 2;

        public static void Main(string[] args)
        {
            new SystemChecker().Run(args[0], args[1]);
            Console.ReadKey();
        }
    }
}
