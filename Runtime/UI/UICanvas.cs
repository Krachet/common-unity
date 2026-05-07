using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Com.Krackhet.Runtime.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Com.Krackhet.Runtime.UI
{
    public class UICanvas : MonoBehaviour
    {
#if UNITY_EDITOR
        [MenuItem("GameObject/UI (External)/UICanvas")]
#endif
        public static Canvas InstantiateUICanvas()
        {
            GameObject canvasObject = new GameObject("UICanvas");
            canvasObject.layer = 5;
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.sortingOrder = 2;
            canvas.worldCamera = InstantiateRenderCamera();
            canvas.planeDistance = 10;
            canvas.sortingLayerName = "UI";
            canvas.vertexColorAlwaysGammaSpace = true;
            CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1080, 1920);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f;
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject(typeof(EventSystem).Name);
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
            }
            canvasObject.AddComponent<GraphicRaycaster>();
            canvasObject.AddComponent<UICanvas>();
            return canvas;
        }
        private static Camera InstantiateRenderCamera()
        {
            Camera mainCamera = Camera.main;
            GameObject canvasObject = new GameObject("UICamera");
            Camera camera = canvasObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Depth;
            camera.cullingMask = 0;
            camera.SetCullingMask("UI", 6);
            camera.allowMSAA = false;
            camera.orthographic = true;
            camera.depth = mainCamera.depth + 1;
            camera.orthographicSize = 5;
            canvasObject.transform.position = Vector3.back * 10f;
            return camera;
        }
    }
}