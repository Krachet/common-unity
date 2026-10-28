using System;
using UnityEngine;

namespace Com.Krackhet.Runtime.UI.Components
{
    /// <summary>
    /// Delegate-based virtual scroll grid. Use when you don't want to create a subclass:
    /// <code>
    /// grid.Bind = (rect, index) => rect.GetComponent&lt;MyCell&gt;().Set(items[index]);
    /// grid.SetItemCount(items.Count);
    /// </code>
    /// </summary>
    public class VirtualScrollGrid : VirtualScrollGrid<RectTransform>
    {
        public Action<RectTransform, int> Bind { get; set; }

        public Action<RectTransform> CellCreated { get; set; }

        protected override void BindCell(RectTransform cell, int index)
        {
            Bind?.Invoke(cell, index);
        }

        protected override void OnCellCreated(RectTransform cell)
        {
            CellCreated?.Invoke(cell);
        }
    }
}
