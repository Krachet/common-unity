namespace Com.Krackhet.Runtime.Inventory
{
    /// <summary>
    /// Marker for plug-in data attached to an <see cref="ItemDefinition"/>.
    /// Concrete traits are [Serializable] classes stored by reference on the
    /// definition and queried through ItemDefinition.TryGetTrait.
    /// </summary>
    public interface ITrait
    {
    }
}
