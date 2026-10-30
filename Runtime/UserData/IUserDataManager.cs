namespace Com.Krackhet.Runtime.Persistence
{
    /// <summary>Non-generic view of a user data manager, used by editor tools.</summary>
    public interface IUserDataManager
    {
        string Key { get; }
        IUserDataStorage Storage { get; }
        bool IsLoaded { get; }
        string ToJson();
        void DeleteAll();
    }
}
