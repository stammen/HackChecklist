namespace Microsoft.HackChecklist.BackgroundProcess
{
    public class Program
    {
        private const int MinimunParametersNumber = 2;

        public static void Main(string[] args)
        {
            new SystemChecker().Run();
        }
    }
}
