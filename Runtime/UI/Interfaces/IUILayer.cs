using UnityEngine;

namespace Com.Krackhet.Runtime.UI
{
    public interface IUILayer
    {
        void Init(IUIManager manager, int layerIndex);
        void Show();
        void Hide();
        int GetLayerIndex();
    }
}