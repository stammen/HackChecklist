using System.Threading.Tasks;

namespace Microsoft.HackChecklist.UWP.Contracts
{
    public interface IAppDataService
    {
        Task<string> GetDataFile(string fileName);
    }
}