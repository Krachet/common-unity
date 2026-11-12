namespace Com.Krackhet.Runtime.Save
{
    public interface ISaveService
    {
        bool Has(string key);

        void Save<T>(string key, T data);

        bool TryLoad<T>(string key, out T data);

        void Delete(string key);

        void Flush();
    }
}
