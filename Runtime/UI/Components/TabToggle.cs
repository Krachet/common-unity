using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Krackhet.Runtime.UI.Components
{
    public class TabToggle : MonoBehaviour
    {
        protected int _tabIndex;
        protected bool _isSelected;
        [SerializeField] protected UIButton tabBtn;

        protected virtual void OnSelect()
        {
            _isSelected = true;
        }

        protected virtual void OnDeselect()
        {
            _isSelected = false;
        }

        protected virtual void OnReset()
        {
            _isSelected = false;
        }

        public virtual void Select(int index)
        {
            if (index == _tabIndex) OnSelect();
            else OnDeselect();
        }

        public virtual void Init(int index, System.Action<int> onTabSelected)
        {
            _tabIndex = index;
            tabBtn.onClick.AddListener(() => onTabSelected?.Invoke(_tabIndex));

            OnReset();
        }
    }
}
