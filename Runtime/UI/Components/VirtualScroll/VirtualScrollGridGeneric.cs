using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Com.Krackhet.Runtime.UI.Components
{
    /// <summary>
    /// Pooled (virtualized) vertical grid for a <see cref="ScrollRect"/>.
    /// Only the visible rows (+ buffer rows) are instantiated and re-bound while scrolling.
    /// <para>Setup:</para>
    /// <list type="number">
    /// <item>ScrollRect + Viewport + Content. Add a <see cref="GridLayoutGroup"/> on Content and set
    /// cell size, spacing, padding, column count and child alignment there (it is only read, then disabled).</item>
    /// <item>Create a cell prefab with your cell component.</item>
    /// <item>Subclass: <c>class MyView : VirtualScrollGrid&lt;MyCell&gt; { protected override void BindCell(MyCell c, int i) =&gt; c.Set(_items[i]); }</c></item>
    /// <item>Call <see cref="SetItemCount"/> when data is ready, <see cref="RefreshVisible"/> when data changes.</item>
    /// </list>
    /// Cells implementing <see cref="IVirtualScrollCell"/> are notified when recycled.
    /// </summary>
    public abstract class VirtualScrollGrid<TCell> : MonoBehaviour where TCell : Component
    {
        #region Serialized Fields

        [Header("Scroll")]
        [SerializeField, FormerlySerializedAs("_levelViewRect")]
        private ScrollRect _scrollRect;

        [SerializeField, FormerlySerializedAs("_levelContainer"), Tooltip("Defaults to ScrollRect.content")]
        private RectTransform _content;

        [SerializeField, FormerlySerializedAs("_viewPort"), Tooltip("Defaults to ScrollRect.viewport")]
        private RectTransform _viewport;

        [SerializeField, Min(0)]
        private int _bufferRows = 3;

        [SerializeField, Tooltip("Optional slider synced with the vertical scroll (0 = top, 1 = bottom)")]
        private Slider _scrollSlider;

        [Header("Cells")]
        [SerializeField, FormerlySerializedAs("_selectorPrefab")]
        private TCell _cellPrefab;

        [SerializeField, Tooltip("Layout source (cell size, spacing, padding, columns, alignment)")]
        private GridLayoutGroup _layoutGroup;

        #endregion

        #region Private Fields

        private readonly List<TCell> _cells = new List<TCell>();
        private readonly List<int> _boundIndices = new List<int>();

        private Slider _boundSlider;
        private Vector2 _cellSize;
        private Vector2 _spacing;
        private TextAnchor _childAlignment;
        private RectOffset _padding;
        private float _contentHeight;
        private float _rowStride;
        private int _columnCount = 1;
        private int _itemCount;
        private int _totalRows;
        private int _pooledRows;
        private int _firstBoundRow = -1;
        private bool _isInitialized;
        private bool _isRefreshingLayout;

        #endregion

        #region Properties

        public int ItemCount => _itemCount;

        public ScrollRect ScrollRect => _scrollRect;

        public RectTransform Content => _content != null ? _content : _scrollRect.content;

        public RectTransform Viewport
        {
            get
            {
                if (_viewport != null)
                {
                    return _viewport;
                }

                return _scrollRect.viewport != null
                    ? _scrollRect.viewport
                    : (RectTransform)_scrollRect.transform;
            }
        }

        /// <summary>
        /// Slider synced with the scroll position. Override to provide it from another component.
        /// </summary>
        protected virtual Slider ScrollSlider => _scrollSlider;

        #endregion

        #region Unity Callbacks

        protected virtual void Awake()
        {
            _scrollRect.onValueChanged.AddListener(HandleScrollValueChanged);

            _boundSlider = ScrollSlider;
            if (_boundSlider != null)
            {
                _boundSlider.onValueChanged.AddListener(HandleSliderValueChanged);
            }
        }

        protected virtual void OnRectTransformDimensionsChange()
        {
            if (!_isInitialized || _isRefreshingLayout)
            {
                return;
            }

            RefreshLayout(false);
        }

        protected virtual void OnDestroy()
        {
            if (_scrollRect != null)
            {
                _scrollRect.onValueChanged.RemoveListener(HandleScrollValueChanged);
            }

            if (_boundSlider != null)
            {
                _boundSlider.onValueChanged.RemoveListener(HandleSliderValueChanged);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the number of items and rebuilds the layout. Use to initialize or when the count changes.
        /// </summary>
        public void SetItemCount(int count, bool resetScroll = true)
        {
            _itemCount = Mathf.Max(0, count);

            Canvas.ForceUpdateCanvases();
            RefreshLayout(resetScroll);
            _isInitialized = true;
        }

        /// <summary>
        /// Re-binds every visible cell. Use when item data changed but the count did not.
        /// </summary>
        public void RefreshVisible()
        {
            if (!_isInitialized)
            {
                return;
            }

            _firstBoundRow = -1;
            RefreshVisibleCells();
        }

        /// <summary>
        /// Re-binds a single item if it is currently bound to a cell.
        /// </summary>
        public void RefreshItem(int index)
        {
            if (!_isInitialized || _firstBoundRow < 0)
            {
                return;
            }

            int poolIndex = index - (_firstBoundRow * _columnCount);
            if (poolIndex < 0 || poolIndex >= _cells.Count || _boundIndices[poolIndex] != index)
            {
                return;
            }

            BindCell(_cells[poolIndex], index);
        }

        public void ScrollToIndex(int index, bool center = false)
        {
            if (!_isInitialized || _itemCount == 0)
            {
                return;
            }

            float maximumOffset = GetMaximumScrollOffset();
            if (maximumOffset <= 0f)
            {
                return;
            }

            int row = Mathf.Clamp(index, 0, _itemCount - 1) / _columnCount;
            float offset = _padding.top + (row * _rowStride);
            if (center)
            {
                offset -= (Viewport.rect.height - _cellSize.y) * 0.5f;
            }

            _scrollRect.StopMovement();
            _scrollRect.verticalNormalizedPosition = 1f - Mathf.Clamp01(offset / maximumOffset);
        }

        public void ScrollToTop()
        {
            _scrollRect.StopMovement();
            _scrollRect.verticalNormalizedPosition = 1f;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Binds item data at <paramref name="index"/> to a pooled <paramref name="cell"/>.
        /// </summary>
        protected abstract void BindCell(TCell cell, int index);

        /// <summary>
        /// Called once when a new cell is instantiated in the pool.
        /// </summary>
        protected virtual void OnCellCreated(TCell cell)
        {
        }

        #endregion

        #region Private Methods

        private void RefreshLayout(bool resetScrollPosition)
        {
            if (_isRefreshingLayout)
            {
                return;
            }

            _isRefreshingLayout = true;

            CacheLayoutSettings();
            ResizeContent();
            Canvas.ForceUpdateCanvases();
            EnsurePoolSize(CalculatePoolSize());

            if (resetScrollPosition)
            {
                ScrollToTop();
            }

            _firstBoundRow = -1;
            RefreshVisibleCells();
            UpdateSlider(_scrollRect.verticalNormalizedPosition);

            _isRefreshingLayout = false;
        }

        private void CacheLayoutSettings()
        {
            _layoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            _layoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
            _layoutGroup.startCorner = GridLayoutGroup.Corner.UpperLeft;

            _columnCount = Mathf.Max(1, _layoutGroup.constraintCount);
            _cellSize = _layoutGroup.cellSize;
            _spacing = _layoutGroup.spacing;
            _childAlignment = _layoutGroup.childAlignment;
            _padding = _layoutGroup.padding;
            _rowStride = Mathf.Max(1f, _cellSize.y + _spacing.y);
            _totalRows = Mathf.CeilToInt((float)_itemCount / _columnCount);

            _layoutGroup.enabled = false;

            if (Content.TryGetComponent(out ContentSizeFitter contentSizeFitter))
            {
                contentSizeFitter.enabled = false;
            }
        }

        private void ResizeContent()
        {
            float rowsHeight = _totalRows * _cellSize.y;
            float spacingHeight = Mathf.Max(0, _totalRows - 1) * _spacing.y;
            float requiredHeight = _padding.top + _padding.bottom + rowsHeight + spacingHeight;

            _contentHeight = Mathf.Max(Viewport.rect.height, requiredHeight);
            Content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _contentHeight);
        }

        private int CalculatePoolSize()
        {
            int visibleRows = Mathf.Max(1, Mathf.CeilToInt(Viewport.rect.height / _rowStride));

            _pooledRows = Mathf.Min(_totalRows, visibleRows + (_bufferRows * 2));
            return Mathf.Min(_itemCount, _pooledRows * _columnCount);
        }

        private void EnsurePoolSize(int requiredPoolSize)
        {
            while (_cells.Count < requiredPoolSize)
            {
                TCell cell = Instantiate(_cellPrefab, Content);
                ConfigureCellRect((RectTransform)cell.transform);
                _cells.Add(cell);
                _boundIndices.Add(-1);
                OnCellCreated(cell);
            }

            for (int i = 0; i < _cells.Count; i++)
            {
                ((RectTransform)_cells[i].transform).sizeDelta = _cellSize;

                if (i >= requiredPoolSize)
                {
                    HideCell(i);
                }
            }
        }

        private void ConfigureCellRect(RectTransform cellRect)
        {
            cellRect.anchorMin = new Vector2(0f, 1f);
            cellRect.anchorMax = new Vector2(0f, 1f);
            cellRect.pivot = new Vector2(0.5f, 0.5f);
            cellRect.sizeDelta = _cellSize;
        }

        private void RefreshVisibleCells()
        {
            if (_itemCount == 0 || _cells.Count == 0)
            {
                for (int i = 0; i < _cells.Count; i++)
                {
                    HideCell(i);
                }

                return;
            }

            int firstVisibleRow = CalculateFirstVisibleRow();
            int maximumFirstRow = Mathf.Max(0, _totalRows - _pooledRows);
            int firstPoolRow = Mathf.Clamp(firstVisibleRow - _bufferRows, 0, maximumFirstRow);

            if (firstPoolRow == _firstBoundRow)
            {
                return;
            }

            _firstBoundRow = firstPoolRow;
            BindPool(firstPoolRow * _columnCount);
        }

        private float GetMaximumScrollOffset()
        {
            return Mathf.Max(0f, _contentHeight - Viewport.rect.height);
        }

        private int CalculateFirstVisibleRow()
        {
            float normalizedPosition = Mathf.Clamp01(_scrollRect.verticalNormalizedPosition);
            float scrollOffset = (1f - normalizedPosition) * GetMaximumScrollOffset();
            float rowOffset = Mathf.Max(0f, scrollOffset - _padding.top);

            return Mathf.Clamp(
                Mathf.FloorToInt(rowOffset / _rowStride),
                0,
                Mathf.Max(0, _totalRows - 1)
            );
        }

        private void BindPool(int firstItemIndex)
        {
            int poolSize = Mathf.Min(_cells.Count, _pooledRows * _columnCount);

            for (int poolIndex = 0; poolIndex < _cells.Count; poolIndex++)
            {
                int itemIndex = firstItemIndex + poolIndex;
                if (poolIndex >= poolSize || itemIndex >= _itemCount)
                {
                    HideCell(poolIndex);
                    continue;
                }

                TCell cell = _cells[poolIndex];
                if (_boundIndices[poolIndex] >= 0 && _boundIndices[poolIndex] != itemIndex)
                {
                    NotifyRecycled(cell);
                }

                _boundIndices[poolIndex] = itemIndex;
                cell.gameObject.SetActive(true);
                PositionCell((RectTransform)cell.transform, itemIndex);
                BindCell(cell, itemIndex);
            }
        }

        private void HideCell(int poolIndex)
        {
            TCell cell = _cells[poolIndex];
            if (_boundIndices[poolIndex] >= 0)
            {
                NotifyRecycled(cell);
                _boundIndices[poolIndex] = -1;
            }

            cell.gameObject.SetActive(false);
        }

        private static void NotifyRecycled(TCell cell)
        {
            if (cell is IVirtualScrollCell recyclable)
            {
                recyclable.OnRecycled();
            }
        }

        private void PositionCell(RectTransform cellRect, int itemIndex)
        {
            int row = itemIndex / _columnCount;
            int column = itemIndex % _columnCount;
            float positionX = CalculateHorizontalAlignmentOffset() + _padding.left + (_cellSize.x * 0.5f);
            float positionY = _padding.top + (_cellSize.y * 0.5f);

            positionX += column * (_cellSize.x + _spacing.x);
            positionY += row * _rowStride;
            cellRect.anchoredPosition = new Vector2(positionX, -positionY);
        }

        private float CalculateHorizontalAlignmentOffset()
        {
            float gridWidth = (_columnCount * _cellSize.x) +
                              (Mathf.Max(0, _columnCount - 1) * _spacing.x);
            float availableWidth = Content.rect.width - _padding.left - _padding.right;
            float remainingWidth = availableWidth - gridWidth;

            switch (_childAlignment)
            {
                case TextAnchor.UpperCenter:
                case TextAnchor.MiddleCenter:
                case TextAnchor.LowerCenter:
                    return remainingWidth * 0.5f;

                case TextAnchor.UpperRight:
                case TextAnchor.MiddleRight:
                case TextAnchor.LowerRight:
                    return remainingWidth;

                default:
                    return 0f;
            }
        }

        private void HandleScrollValueChanged(Vector2 normalizedPosition)
        {
            UpdateSlider(normalizedPosition.y);
            RefreshVisibleCells();
        }

        private void HandleSliderValueChanged(float value)
        {
            _scrollRect.verticalNormalizedPosition = 1f - Mathf.Clamp01(value);
        }

        private void UpdateSlider(float verticalNormalizedPosition)
        {
            if (_boundSlider == null)
            {
                return;
            }

            _boundSlider.SetValueWithoutNotify(1f - Mathf.Clamp01(verticalNormalizedPosition));
            _boundSlider.interactable = _contentHeight > Viewport.rect.height;
        }

        #endregion
    }
}
