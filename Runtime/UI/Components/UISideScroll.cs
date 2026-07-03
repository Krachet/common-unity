using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Krackhet.Runtime.UI
{
    public class UISideScroll : MonoBehaviour
    {
        #region Events Fields
        public event Action<int> onScrollIndexChanged;
        public event Action<int> onScrollStarted;
        public event Action<int> onScrollEnded;
        #endregion

        #region Interfaces Fields
        public List<RectTransform> panels => _panels;
        #endregion

        #region Serialize Fields
        [SerializeField]
        private float _velocityTrigger = 20f;

        [SerializeField]
        private float _snapSpeed = 10f;

        [SerializeField]
        private ScrollRect _scrollRect;

        [SerializeField]
        private List<RectTransform> _panels;
        #endregion

        #region Private Fields
        private int _scrollIndex;
        private float _scrollTarget;
        private bool _isScrolling;
        private ScrollerState _state;
        #endregion

        #region Unity Methods
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _isScrolling = true;
                onScrollStarted?.Invoke(_scrollIndex);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                _isScrolling = false;
                onScrollEnded?.Invoke(_scrollIndex);
            }

            switch (_state)
            {
                case ScrollerState.Scrolling:
                    HandleScrollingState();
                    break;
                case ScrollerState.Snapping:
                    HandleSnappingState();
                    break;
            }
        }
        #endregion

        private void HandleScrollingState()
        {
            float scrollValue = _scrollRect.normalizedPosition.x;
            int index = Mathf.RoundToInt(scrollValue * (panels.Count - 1));
            index = Mathf.Clamp(index, 0, panels.Count - 1);
            _scrollTarget = (float)index / (panels.Count - 1);
            if (!_isScrolling && (index != _scrollIndex || _scrollRect.velocity.magnitude < _velocityTrigger))
            {
                onScrollIndexChanged?.Invoke(index);
                _state = ScrollerState.Snapping;
                _scrollIndex = index;
            }
        }

        private void HandleSnappingState()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _state = ScrollerState.Scrolling;
            }
            SnappingToScrollTarget();
        }

        private void SnappingToScrollTarget()
        {
            float targetPosition = Mathf.Lerp(
                _scrollRect.normalizedPosition.x,
                _scrollTarget,
                Time.deltaTime * 10.0f
            );
            _scrollRect.normalizedPosition = new Vector2(targetPosition, 0);
        }
    }

    public enum ScrollerState
    {
        Scrolling,
        Snapping
    }
}
