namespace Com.Krackhet.Runtime.Persistence
{
    /// <summary>
    /// Where the raw save string lives. The user data manager is the only caller,
    /// so swapping PlayerPrefs for a file or cloud backend only needs a new implementation.
    /// </summary>
    public interface IUserDataStorage
    {
        bool Exists(string key);
        string Read(string key);
        void Write(string key, string data);
        void Delete(string key);
    }
}
