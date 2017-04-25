using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace Microsoft.HackChecklist.UWP.Services
{
    public class AppDataService
    {
        private const string RootPath = "ms-appx:///";

        public async Task<string> GetDataFile(string fileName)
        {
            var configText = string.Empty;
            try
            {
                var uri = new Uri($"{RootPath}{fileName}.json");

                var configFile = await StorageFile.GetFileFromApplicationUriAsync(uri);
                if (configFile != null)
                {
                    configText = await FileIO.ReadTextAsync(configFile);
                }
            }
            catch (Exception)
            { 
                //Ignore
            }

            return configText;
        }
    }
}
