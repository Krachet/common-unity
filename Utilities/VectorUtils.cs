using System.Collections.Generic;
using UnityEngine;

public static class VectorUtil 
{
    public static float GetTotalLength(Vector3[] points)
    {
        if (points == null || points.Length < 2)
            return 0f;

        float total = 0f;
        for (int i = 1; i < points.Length; i++)
        {
            total += Vector3.Distance(points[i - 1], points[i]);
        }
        return total;
    }

    public static float CalculateLength(List<Vector2> points)
    {
        float length = 0;
        for (int i = 0; i < points.Count - 1; i++)
        {
            length += Vector2.Distance(points[i], points[i + 1]);
        }

        return length;
    }

    public static List<Vector2> GetPointsBetween(Vector2 start, Vector2 end)
    {
        List<Vector2> points = new List<Vector2>();

        Vector2 direction = end - start;
        float distance = direction.magnitude;
        direction.Normalize();

        // Add points 1 unit apart including start and end
        for (float i = 0; i <= distance; i += 1f)
        {
            points.Add(start + direction * i);
        }

        // Ensure end point is included precisely
        if ((points[points.Count - 1] - end).sqrMagnitude > 0.001f)
            points.Add(end);

        return points;
    }

    public static Vector3[] ToVector3Array(List<Vector2> list, float z = 0f)
    {
        Vector3[] result = new Vector3[list.Count];

        for (int i = 0; i < list.Count; i++)
        {
            result[i] = new Vector3(list[i].x, list[i].y, z);
        }

        return result;
    }

    public static bool IsCellAdjacent(Vector2Int cellA, Vector2Int cellB)
    {
        int deltaX = Mathf.Abs(cellA.x - cellB.x);
        int deltaY = Mathf.Abs(cellA.y - cellB.y);

        return (deltaX == 1 && deltaY == 0) || (deltaX == 0 && deltaY == 1);
    }

    /// <summary>
    /// Positions a UI object along a line between two RectTransforms based on a percentage.
    /// Works responsively with anchored positions.
    /// </summary>
    /// <param name="targetObject">The RectTransform to position</param>
    /// <param name="startRect">The starting RectTransform</param>
    /// <param name="endRect">The ending RectTransform</param>
    /// <param name="percent">The percentage along the line (0-1 range)</param>
    public static void PositionAlongLine(RectTransform targetObject, RectTransform startRect, RectTransform endRect, float percent)
    {
        percent = Mathf.Clamp01(percent);
        targetObject.anchoredPosition = Vector2.Lerp(startRect.anchoredPosition, endRect.anchoredPosition, percent);
    }

    /// <summary>
    /// Calculates the anchored position along a line between two RectTransforms based on a percentage.
    /// Returns the position without modifying any object.
    /// </summary>
    /// <param name="startRect">The starting RectTransform</param>
    /// <param name="endRect">The ending RectTransform</param>
    /// <param name="percent">The percentage along the line (0-1 range)</param>
    /// <returns>The calculated anchored position</returns>
    public static Vector2 GetPositionAlongLine(RectTransform startRect, RectTransform endRect, float percent)
    {
        percent = Mathf.Clamp01(percent);
        return Vector2.Lerp(startRect.anchoredPosition, endRect.anchoredPosition, percent);
    }

    /// <summary>
    /// Positions a UI object along the width of a single RectTransform based on a percentage.
    /// Useful for positioning along a slider or progress bar.
    /// </summary>
    /// <param name="targetObject">The RectTransform to position</param>
    /// <param name="lineRect">The RectTransform that defines the line</param>
    /// <param name="percent">The percentage along the line (0-1 range)</param>
    public static void PositionAlongWidth(RectTransform targetObject, RectTransform lineRect, float percent)
    {
        percent = Mathf.Clamp01(percent);
        float width = lineRect.rect.width;
        float xPosition = -width / 2f + (width * percent);
        targetObject.anchoredPosition = new Vector2(xPosition, targetObject.anchoredPosition.y);
    }

    /// <summary>
    /// Positions a UI object along the height of a single RectTransform based on a percentage.
    /// </summary>
    /// <param name="targetObject">The RectTransform to position</param>
    /// <param name="lineRect">The RectTransform that defines the line</param>
    /// <param name="percent">The percentage along the line (0-1 range)</param>
    public static void PositionAlongHeight(RectTransform targetObject, RectTransform lineRect, float percent)
    {
        percent = Mathf.Clamp01(percent);
        float height = lineRect.rect.height;
        float yPosition = -height / 2f + (height * percent);
        targetObject.anchoredPosition = new Vector2(targetObject.anchoredPosition.x, yPosition);
    }

    /// <summary>
    /// Gets the local X position along the width of a RectTransform based on a percentage.
    /// </summary>
    /// <param name="lineRect">The RectTransform that defines the line</param>
    /// <param name="percent">The percentage along the line (0-1 range)</param>
    /// <returns>The calculated X position</returns>
    public static float GetXPositionAlongWidth(RectTransform lineRect, float percent)
    {
        percent = Mathf.Clamp01(percent);
        float width = lineRect.rect.width;
        return -width / 2f + (width * percent);
    }
}
