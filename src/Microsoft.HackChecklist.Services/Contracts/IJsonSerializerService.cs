namespace Microsoft.HackChecklist.Services.Contracts
{
    public interface IJsonSerializerService
    {
        string Serialize<T>(T data);
        T Deserialize<T>(string strData);
    }
}