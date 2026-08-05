using Com.Krackhet.Runtime.Managers;
using UnityEngine;

namespace Com.Krackhet.Runtime.UI
{
    public interface IUIManager
    {
        Camera RenderCamera { get; }
        Canvas UICanvas { get; }
        UIManagerStatus Status { get; }

        void Initialize();
        void RegisterLayer(IUILayer layer);
        void UnregisterLayer(IUILayer layer);
        void AddLayer(IUILayer layer);
        void RemoveLayer(IUILayer layer);
    }
}