using Com.Krackhet.Runtime.Managers;
using UnityEngine;

namespace Com.Krackhet.Runtime.UI
{
    public interface IUIManager: IGameInternal
    {
        Camera RenderCamera { get; }
        Canvas UICanvas { get; }

        void Initialise();
        void RegisterLayer(IUILayer layer);
        void UnregisterLayer(IUILayer layer);
        void AddLayer(IUILayer layer);
        void RemoveLayer(IUILayer layer);
    }
}