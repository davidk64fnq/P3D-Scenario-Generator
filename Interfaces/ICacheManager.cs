namespace P3D_Scenario_Generator.Interfaces
{
    public interface ICacheManager
    {
        T DeserializeFromFile<T>(string filePath);
        void SerializeToFile<T>(T obj, string filePath);
        Task<T> DeserializeFromFileAsync<T>(string filePath);
        Task SerializeToFileAsync<T>(T obj, string filePath);
    }
}