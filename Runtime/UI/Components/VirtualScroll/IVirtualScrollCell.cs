namespace Com.Krackhet.Runtime.UI.Components
{
    /// <summary>
    /// Optional interface for cells used by <see cref="VirtualScrollGrid{TCell}"/>.
    /// Called when a pooled cell is about to be re-bound to another index or hidden.
    /// </summary>
    public interface IVirtualScrollCell
    {
        void OnRecycled();
    }
}
