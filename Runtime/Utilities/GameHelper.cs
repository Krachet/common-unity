using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Com.Krackhet.Runtime.Utilities
{
    public static class GameHelper
    {
        #region Public Methods
        /// <summary>
        /// Returns the current local Unix timestamp adjusted for timezone and realtime.
        /// </summary>
        public static long Now()
        {
            long unixTimeSinceStartup = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            double adjustment = Time.realtimeSinceStartupAsDouble
                - TimeZoneInfo.Local.BaseUtcOffset.TotalSeconds;
            return (long)(unixTimeSinceStartup - adjustment + Time.realtimeSinceStartupAsDouble);
        }

        /// <summary>
        /// Returns all non-abstract, non-generic classes deriving from <typeparamref name="T"/>
        /// across all loaded assemblies.
        /// </summary>
        public static Type[] GetAllDerivedClassesOf<T>() where T : class
        {
            Type inheritType = typeof(T);
            List<Type> types = new List<Type>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                foreach (Type type in assembly.GetTypes())
                    if (type.IsClass && !type.IsAbstract && !type.IsGenericTypeDefinition
                        && inheritType.IsAssignableFrom(type))
                        types.Add(type);
            return types.ToArray();
        }

        public static void FitObjectToScreen(this GameObject obj, 
            Camera camera, 
            out float heightDifference,
            out float widthDifference,
            float widthClamp = -1f, 
            float heightClamp = -1f) 
        {
            heightDifference = 1;
            widthDifference = 1;
            obj.transform.position = new Vector3(camera.transform.position.x, camera.transform.position.y, 0);
            float cameraWidth = camera.rect.width;
            float cameraHeight = camera.rect.height;
            Vector3 bottomLeft = camera.ViewportToWorldPoint(Vector3.zero);
            Vector3 topRight = camera.ViewportToWorldPoint(new Vector3(cameraWidth, cameraHeight));
            Vector3 screenSize = topRight - bottomLeft;
            Vector3 screenSizeClamped = new Vector3(
                widthClamp > 0 ? Mathf.Min(screenSize.x, widthClamp) : screenSize.x,
                heightClamp > 0 ? Mathf.Min(screenSize.y, heightClamp) : screenSize.y,
                1f
            );
            float screenRatio = screenSizeClamped.x / screenSizeClamped.y;
            float desiredRatio = obj.transform.localScale.x / obj.transform.localScale.y;

            if (screenRatio > desiredRatio)
            {
                float height = screenSize.y;
                widthDifference = height * desiredRatio / screenSize.x;
                obj.transform.localScale = new Vector3(height * desiredRatio, height);
            }
            else
            {
                float width = screenSize.x;
                heightDifference = width / desiredRatio / screenSize.y;
                obj.transform.localScale = new Vector3(width, width / desiredRatio);
            }
        }

        public static void FitObjectToRect(this GameObject obj, 
            Camera camera,
            Rect rectBound, 
            out float heightDifference,
            out float widthDifference,
            float widthClamp = -1f, 
            float heightClamp = -1f) 
        {
            heightDifference = 1;
            widthDifference = 1;
            obj.transform.position = camera.ViewportToWorldPoint(rectBound.center);
            float cameraWidth = rectBound.width;
            float cameraHeight = rectBound.height;
            Vector3 bottomLeft = camera.ViewportToWorldPoint(Vector3.zero);
            Vector3 topRight = camera.ViewportToWorldPoint(new Vector3(cameraWidth, cameraHeight));
            Vector3 screenSize = topRight - bottomLeft;
            Vector3 screenSizeClamped = new Vector3(
                widthClamp > 0 ? Mathf.Min(screenSize.x, widthClamp) : screenSize.x,
                heightClamp > 0 ? Mathf.Min(screenSize.y, heightClamp) : screenSize.y,
                1f
            );
            float screenRatio = screenSizeClamped.x / screenSizeClamped.y;
            float desiredRatio = obj.transform.localScale.x / obj.transform.localScale.y;

            if (screenRatio > desiredRatio)
            {
                float height = screenSize.y;
                widthDifference = height * desiredRatio / screenSize.x;
                obj.transform.localScale = new Vector3(height * desiredRatio, height);
            }
            else
            {
                float width = screenSize.x;
                heightDifference = width / desiredRatio / screenSize.y;
                obj.transform.localScale = new Vector3(width, width / desiredRatio);
            }
        }

        /// <summary>
        /// Returns a world-space position along a LineRenderer based on a percentage
        /// (0.0 to 1.0) of the total polyline length.
        /// </summary>
        public static Vector3 GetPositionOnLineRenderer(
            this LineRenderer lineRenderer, float percentage)
        {
            if (lineRenderer == null)
            {
                Debug.LogError("[GameHelper] GetPositionOnLineRenderer: LineRenderer is null.");
                return Vector3.zero;
            }

            percentage = Mathf.Clamp01(percentage);
            int positionCount = lineRenderer.positionCount;
            if (positionCount == 0) return Vector3.zero;
            if (positionCount == 1)
                return TransformPosition(lineRenderer, lineRenderer.GetPosition(0));

            Vector3[] positions = new Vector3[positionCount];
            lineRenderer.GetPositions(positions);

            // Convert to world space when useWorldSpace is disabled
            if (!lineRenderer.useWorldSpace)
            {
                Transform lrTransform = lineRenderer.transform;
                for (int i = 0; i < positions.Length; i++)
                    positions[i] = lrTransform.TransformPoint(positions[i]);
            }

            float totalLength = 0f;
            float[] segmentLengths = new float[positionCount - 1];
            for (int i = 0; i < positionCount - 1; i++)
            {
                segmentLengths[i] = Vector3.Distance(positions[i], positions[i + 1]);
                totalLength += segmentLengths[i];
            }

            if (totalLength <= 0f)
                return positions[0];

            float targetDistance = percentage * totalLength;
            float accumulated = 0f;

            for (int i = 0; i < segmentLengths.Length; i++)
            {
                float segLen = segmentLengths[i];
                if (targetDistance <= accumulated + segLen)
                {
                    float t = (targetDistance - accumulated) / segLen;
                    return Vector3.Lerp(positions[i], positions[i + 1], t);
                }
                accumulated += segLen;
            }

            return positions[positionCount - 1];
        }
        #endregion

        #region Private Methods
        private static Vector3 TransformPosition(
            LineRenderer lineRenderer, Vector3 localPosition)
        {
            return lineRenderer.useWorldSpace
                ? localPosition
                : lineRenderer.transform.TransformPoint(localPosition);
        }
        #endregion
    }
}
