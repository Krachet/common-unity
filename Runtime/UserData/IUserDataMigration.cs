using Newtonsoft.Json.Linq;

namespace Com.Krackhet.Runtime.Persistence
{
    /// <summary>
    /// Moves the raw save json from <see cref="FromVersion"/> to FromVersion + 1.
    /// </summary>
    /// <example>
    /// Renaming the "levels" section to "levelProgress":
    /// <code>
    /// sealed class Migration_1_to_2 : IUserDataMigration
    /// {
    ///     public int FromVersion => 1;
    ///     public void Migrate(JObject root)
    ///     {
    ///         root["levelProgress"] = root["levels"];
    ///         root.Remove("levels");
    ///     }
    /// }
    /// </code>
    /// </example>
    public interface IUserDataMigration
    {
        int FromVersion { get; }
        void Migrate(JObject root);
    }
}
